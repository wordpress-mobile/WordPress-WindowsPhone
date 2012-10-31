using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Media;



namespace WordPress.Model
{
    public class UploadFileRPC : XmlRemoteProcedureCall<Media>
    {
        #region member variables

        private const string METHODNAME_VALUE = "wp.uploadFile";
        private int retryCount = 0;

        private const string FILE_VALUE = "file";
        private const string URL_VALUE = "url";
        private const string TYPE_VALUE = "type";


        /// <summary>
        /// Holds a format string with the XMLRPC post content
        /// </summary>
        private readonly string _content;
        private State originalState;

        #endregion

        #region constructors

        public UploadFileRPC()
            : base()
        {
            _content = XMLRPCTable.wp_uploadFile;
            MethodName = METHODNAME_VALUE;
        }

        public UploadFileRPC(Blog blog, Media currentMedia, bool overwrite)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_uploadFile;

            Blog = blog;
            CurrentMedia = currentMedia;
            Overwrite = overwrite;
        }

        #endregion

        #region properties

        public Blog Blog { get; private set; }

        public Media CurrentMedia { get; private set; }

        public bool Overwrite { get; private set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (null == Blog)
            {
                throw new ArgumentException("Blog may not be null", "Blog");
            }
            if (string.IsNullOrEmpty(CurrentMedia.FileName))
            {
                throw new ArgumentException("Name may not be null or an empty string.", "Name");
            }
            if (string.IsNullOrEmpty(CurrentMedia.MimeType))
            {
                throw new ArgumentException("Type may not be null or an empty string", "Type");
            }
            if (null == CurrentMedia.LocalPath)
            {
                throw new ArgumentException("Location of the file may not be null", "Data");
            }
        }

        protected override string BuildPostContentString()
        {
            throw new NotSupportedException("BuildPostContentString should not be called on image");
            /*
            string content = "<?xml version=\"1.0\"?><methodCall><methodName>wp.uploadFile</methodName><params>" +
                "<param><value><int>" + Blog.BlogId + "</int></value></param>" +
                "<param><value><string>" + Credentials.UserName.HtmlEncode() + "</string></value></param>" +
                "<param><value><string>" + Credentials.Password.HtmlEncode() + "</string></value></param>" +
                "<param><struct><member><name>name</name><value><string>" + FileName.HtmlEncode() + "</string></value></member>" +
                "<member><name>type</name><value><string>" + MimeType.HtmlEncode() + "</string></value></member>" +
                "<member><name>bits</name><value><base64>";

            byte[] chunk = new byte[3600];
            _bitmapStream.Position = 0;

            int count = 0;
            while ((count = _bitmapStream.Read(chunk, 0, chunk.Length)) > 0)
            {
                string result = Convert.ToBase64String(chunk);
                result = result.Trim();
                content += result;
            }

            content += "</base64></value></member>" +
                "<member><name>overwrite</name><value><bool>" + Convert.ToInt32(Overwrite) + "</bool></value></member>" +
                "</struct></param></params></methodCall>";
            return content;*/
        }
        
        /* Another way of reading a file by chunks
         *             bool readCompleted = false;
               while ( readCompleted == false )
               {
                   int index = 0;
                   //we're trying to keep reading into our chunk until either we reach the end of the stream, or we've read everything we need.
                   while (index < chunk.Length)
                   {
                       int bytesRead = _bitmapStream.Read(chunk, index, chunk.Length - index);
                       this.DebugLog("Read the following noof bytes: " + bytesRead);
                       if (bytesRead == 0)
                       {
                           break;
                       }
                       index += bytesRead;
                   }
                   if (index != 0) // Our previous chunk may have been the last one
                   {
                       string result = Convert.ToBase64String(chunk);
                       result = result.Trim();
                       content += result;
                   }
                   if (index != chunk.Length) // We didn't read a full chunk: we're done
                   {
                       readCompleted = true;
                       this.DebugLog("No more bytes to read");
                   }
               }
               */



        /* Read the whole file into the byte array. Do not use this method.
        int length = (int)_bitmapStream.Length;
        _bitmapStream.Position = 0;
        byte[] payload = new byte[length];
        _bitmapStream.Read(payload, 0, length);
        string result = Convert.ToBase64String(payload);
        result = result.Trim();
        */

        /* Override methods defined in the base class since we are uploading the content of the image by chunk */       
        public override void ExecuteAsync(object taskId)
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
            request.ContentType = XmlRPCRequestConstants.CONTENTTYPE;
            request.Method = XmlRPCRequestConstants.POST;
            request.UserAgent = Constants.WORDPRESS_USERAGENT;

            State state = new State { Operation = asyncOp, Request = request };

            request.BeginGetRequestStream(OnBeginGetRequestStreamCompleted, state);

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

            Stream _bitmapStream = CurrentMedia.getImageStream();

