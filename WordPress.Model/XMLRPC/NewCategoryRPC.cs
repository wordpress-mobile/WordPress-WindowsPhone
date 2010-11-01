using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class NewCategoryRPC: XmlRemoteProcedureCall<Category>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.newCategory";

        #endregion

        #region constructors

        public NewCategoryRPC()
            : base()
        {
            _content = XMLRPCTable.wp_newCategory;
            MethodName = "wp.newCategory";
        }

        public NewCategoryRPC(Blog blog, Category category)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_newCategory;
            BlogId= blog.BlogId;
            Category = category;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        public Category Category { get; set; }

        #endregion

        #region methods

        protected override void ValidateValues()
        {
            base.ValidateValues();

            if (-1 >= BlogId)
            {
                throw new ArgumentException("BlogId is an invalid value", "BlogId");
            }

            if (null == Category)
            {
                throw new ArgumentException("Category may not be null", "Category");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content,
                BlogId,
                Credentials.UserName.HtmlEncode(),
                Credentials.Password.HtmlEncode(),
                Category.Description.HtmlEncode(),
                Category.CategorySlug.HtmlEncode(),
                Category.ParentId,
                Category.CategoryName.HtmlEncode());
            return result;
        }

        protected override List<Category> ParseResponseContent(XDocument xDoc)
        {
            XElement intElement = xDoc.Descendants(XmlRPCResponseConstants.INT).First();
            int categoryId = 0;
            if (int.TryParse(intElement.Value, out categoryId))
            {
                Category.CategoryId = categoryId;

                List<Category> result = new List<Category>();
                result.Add(Category);
                return result;
            }
            else
            {
                Exception exception = new Exception(XmlRPCResponseConstants.UNABLETOPARSEEXPECTEDRESPONSE_MESSAGE);
                throw exception;
            }
        }

        #endregion

    }
}
