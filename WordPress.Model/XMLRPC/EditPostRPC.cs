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
    public class EditPostRPC : AbstractPostRPC
    {
 
        #region constructors

        public EditPostRPC()
            : base()
        {
            _content = XMLRPCTable.metaWeblog_editPost;
            MethodName = "metaWeblog.editPost";
        }

        public EditPostRPC(Blog blog, Post post)
            : base(blog.Xmlrpc, "metaWeblog.editPost", blog.Username, blog.Password)
        {
            _content = XMLRPCTable.metaWeblog_editPost;
            Post = post;
        }

        #endregion

 
        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (null == base.Post)
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
                String.Format(XmlRPCRequestConstants.DATETIMEFORMATSTRING, Post.DateCreated.ToUniversalTime()));

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
                XmlRPCException exception = new XmlRPCException(XmlRPCResponseConstants.XML_RPC_OPERATION_FAILED_CODE, XmlRPCResponseConstants.XML_RPC_OPERATION_FAILED_MESSAGE);
                throw exception;
            }
        }

        #endregion


    }
}
