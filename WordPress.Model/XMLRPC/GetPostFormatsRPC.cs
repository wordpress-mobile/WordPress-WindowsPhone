using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class GetPostFormatsRPC:XmlRemoteProcedureCall<Category>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.getPostFormats";

        #endregion

        #region constructors

        public GetPostFormatsRPC()
            : base()
        {
            _content = XMLRPCTable.wp_getPostFormats;
            MethodName = METHODNAME_VALUE;
        }

        public GetPostFormatsRPC(Blog blog)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_getPostFormats;
            BlogId = blog.BlogId;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (-1 >= BlogId)
            {
                throw new ArgumentException("BlogId value is invalid", "BlogId");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content,
                BlogId,
                Credentials.UserName.HtmlEncode(),
                Credentials.Password.HtmlEncode());
            return result;
        }

        protected override List<Category> ParseResponseContent(XDocument xDoc)
        {
            List<Category> result = new List<Category>();

            foreach (XElement structElement in xDoc.Descendants(XmlRPCResponseConstants.STRUCT))
            {
                foreach (XElement member in structElement.Descendants(XmlRPCResponseConstants.MEMBER))
                {
                    string value = null;
                    string memberName = member.Descendants(XmlRPCResponseConstants.NAME).First().Value;
                    if ("test".Equals(memberName))
                    {
                        value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    }
                }
            }

            return result;
        }

        #endregion

    }
}
