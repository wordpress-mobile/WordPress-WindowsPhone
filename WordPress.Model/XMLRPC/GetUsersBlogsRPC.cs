using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class GetUsersBlogsRPC : XmlRemoteProcedureCall<Blog>
    {
        #region member variables

        /// <summary>
        /// Holds a format string with the XMLRPC post content
        /// </summary>
        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.getUsersBlogs";

        #endregion

        #region constructors

        public GetUsersBlogsRPC()
            : base()
        {
            _content = XMLRPCTable.wp_getUsersBlogs;
            MethodName = METHODNAME_VALUE;
        }

        public GetUsersBlogsRPC(string url, string username, string password)
            : base(url, METHODNAME_VALUE, username, password)
        {
            _content = XMLRPCTable.wp_getUsersBlogs;
        }

        #endregion

        #region methods

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content,
                Credentials.UserName,
                Credentials.Password);  
            return result;
        }

        protected override List<Blog> ParseResponseContent(XDocument xDoc)
        {
            XElement structElement = xDoc.Descendants(XmlRPCResponseConstants.STRUCT).First();
            Blog blog = new Blog(structElement);
            blog.Username = Credentials.UserName;
            blog.Password = Credentials.Password;

            List<Blog> result = new List<Blog>();
            result.Add(blog);
            return result;
        }
        #endregion

    }
}
