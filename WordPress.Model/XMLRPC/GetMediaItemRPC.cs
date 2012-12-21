using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class GetMediaItemRPC:XmlRemoteProcedureCall<MediaItem>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.getMediaItem";

        #endregion

        #region constructors

        public GetMediaItemRPC()
            : base()
        {
            _content = XMLRPCTable.wp_getPostFormats;
            MethodName = METHODNAME_VALUE;
        }

        public GetMediaItemRPC(Blog blog, string mediaId)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_getMediaItem;
            BlogId = blog.BlogId;
            MediaId = mediaId;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }
        public string MediaId { get; set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (-1 >= BlogId)
            {
                throw new ArgumentException("BlogId value is invalid", "BlogId");
            }
            if (null == MediaId || MediaId.Length == 0)
            {
                throw new ArgumentException("The MediaId is invalid", "MediaId");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content,
                BlogId,
                Credentials.UserName.HtmlEncode(),
                Credentials.Password.HtmlEncode(),
                MediaId);
            return result;
        }

        protected override List<MediaItem> ParseResponseContent(XDocument xDoc)
        {
            List<MediaItem> result = new List<MediaItem>();
            XElement structElement = xDoc.Descendants(XmlRPCResponseConstants.STRUCT).First();
            
            MediaItem m = new MediaItem(structElement);
            result.Add(m);
            
            return result;
        }

        #endregion

    }
}
