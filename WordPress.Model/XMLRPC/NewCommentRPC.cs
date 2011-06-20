using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    /// <summary>
    /// Used to reply to an existing comment.  On success, the Comment property will be
    /// updated with the resulting id value from the WordPress system.
    /// </summary>
    public class NewCommentRPC: XmlRemoteProcedureCall<Comment>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.newComment";

        #endregion

        #region constructors

        public NewCommentRPC()
            : base()
        {
            _content = XMLRPCTable.wp_newComment;
            MethodName = METHODNAME_VALUE;
        }

        public NewCommentRPC(Blog blog, Comment parentComment, Comment newComment)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_newComment;
            BlogId = blog.BlogId;
            ParentComment = parentComment;
            Comment = newComment;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        public Comment ParentComment { get; set; }

        public Comment Comment { get; set; }

        #endregion

        #region methods

       protected override void ValidateValues()
        {
            base.ValidateValues();

            if (-1 >= BlogId)
            {
                throw new ArgumentException("BlogId is an invalid value", "BlogId");
            }

            if (null == ParentComment)
            {
                throw new ArgumentException("ParentComment may not be null", "ParentComment");
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
                ParentComment.PostId,
                Comment.Content.HtmlEncode(),
                ParentComment.CommentId);
            return result;
        }

        protected override List<Comment> ParseResponseContent(XDocument xDoc)
        {
            XElement intElement = xDoc.Descendants(XmlRPCResponseConstants.INT).First();
            int commentId = -1;
            if (int.TryParse(intElement.Value, out commentId))
            {
                Comment.CommentId = commentId;
                List<Comment> result = new List<Comment>();
                result.Add(Comment);
                return result;
            }
            else
            {
                Exception exception = new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
                throw exception;
            }
        }

        #endregion



    }
}
