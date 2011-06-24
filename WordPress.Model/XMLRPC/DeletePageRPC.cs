using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class DeletePageRPC : XmlRemoteProcedureCall<Post>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.deletePage";

        #endregion

        #region constructors

        public DeletePageRPC()
            : base()
        {
            _content = XMLRPCTable.wp_deletePage;
            MethodName = METHODNAME_VALUE;
        }

        public DeletePageRPC(Blog blog, Post post)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_deletePage;
            BlogId = blog.BlogId;
            Page = post;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        public Post Page { get; set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (-1 >= BlogId)
            {
                throw new ArgumentException("BlogId value is invalid", "BlogId");
            }

            if (null == Page)
            {
                throw new ArgumentException("Page may not be null", "Page");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content,
                BlogId,
                Credentials.UserName.HtmlEncode(),
                Credentials.Password.HtmlEncode(),
                Page.PostId);
            return result;

        }

        protected override List<Post> ParseResponseContent(XDocument xDoc)
        {
            XElement booleanElement = xDoc.Descendants(XmlRPCResponseConstants.BOOLEAN).First();
            if (XmlRPCResponseConstants.FALSE_STRING != booleanElement.Value)
            {                
                List<Post> result = new List<Post>();
                result.Add(Page);
                return result;
            }
            else
            {
                XmlRPCException exception = new XmlRPCException(10000, XmlRPCResponseConstants.XML_RPC_OPERATION_FAILED);
                throw exception;
            }
        }

        #endregion




    }
}
