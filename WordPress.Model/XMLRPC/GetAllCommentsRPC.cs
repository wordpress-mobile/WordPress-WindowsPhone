using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class GetAllCommentsRPC: XmlRemoteProcedureCall<Comment>
    {
        private const string METHODNAME_VALUE = "wp.getComments";

        private readonly string _content;

        public GetAllCommentsRPC()
            : base()
        {
            _content = XMLRPCTable.wp_getComments;
            MethodName = METHODNAME_VALUE;
        }

        public GetAllCommentsRPC(Blog blog)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_getComments;
            BlogId = blog.BlogId;
        }

        public int BlogId { get; set; }

        public eCommentStatus? CommentStatus { get; set; }

        public int Offset { get; set; }

        public int Number { get; set; }

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
                Credentials.Password.HtmlEncode(), 
                string.Empty, 
                CommentStatus.HasValue ? CommentStatus.Value.ToString() : string.Empty,
                Offset, 
                Number);
            return result;
        }

        protected override List<Comment> ParseResponseContent(XDocument xDoc)
        {
            List<Comment> result = new List<Comment>();

            foreach (XElement structElement in xDoc.Descendants(XmlRPCResponseConstants.STRUCT))
            {
                Comment current = new Comment(structElement);
                result.Add(current);
            }

            return result;
        }
    }
}