            using (contentStream)
            {
                //Write the first chunk of data
                string content = "<?xml version=\"1.0\"?><methodCall><methodName>wp.uploadFile</methodName><params>" +
                "<param><value><int>" + Blog.BlogId + "</int></value></param>" +
                "<param><value><string>" + Credentials.UserName.HtmlEncode() + "</string></value></param>" +
                "<param><value><string>" + Credentials.Password.HtmlEncode() + "</string></value></param>" +
                "<param><struct><member><name>name</name><value><string>" + CurrentMedia.FileName + "</string></value></member>" +
                "<member><name>type</name><value><string>" + CurrentMedia.MimeType.HtmlEncode() + "</string></value></member>" +
                "<member><name>bits</name><value><base64>";

                byte[] payload = Encoding.UTF8.GetBytes(content);
                contentStream.Write(payload, 0, payload.Length);
                
                //Write the chunks of the image
                byte[] chunk = new byte[3600];
                int count = 0;
                while ((count = _bitmapStream.Read(chunk, 0, chunk.Length)) > 0)
                {
                    payload = Encoding.UTF8.GetBytes(Convert.ToBase64String(chunk).Trim());
                    contentStream.Write(payload, 0, payload.Length);
                }
                _bitmapStream.Close();

                //Write the last chunk of data
                content = "</base64></value></member>" +
                    "<member><name>overwrite</name><value><bool>" + Convert.ToInt32(Overwrite) + "</bool></value></member>" +
                    "</struct></param></params></methodCall>";

                payload = Encoding.UTF8.GetBytes(content);
                contentStream.Write(payload, 0, payload.Length);           
            }

            request.BeginGetResponse(OnBeginFileResponseCompleted, state);

            state.Operation.Post(onProgressReportDelegate, new ProgressChangedEventArgs(40, state.Operation.UserSuppliedState));
        }

        private void OnBeginFileResponseCompleted(IAsyncResult result)
        {
            State state = result.AsyncState as State;
            if (retryCount == 0)
            {
                originalState = state;
            }
                
            HttpWebRequest request = state.Request;
            HttpWebResponse response = null;

            try
            {
                response = request.EndGetResponse(result) as HttpWebResponse;
            }
            catch (Exception ex)
            {
                if (retryCount == 2)
                {
                    CompletionMethod(null, ex, false, originalState.Operation);
                    return;
                }
                else
                {
                    retryCount++;
                    AsyncOperation newAsyncOp = AsyncOperationManager.CreateOperation(Guid.NewGuid());
                    BeginBuildingHttpWebRequest(newAsyncOp);
                    return;
                }
            }

            originalState.Operation.Post(onProgressReportDelegate, new ProgressChangedEventArgs(60, originalState.Operation.UserSuppliedState));

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
                CompletionMethod(null, ex, false, originalState.Operation);
                return;
            }

            originalState.Operation.Post(onProgressReportDelegate, new ProgressChangedEventArgs(80, originalState.Operation.UserSuppliedState));

            XDocument xDoc = null;
            try
            {
                xDoc = this.cleanAndParseServerResponse(responseContent);
            }
            catch (Exception ex2)
            {
                CompletionMethod(null, ex2, false, originalState.Operation);
                return;
            }
           
            var fault = xDoc.Descendants().Where(element => XmlRPCResponseConstants.NAME == element.Name && XmlRPCResponseConstants.FAULTCODE_VALUE == element.Value);
            if (null != fault && 0 < fault.Count())
            {
                Exception exception = ParseFailureInfo(xDoc.Descendants(XmlRPCResponseConstants.STRUCT).First());
                CompletionMethod(null, exception, false, originalState.Operation);
            }
            else
            {
                List<Media> items = null;
                Exception exception = null;
                try
                {
                    items = ParseResponseContent(xDoc);
                }
                catch (Exception ex)
                {
                    if (ex is XmlRPCException || ex is XmlRPCParserException)
                        exception = ex;
                    else
                    {
                        exception = new XmlRPCException(XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_CODE, XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_MESSAGE, ex);
                    }
                }
                CompletionMethod(items, exception, false, originalState.Operation);
            }
        }
   
        /* END of the overrided methods */

        protected override List<Media> ParseResponseContent(XDocument xDoc)
        {
            List<Media> result = new List<Media>();
            foreach (XElement structElement in xDoc.Descendants(XmlRPCResponseConstants.STRUCT))
            {
                this.ParseImageResponseElement(structElement);
                result.Add(CurrentMedia);
            }
            return result;
        }
        
        private void ParseImageResponseElement(XElement element)
        {
            if (!element.HasElements)
            {
                throw new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_CODE, XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }

            string value = null;
            foreach (XElement member in element.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                string memberName = member.Descendants(XmlRPCResponseConstants.NAME).First().Value;
                if (FILE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    CurrentMedia.FileName = value;
                }
                else if (URL_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    CurrentMedia.Url = value;
                }
                else if (TYPE_VALUE.Equals(memberName))
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    CurrentMedia.MimeType = value;
                }
            }
        }

        #endregion
    }
}
