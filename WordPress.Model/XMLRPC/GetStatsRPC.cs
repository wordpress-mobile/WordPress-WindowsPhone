using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml;

namespace WordPress.Model
{
    /// <summary>
    /// Queries the WordPress statistic service for the given statistic type over the selected
    /// time period.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GetStatsRPC<T> where T : INotifyPropertyChanged
    {
        #region member variables

        protected const string STATSURL_FORMATSTRING = "http://stats.wordpress.com/api/1.0/?api_key={0}&blog_uri={1}&format=xml&table={2}&end={3}&days={4}&limit=-1{5}";

        private SendOrPostCallback onCompletedDelegate;

        #endregion

        #region events

        public event XMLRPCCompletedEventHandler<T> Completed;

        #endregion

        #region constructors

        public GetStatsRPC()
        {
            onCompletedDelegate = new SendOrPostCallback(NotifyCompleted);
        }

        public GetStatsRPC(Blog blog)
            : this()
        {
            Blog = blog;
        }

        #endregion

        #region properties

        public Blog Blog { get; private set; }

        public eStatisticPeriod StatisicPeriod { get; set; }

        public eStatisticType StatisticType { get; set; }

        protected virtual Uri Uri
        {
            get
            {
                string url = string.Format(STATSURL_FORMATSTRING,
                    Blog.ApiKey,
                    Blog.Url,
                    StatisticType.ToString().ToLower(),
                    DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString(),
                    Days,
                    Period
                    );

                return new Uri(url, UriKind.Absolute);
            }
        }

        private int Days
        {
            get
            {
                switch (StatisicPeriod)
                {
                    case eStatisticPeriod.LastWeek:
                        return 7;
                    case eStatisticPeriod.LastMonth:
                        return 30;
                    case eStatisticPeriod.LastQuarter:
                        return 12;
                    case eStatisticPeriod.LastYear:
                        return 11;
                    case eStatisticPeriod.AllTime:
                        return -1;
                    default:
                        return -1;
                }
            }
        }

        private string Period
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                switch (StatisicPeriod)
                {
                    case eStatisticPeriod.LastWeek:
                        //do nothing here
                        break;
                    case eStatisticPeriod.LastMonth:
                        //do nothing here
                        break;
                    case eStatisticPeriod.LastQuarter:
                        builder.Append("&period=week");
                        break;
                    case eStatisticPeriod.LastYear:
                        builder.Append("&period=month");
                        break;
                    case eStatisticPeriod.AllTime:
                        builder.Append("&period=month");
                        break;
                    default:
                        break;
                }

                return builder.ToString();
            }
        }

        #endregion

        #region methods

        private void CompletionMethod(List<T> items, Exception exception, bool canceled, AsyncOperation asyncOp)
        {
            //package the results of the operation in an XMLRPCCompletedEventArgs object
            XMLRPCCompletedEventArgs<T> args = new XMLRPCCompletedEventArgs<T>(items, exception, canceled, asyncOp.UserSuppliedState);

            asyncOp.PostOperationCompleted(onCompletedDelegate, args);
        }

        private void NotifyCompleted(object state)
        {
            XMLRPCCompletedEventArgs<T> args = state as XMLRPCCompletedEventArgs<T>;
            if (null != Completed)
            {
                Completed(this, args);
            }
        }

        protected virtual void ValidateValues()
        {
            if (null == Blog)
            {
                throw new ArgumentException("Blog may not be null", "Blog");
            }
            if (string.IsNullOrEmpty(Blog.ApiKey))
            {
                throw new ArgumentException("Blog.ApiKey may not be null or empty", "Blog.ApiKey");
            }
        }

        public void ExecuteAsync()
        {
            ExecuteAsync(Guid.NewGuid());
        }

        public void ExecuteAsync(object taskId)
        {
            ValidateValues();

            AsyncOperation operation = AsyncOperationManager.CreateOperation(taskId);

            ThreadPool.QueueUserWorkItem((state) =>
            {
                FetchStats(operation);
            }, operation);
        }

        private void FetchStats(AsyncOperation operation)
        {
            HttpWebRequest request = HttpWebRequest.Create(Uri) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.ContentType = XmlRPCRequestConstants.CONTENTTYPE;
            request.Method = XmlRPCRequestConstants.POST;
            request.UserAgent = Constants.WORDPRESS_USERAGENT;

            request.BeginGetResponse(responseResult =>
            {
                try
                {
                    HttpWebResponse response = request.EndGetResponse(responseResult) as HttpWebResponse;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseContent = reader.ReadToEnd();
                        XDocument xDoc = ParseDocument(responseContent);

                        Exception exception = null;
                        List<T> items = null;

                        var fault = xDoc.Descendants().Where(element => XmlRPCResponseConstants.NAME == element.Name && XmlRPCResponseConstants.FAULTCODE_VALUE == element.Value);
                        if (null != fault && 0 < fault.Count())
                        {
                            exception = ParseFailureInfo(xDoc.Descendants(XmlRPCResponseConstants.STRUCT).First());
                        }
                        else
                        {
                            try
                            {
                                items = ParseResponseContent(xDoc);
                            }
                            catch (Exception ex)
                            {
                                exception = ex;
                            }
                        }

                        CompletionMethod(items, exception, false, operation);
                    }
                }
                catch (Exception ex)
                {
                    CompletionMethod(new List<T>(), ex, false, operation);
                }
            }, request);
        }

        private XDocument ParseDocument(string content)
        {
            XDocument xDoc = null;
            try
            {
                xDoc = XDocument.Parse(content, LoadOptions.None);
            }
            catch (Exception outerEx)
            {
                //Re-parse the response after some cleaning stuff
                string originalServerResponse = null; //Keep a copy of the server response "as-is", without cleaning it.
                if (!String.IsNullOrEmpty(content))
                {
                    originalServerResponse = String.Copy(content);
                    Regex regex = new Regex("\n", RegexOptions.IgnoreCase);
                    content = regex.Replace(content, String.Empty);
                    regex = new Regex("\t", RegexOptions.IgnoreCase);
                    content = regex.Replace(content, String.Empty);

                    //Use numeric entities instead of the named version by looping over pairs with foreach
                    foreach (KeyValuePair<string, string> pair in StringUtils.getHtmlEntities())
                    {
                        content = content.Replace( pair.Key, "&#" + pair.Value + ";" );
                    }

                    //remove the un-replaced entities
                    regex = new Regex("&([a-zA-Z]+;)", RegexOptions.IgnoreCase);
                    content = regex.Replace(content, String.Empty);

                    //note: We are not removing 'non-utf-8 characters'. We are removing utf-8 characters that may not appear in well-formed XML documents.
                    string pattern = @"#x((10?|[2-F])FFF[EF]|FDD[0-9A-F]|[19][0-9A-F]|7F|8[0-46-9A-F]|0?[1-8BCEF])";
                    regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    if (regex.IsMatch(content))
                    {
                        this.DebugLog("found characters that must not appear in the XML-RPC response");
                        content = regex.Replace(content, String.Empty);
                    }

                    if (!content.StartsWith("<?xml"))
                    {
                        //clean the junk b4 the xml preamble
                        this.DebugLog("cleaning the junk before the xml preamble");
                        int indexOfFirstLt = content.IndexOf("<?xml");
                        if (indexOfFirstLt > -1)
                            content = content.Substring(indexOfFirstLt);
                    }
                }

                try
                {
                    xDoc = XDocument.Parse(content, LoadOptions.None);
                }
                catch (Exception ex)
                {

                    //something went wrong during the parsing process
                    this.DebugLog("Parser error: " + ex.Message); //this is the original error, that should not be shown to the user.

                    if (ex is XmlException)
                    {
                        //most likely case here is that the API key is not for an administrator's account.
                        //try to read the error from the response
                        string errorDescription = ParseErrorDescription(originalServerResponse);
                        if (!string.IsNullOrEmpty(errorDescription))
                        {
                            throw new Exception(errorDescription, ex);
                        }
                    }

                    //Keep track of the original exception by adding the response from the server. If the recovery fails we should throw this exception.
                    if (!String.IsNullOrEmpty(originalServerResponse))
                    {
                        ex = new Exception("\n Server Response --> " + originalServerResponse, ex);
                    }
                    throw new XmlRPCParserException(XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_CODE, XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_MESSAGE, ex);
                }
            }
            return xDoc;
        }

        private string ParseErrorDescription(string content)
        {
            string line = string.Empty;
            string result = string.Empty;
            string error = "Error:";

            using (StringReader reader = new StringReader(content))
            {
                line = reader.ReadLine();

                //TODO: will the string be I18N'ed?
                if (line.StartsWith(error))
                {
                    result = line.Replace(error, string.Empty).Trim();
                }
            }
            return result;
        }

        private Exception ParseFailureInfo(XElement element)
        {
            int faultCode = -1;
            string message = string.Empty;

            XElement valueElement = null;
            foreach (XElement nameElement in element.Descendants(XmlRPCResponseConstants.NAME))
            {
                if (XmlRPCResponseConstants.FAULTCODE_VALUE.Equals(nameElement.Value))
                {
                    valueElement = ((XElement)nameElement.NextNode).DescendantNodes().First() as XElement;
                    if (!int.TryParse(valueElement.Value, out faultCode))
                    {
                        return new ArgumentException("Unable to parse fault code from response");
                    }
                }
                else if (XmlRPCResponseConstants.FAULTSTRING_VALUE.Equals(nameElement.Value))
                {
                    valueElement = ((XElement)nameElement.NextNode).DescendantNodes().First() as XElement;
                    message = valueElement.Value;
                }
            }

            return new XmlRPCException(faultCode, message);
        }

        protected abstract List<T> ParseResponseContent(XDocument xDoc);

        #endregion
    }
}
