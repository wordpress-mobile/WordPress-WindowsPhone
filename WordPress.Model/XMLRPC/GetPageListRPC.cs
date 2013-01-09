using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace WordPress.Model
{
    public class GetPageListRPC: XmlRemoteProcedureCall<PageListItem>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.getPages";

        #endregion

        #region constructors

        public GetPageListRPC(Blog blog)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_getPages;
            BlogId = blog.BlogId;
        }

        #endregion

        #region properties

        public int BlogId { get; set; }

        public int NumberOfPages { get; set; }

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
                NumberOfPages);
            return result;
        }

        protected override List<PageListItem> ParseResponseContent(XDocument xDoc)
        {            
            List<PageListItem> result = new List<PageListItem>();

            XElement arrayElements = xDoc.Descendants(XmlRPCResponseConstants.ARRAY).First().Element(XmlRPCResponseConstants.DATA);

            foreach (XElement arrayEl in arrayElements.Elements(XmlRPCResponseConstants.VALUE))
            {
                XElement currentStruct = arrayEl.Element(XmlRPCResponseConstants.STRUCT);
                PageListItem current = new PageListItem(currentStruct);
                result.Add(current);
            }
            return result;
        }
        #endregion

    }
}
