using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class UploadFileRPC: XmlRemoteProcedureCall<UploadedFileInfo>
    {
        #region member variables

        private const string METHODNAME_VALUE = "wp.uploadFile";
        private const string PHOTOCHOOSER_VALUE = "PhotoChooser";
        private const string JPEG_EXTENSION = ".jpg";
        private const string PNG_EXTENSION = ".png";
        private const string BMP_EXTENSION = ".bmp";
        
        /// <summary>
        /// Holds a format string with the XMLRPC post content
        /// </summary>
        private readonly string _content;

        private string _fileName;
        
        #endregion

        #region constructors

        public UploadFileRPC()
            : base()
        {
            _content = XMLRPCTable.wp_uploadFile;
            MethodName = METHODNAME_VALUE;
        }

        public UploadFileRPC(Blog blog, string fileName, byte[] payload, bool overwrite)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_uploadFile;

            Blog = blog;
            TranslateFileName(fileName);
            Payload = payload;
            Overwrite = overwrite;
        }

        #endregion

        #region properties

        public Blog Blog { get; private set; }

        public string FileName {
            get { return _fileName; }
            private set
            {
                _fileName = value;
                TranslateMimeType();
            }
        }
        
        public string MimeType { get; private set;}
        
        public bool Overwrite { get; private set; }
        
        public byte[] Payload { get; private set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (null == Blog)
            {
                throw new ArgumentException("Blog may not be null", "Blog");
            }
            if (string.IsNullOrEmpty(FileName))
            {
                throw new ArgumentException("Name may not be null or an empty string.", "Name");
            }
            if (string.IsNullOrEmpty(MimeType))
            {
                throw new ArgumentException("Type may not be null or an empty string", "Type");
            }
            if (null == Payload)
            {
                throw new ArgumentException("Payload may not be null", "Payload");
            }            
        }

        protected override string BuildPostContentString()
        {
            string content = string.Format(_content,
                Blog.BlogId,
                Credentials.UserName.HtmlEncode(),
                Credentials.Password.HtmlEncode(),
                FileName.HtmlEncode(),
                MimeType.HtmlEncode(),
                EncodePayload(),
                Convert.ToInt32(Overwrite));
            return content;
        }

        private string EncodePayload()
        {
            string result = Convert.ToBase64String(Payload);
            return result.Trim();
        }

        protected override List<UploadedFileInfo> ParseResponseContent(XDocument xDoc)
        {
            List<UploadedFileInfo> result = new List<UploadedFileInfo>();
            foreach (XElement structElement in xDoc.Descendants(XmlRPCResponseConstants.STRUCT))
            {
                UploadedFileInfo info = new UploadedFileInfo(structElement);
                result.Add(info);
            }
            return result;
        }

        private void TranslateMimeType()
        {
            //DEV NOTE: PhotoChooserTask only seems to allow pictures, no video.
            //capture everything else (if that is even possible) as
            //application/octet-stream
            string extension = Path.GetExtension(FileName);

            if (JPEG_EXTENSION.Equals(extension))
            {
                MimeType = MimeTypes.JPEG;
            }
            else
            {
                MimeType = MimeTypes.UNKNOWN;
            }            
        }

        public void TranslateFileName(string originalFileName)
        {
            //DEV NOTE: the original file name from the PhotoChooserTask is pretty gross.
            //The plan is to nab the extension and use a timestamp for the file name so
            //there's something that doesn't seem crazy when the user checks what media
            //has been uploaded.
            if (originalFileName.Contains(PHOTOCHOOSER_VALUE))
            {
                DateTime capture = DateTime.Now;
                string fileNameFormat = "{0}{1}{2}{3}{4}{5}{6}"; //year, month, day, hours, min, sec, file extension
                string fileName = string.Format(fileNameFormat,
                    capture.Year,
                    capture.Month,
                    capture.Day,
                    capture.Hour,
                    capture.Minute,
                    capture.Second,
                    Path.GetExtension(originalFileName));
                FileName = fileName;
                return;
            }

            //if we're at this point, the file name should be reasonably readable so we'll
            //leave it alone
            FileName = Path.GetFileName(originalFileName);
        }

        #endregion
    }
}
