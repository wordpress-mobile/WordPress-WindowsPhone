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
    public class NewPostRPC : AbstractPostRPC
    {

        #region constructors

        public NewPostRPC()
            : base()
        {
            _content = XMLRPCTable.metaWeblog_newPost;
            MethodName = "metaWeblog.newPost";
        }

        public NewPostRPC(Blog blog, Post post)
            : base(blog.Xmlrpc, "metaWeblog.newPost", blog.Username, blog.Password)
        {
            _content = XMLRPCTable.metaWeblog_newPost;
            Post = post;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        #endregion

        #region methods

        private string FormatCustomFields()
        {
            string dataFormatString = "<value><struct><member><name>key</name><value><string>{0}</string></value></member><member><name>value</name><value><string>{1}</string></value></member></struct></value>";

            StringBuilder customFieldBuilder = new StringBuilder();
            string data = string.Empty;

            foreach (CustomField cf in Post.CustomFields)
            {
                data = string.Format(dataFormatString, cf.Key, cf.Value);
                customFieldBuilder.Append(data);
            }

            return customFieldBuilder.ToString();
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
                Credentials.UserName.HtmlEncode(),
                Credentials.Password.HtmlEncode(),
                Post.MtKeyWords.XmlEscape(),
                PostType.ToString(),
                FormatCategories(),
                Post.Title.XmlEscape(),
                Post.Description.HtmlEncode(),
                PostType.ToString(),
                Post.PostStatus,
                FormatCustomFields(),
                Post.PostFormat,
                String.Format(XmlRPCRequestConstants.DATETIMEFORMATSTRING, Post.DateCreatedGMT)
                );
            return result;
        }

        protected override List<Post> ParseResponseContent(XDocument xDoc)
        {
            XElement stringElement = xDoc.Descendants(XmlRPCResponseConstants.STRING).First();

            string postId = stringElement.Value;
            Post.PostId = postId;
            List<Post> result = new List<Post>();
            result.Add(Post);
            return result;
        }

        #endregion

    }
}
