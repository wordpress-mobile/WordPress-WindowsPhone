using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace WordPress.Model
{
    public abstract class XmlRemoteProcedureCall<T> where T : INotifyPropertyChanged
    {
        //DEV NOTE:implement the event-based Asynchronous Pattern here so we can
        //create these objects on threads (specifically the main/UI thread)
        //and react to the events on the original without needing to use callbacks

        //DEV NOTE: based on walkthrough found here: http://msdn.microsoft.com/en-us/library/bz33kx67.aspx

        #region member variables

        private static object _syncRoot = new object();

        private SendOrPostCallback onProgressReportDelegate;
        private SendOrPostCallback onCompletedDelegate;
        private Dictionary<object, object> userStateToLifetime = new Dictionary<object, object>();

        //constants for parsing fault codes from the rpc response
        private const string FAULTCODE_VALUE = "faultCode";
        private const string FAULTSTRING_VALUE = "faultString";

        //constants for the HttpWebRequest
        private const string REQUESTCONTENTTYPE_VALUE = "text/xml";
        private const string REQUESTMETHOD_VALUE = "post";
        private const string REQUESTUSERAGENT_VALUE = "wp7/wordpress 1.0";

        #endregion

        #region events

        /// <summary>
        /// Notifies subscribers of changes in progress.  Percentage values are as follows:
        /// <list type="bullet">
        /// <item>20: the parameters have been validated and the HttpWebRequest.BeginGetRequestStream method has been invoked</item>
        /// <item>40: the content of the HttpWebRequest has successfully been generated and the HttpWebRequest.BeginGetResponse method has been invoked</item>
        /// <item>60: a response has been received from the web server</item>
        /// <item>80: the response from the web server has successfully been read and parsing will begin</item>
        /// </list>
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        /// <summary>
        /// Notifies subscribers that asynchronous operation has been completed.  This event does not signify that the task
        /// successfully executed; the XMLRPCCompletedEventHandler.Exception should be examined to determine if the task
        /// was successful.
        /// </summary>
        public event XMLRPCCompletedEventHandler<T> Completed;

        #endregion

        #region private type declarations

        private delegate void WorkerEventHandler(AsyncOperation asyncOp);

        /// <summary>
        /// Utility class passed to async methods
        /// </summary>
        private class State
        {
            public AsyncOperation Operation { get; set; }
            public HttpWebRequest Request { get; set; }
        }

        #endregion

        #region constructors

        public XmlRemoteProcedureCall()
        {
            //initialize internal delegate methods
            onProgressReportDelegate = new SendOrPostCallback(NotifyProgressChanged);
            onCompletedDelegate = new SendOrPostCallback(NotifyCompleted);
        }

        public XmlRemoteProcedureCall(string url, string methodName, string username, string password)
            : this()
        {
            Url = url;
            MethodName = methodName;
            Credentials = new NetworkCredential(username, password);
        }

        #endregion

        #region properties
        
        public string Url { get; set; }

        public string MethodName { get; set; }

        public NetworkCredential Credentials { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Creates the post content for the HttpWebRequest that will be submitted to the
        /// xml rpc.
        /// </summary>
        /// <returns>the post content</returns>
        protected abstract string BuildPostContentString();

        /// <summary>
        /// Validates that the XmlRemoteProcedureCall has all the necessary values for the
        /// xml rpc to successfully execute.  The base class validates that the Url and the
        /// Credentials are properly configured.
        /// </summary>
        /// <exception cref="System.ArgumentException">the specified argument is incorrect or missing</exception>
        protected virtual void ValidateValues()
        {
            if (string.IsNullOrEmpty(Url))
            {
                throw new ArgumentException("Url may not be null or empty");
            }
            if (null == Credentials)
            {
                throw new ArgumentException("Crednetials may not be null");
            }
            if (string.IsNullOrEmpty(Credentials.UserName))
            {
                throw new ArgumentException("Credentials.UserName may not be null or empty", "Credentials");
            }
            if (string.IsNullOrEmpty(Credentials.Password))
            {
                throw new ArgumentException("Credentials.Password may not be null or empty", "Credentials");
            }
        }

        /// <summary>
        /// Implementation classes will parse the xml rpc response here into a list of specific
        /// object types.  If an unexpected value is found during the process, the implementation
        /// should throw a meaningful exception.
        /// </summary>
        /// <param name="xDoc"></param>
        /// <returns></returns>
        protected abstract List<T> ParseResponseContent(XDocument xDoc);

        //this method is invoked via the AsyncOperation object, so its guaranteed to be executed
        //on the correct thread...
        protected void NotifyProgressChanged(object state)
        {
            //DEV NOTE: in testing i put break points in the event handlers for this event
            //and found that execution of the worker thread seemed to become blocked.
            ProgressChangedEventArgs args = state as ProgressChangedEventArgs;
            if (null != ProgressChanged)
            {
                ProgressChanged(this, args);
            }
        }

        //this method is invoked via the AsyncOperation object, so its guaranteed to be executed
        //on the correct thread...
        protected void NotifyCompleted(object state)
        {
            XMLRPCCompletedEventArgs<T> args = state as XMLRPCCompletedEventArgs<T>;
            if (null != Completed)
            {
                Completed(this, args);
            }
        }

        //invoke the AsyncOperation.PostOperationCompleted method to signal that the task 
        //has completed.
        private void CompletionMethod(List<T> items, Exception exception, bool canceled, AsyncOperation asyncOp)
        {
            if (!canceled)
            {
                lock (_syncRoot)
                {
                    if (userStateToLifetime.ContainsKey(asyncOp.UserSuppliedState))
                    {
                        userStateToLifetime.Remove(asyncOp.UserSuppliedState);
                    }
                }
            }

            //package the results of the operation in an XMLRPCCompletedEventArgs object
            XMLRPCCompletedEventArgs<T> args = new XMLRPCCompletedEventArgs<T>(items, exception, canceled, asyncOp.UserSuppliedState);

            asyncOp.PostOperationCompleted(onCompletedDelegate, args);
        }

        /// <summary>
        /// Starts building the request to the xml rpc, using Guid.NewGuid as the
        /// unique identifier for the task
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        public virtual void ExecuteAsync()
        {
            ExecuteAsync(Guid.NewGuid());
        }

        /// <summary>
        /// Starts building the request to the xml rpc, using the argument as the
        /// unique identifier for the task.
        /// </summary>
        /// <param name="taskId"></param>
        /// <exception cref="System.ArgumentException"></exception>
        public virtual void ExecuteAsync(object taskId)
        {
            ValidateValues();

            AsyncOperation operation = AsyncOperationManager.CreateOperation(taskId);

            //start the async op
            ThreadPool.QueueUserWorkItem((object state) =>
            {
                BeginBuildingHttpWebRequest(operation);
            });
        }

        private void BeginBuildingHttpWebRequest(AsyncOperation asyncOp)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(Url) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.ContentType = REQUESTCONTENTTYPE_VALUE;
            request.Method = REQUESTMETHOD_VALUE;
            request.UserAgent = REQUESTUSERAGENT_VALUE;

            State state = new State { Operation = asyncOp, Request = request };

            request.BeginGetRequestStream(OnBeginGetRequestStreamCompleted, state);

            //TODO: wrap this in try/catch?
            asyncOp.Post(onProgressReportDelegate, new ProgressChangedEventArgs(20, asyncOp.UserSuppliedState));
        }

        private void OnBeginGetRequestStreamCompleted(IAsyncResult result)
        {
            State state = result.AsyncState as State;

            HttpWebRequest request = state.Request;
            Stream contentStream = null;

            try
            {
                contentStream = request.EndGetRequestStream(result);
            }
            catch (Exception ex)
            {
                CompletionMethod(null, ex, false, state.Operation);
                return;
            }

            string postContent = BuildPostContentString();
            byte[] payload = Encoding.UTF8.GetBytes(postContent);

            using (contentStream)
            {
                contentStream.Write(payload, 0, payload.Length);
            }

            request.BeginGetResponse(OnBeginGetResponseCompleted, state);

            //TODO: wrap this in try/catch?
            state.Operation.Post(onProgressReportDelegate, new ProgressChangedEventArgs(40, state.Operation.UserSuppliedState));
        }


        private void OnBeginGetResponseCompleted(IAsyncResult result)
        {
            State state = result.AsyncState as State;
            HttpWebRequest request = state.Request;
            HttpWebResponse response = null;

            try
            {
                response = request.EndGetResponse(result) as HttpWebResponse;
            }
            catch (Exception ex)
            {
                CompletionMethod(null, ex, false, state.Operation);
                return;
            }

            //TODO: wrap this in try/catch?
            state.Operation.Post(onProgressReportDelegate, new ProgressChangedEventArgs(60, state.Operation.UserSuppliedState));


            Stream responseStream = response.GetResponseStream();
            string responseContent = null;

            try
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseContent = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                CompletionMethod(null, ex, false, state.Operation);
                return;
            }

            //TODO: wrap this in try/catch?
            state.Operation.Post(onProgressReportDelegate, new ProgressChangedEventArgs(80, state.Operation.UserSuppliedState));

            //search for fault code/fault string
            XDocument xDoc = XDocument.Parse(responseContent, LoadOptions.None);
            var fault = xDoc.Descendants().Where(element => XmlRPCResponseConstants.NAME == element.Name && XmlRPCResponseConstants.FAULTCODE_VALUE == element.Value);
            if (null != fault && 0 < fault.Count())
            {
                Exception exception = ParseFailureInfo(xDoc.Descendants(XmlRPCResponseConstants.STRUCT).First());
                CompletionMethod(null, exception, false, state.Operation);
            }
            else
            {
                List<T> items = null;
                Exception exception = null;
                try
                {
                    items = ParseResponseContent(xDoc);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                CompletionMethod(items, exception, false, state.Operation);
            }
        }

        private Exception ParseFailureInfo(XElement element)
        {
            int faultCode = -1;
            string message = string.Empty;

            XElement valueElement = null;
            foreach (XElement nameElement in element.Descendants(XmlRPCResponseConstants.NAME))
            {
                if (FAULTCODE_VALUE.Equals(nameElement.Value))
                {
                    valueElement = ((XElement)nameElement.NextNode).DescendantNodes().First() as XElement;
                    if (!int.TryParse(valueElement.Value, out faultCode))
                    {
                        //TODO: what to do here?
                    }
                }
                else if (FAULTSTRING_VALUE.Equals(nameElement.Value))
                {
                    valueElement = ((XElement)nameElement.NextNode).DescendantNodes().First() as XElement;
                    message = valueElement.Value;
                }
            }

            return new XmlRPCException(faultCode, message);
        }

        #endregion

    }
}
