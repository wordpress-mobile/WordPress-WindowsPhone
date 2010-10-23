using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class GetCategoriesRPC:XmlRemoteProcedureCall<Category>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.getCategories";

        #endregion

        #region constructors

        public GetCategoriesRPC()
            : base()
        {
            _content = XMLRPCTable.wp_getCategories;
            MethodName = METHODNAME_VALUE;
        }

        public GetCategoriesRPC(Blog blog)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_getCategories;
            BlogId = blog.BlogId;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        #endregion

        #region methods

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
                Credentials.Password.HtmlEncode());
            return result;
        }

        protected override List<Category> ParseResponseContent(XDocument xDoc)
        {
            List<Category> result = new List<Category>();

            foreach (XElement structElement in xDoc.Descendants(XmlRPCResponseConstants.STRUCT))
            {
                Category current = new Category(structElement);
                result.Add(current);
            }

            return result;
        }

        #endregion

    }
}
