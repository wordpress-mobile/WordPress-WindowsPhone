using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WordPress.Model
{
    /// <summary>
    /// Used to set the status of existing comments
    /// </summary>
    public class EditCommentRPC: XmlRemoteProcedureCall<Comment>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.editComment";

        #endregion

        #region constructors

        public EditCommentRPC()
            : base()
        {
            _content = XMLRPCTable.wp_editComment;
            MethodName = METHODNAME_VALUE;
        }

        public EditCommentRPC(Blog blog, Comment comment)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_editComment;
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
                Comment.CommentId,
                Comment.Content.HtmlEncode(),
                Comment.Author.HtmlEncode(),
                Comment.AuthorEmail,
                Comment.DateCreatedGMT.ToString("yyyyMMddTHH:mm:ss"),
                Comment.Status,
                Comment.AuthorUrl);
            return result;
        }

        protected override List<Comment> ParseResponseContent(XDocument xDoc)
        {
            XElement booleanElement = xDoc.Descendants(XmlRPCResponseConstants.BOOLEAN).First();
            bool success = Convert.ToBoolean(Convert.ToInt16(booleanElement.Value));
            if (success)
            {
                List<Comment> result = new List<Comment>();
                result.Add(Comment);
                return result;
            }
            else
            {
                Exception exception = new Exception(XmlRPCResponseConstants.UNABLETOPARSEEXPECTEDRESPONSE_MESSAGE);
                throw exception;
            }
        }

        #endregion

    }
}
