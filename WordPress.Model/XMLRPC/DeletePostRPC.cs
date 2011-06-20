using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class DeletePostRPC: XmlRemoteProcedureCall<Post>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "blogger.deletePost";

        #endregion

        #region constructors

        public DeletePostRPC()
            : base()
        {
            _content = XMLRPCTable.blogger_deletePost;
            MethodName = METHODNAME_VALUE;
        }

        public DeletePostRPC(Blog blog, Post post)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.blogger_deletePost;
            //app key is ignored
            Post = post;
        }

        #endregion

        #region properties

        public string AppKey { get; set; }

        public Post Post { get; set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (null == Post)
            {
                throw new ArgumentException("Post may not be null", "Post");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content,
                    AppKey,
                    Post.PostId,
                    Credentials.UserName.HtmlEncode(),
                    Credentials.Password.HtmlEncode());
            return result;
        }

        protected override List<Post> ParseResponseContent(XDocument xDoc)
        {
            XElement booleanElement = xDoc.Descendants(XmlRPCResponseConstants.BOOLEAN).First();
            if (XmlRPCResponseConstants.FALSE_STRING != booleanElement.Value)
            {
                List<Post> result = new List<Post>();
                result.Add(Post);
                return result;
            }
            else
            {
                Exception exception = new Exception(XmlRPCResponseConstants.XML_RPC_OPERATION_FAILED);
                throw exception;
            }
        }

        #endregion
    }
}
