using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Collections.Generic;

namespace WordPress.Model
{
    /// <summary>
    /// Used to edit both posts and pages
    /// </summary>
    public class NewPostRPC : XmlRemoteProcedureCall<Post>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "metaWeblog.newPost";

        #endregion

        #region constructors

        public NewPostRPC()
            : base()
        {
            _content = XMLRPCTable.metaWeblog_newPost;
            MethodName = "metaWeblog.newPost";
        }

        public NewPostRPC(Blog blog, Post post)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.metaWeblog_newPost;
            Post = post;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        public Post Post { get; set; }

        public ePostType PostType { get; set; }

        public bool Publish { get; set; }

        #endregion

        #region methods

        private string FormatCategories()
        {
            string dataFormatString = "<value><string>{0}</string></value>";

            StringBuilder categoryBuilder = new StringBuilder();
            string data = string.Empty;

            foreach (string category in Post.Categories)
            {
                data = string.Format(dataFormatString, category);
                categoryBuilder.Append(data);
            }

            return categoryBuilder.ToString();
        }

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (-1 >= BlogId)
            {
                throw new ArgumentException("BlogId is an invalid value", "BlogId");
            }

            if (null == Post)
            {
                throw new ArgumentException("Post may not be null", "Post");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content,
                Post.PostId,
                Credentials.UserName,
                Credentials.Password,
                Post.MtKeyWords,
                PostType.ToString(),
                FormatCategories(),
                HttpUtility.HtmlEncode(Post.Title),
                HttpUtility.HtmlEncode(Post.Description),
                Publish);

            return result;
        }

        protected override List<Post> ParseResponseContent(XDocument xDoc)
        {
            XElement stringElement = xDoc.Descendants(XmlRPCResponseConstants.STRING).First();

            int postId;
            if (int.TryParse(stringElement.Value, out postId))
            {
                Post.PostId = postId;
                List<Post> result = new List<Post>();
                result.Add(Post);
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
