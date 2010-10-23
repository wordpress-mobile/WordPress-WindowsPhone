using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class GetCommentsRPC: XmlRemoteProcedureCall<Comment>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.getComments";

        #endregion

        #region constructors

        public GetCommentsRPC()
            : base()
        {
            _content = XMLRPCTable.wp_getComments;
            MethodName = METHODNAME_VALUE;
        }

        public GetCommentsRPC(Blog blog)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_getComments;
            BlogId = blog.BlogId;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        public int PostId { get; set; }

        public string CommentStatus { get; set; }

        public int Offset { get; set; }

        public int Number { get; set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (-1 >= BlogId)
            {
                throw new ArgumentException("BlogId is an invalid value", "BlogId");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content, 
                BlogId,
                Credentials.UserName.HtmlEncode(),
                Credentials.Password.HtmlEncode(), 
                PostId, 
                CommentStatus.ToString(), 
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

        #endregion

    }
}
