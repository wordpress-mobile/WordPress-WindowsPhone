using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class GetPostRPC : XmlRemoteProcedureCall<Post>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "metaWeblog.getPost";

        #endregion

        #region constructor

        public GetPostRPC()
            : base()
        {
            _content = XMLRPCTable.metaWeblog_getPost;
            MethodName = METHODNAME_VALUE;
        }

        public GetPostRPC(Blog blog, string postId)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.metaWeblog_getPost;
            PostId = postId;
        }

        #endregion

        #region properties

        public string PostId { get; set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (string.IsNullOrEmpty(PostId))
            {
                throw new ArgumentException("PostId may not be null or empty", "PostId");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content,
                PostId,
                Credentials.UserName.HtmlEncode(),
                Credentials.Password.HtmlEncode());
            return result;
        }

        protected override List<Post> ParseResponseContent(XDocument xDoc)
        {
            XElement structElement = xDoc.Descendants(XmlRPCResponseConstants.STRUCT).First();
            Post post = new Post(structElement);

            List<Post> result = new List<Post>();
            result.Add(post);
            return result;
        }

        #endregion

    }
}
