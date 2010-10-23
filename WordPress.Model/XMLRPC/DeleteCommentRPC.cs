using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class DeleteCommentRPC: XmlRemoteProcedureCall<Comment>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.deleteComment";

        #endregion

        #region constructors

        public DeleteCommentRPC()
            : base()
        {
            _content = XMLRPCTable.wp_deleteComment;
            MethodName = METHODNAME_VALUE;
        }

        public DeleteCommentRPC(Blog blog, Comment comment)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_deleteComment;
            BlogId = blog.BlogId;
            Comment = comment;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        public Comment Comment { get; set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (-1 >= BlogId)
            {
                throw new ArgumentException("BlogId value is invalid", "BlogId");
            }
            if (null == Comment)
            {
                throw new ArgumentException("Comment may not be null", "Comment");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content,
                BlogId,
                Credentials.UserName.HtmlEncode(),
                Credentials.Password.HtmlEncode(),
                Comment.CommentId);
            return result;
        }

        protected override List<Comment> ParseResponseContent(XDocument xDoc)
        {
            XElement booleanElement = xDoc.Descendants(XmlRPCResponseConstants.BOOLEAN).First();
            if (XmlRPCResponseConstants.FALSE_STRING != booleanElement.Value)
            {
                List<Comment> result = new List<Comment>();
                result.Add(Comment);
                return result;
            }
            else
            {
                Exception exception = new Exception(XmlRPCResponseConstants.RPCRETURNEDFAILURE_MESSAGE);
                throw exception;
            }
        }

        #endregion
    }
}
