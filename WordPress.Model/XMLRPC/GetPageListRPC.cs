using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class GetPageListRPC: XmlRemoteProcedureCall<PageListItem>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.getPageList";

        #endregion

        #region constructors

        public GetPageListRPC()
            : base()
        {
            _content = XMLRPCTable.wp_getPageList;
            MethodName = METHODNAME_VALUE;
        }

        public GetPageListRPC(Blog blog)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_getPageList;
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
                throw new ArgumentException("BlogId is an invalid value", "BlogId");
            }
        }

        protected override string BuildPostContentString()
        {
            string result = string.Format(_content, 
                BlogId, 
                Credentials.UserName, 
                Credentials.Password);
            return result;
        }

        protected override List<PageListItem> ParseResponseContent(XDocument xDoc)
        {            
            List<PageListItem> result = new List<PageListItem>();
            foreach (XElement structElement in xDoc.Descendants(XmlRPCResponseConstants.STRUCT))
            {
                PageListItem current = new PageListItem(structElement);
                result.Add(current);
            }
            return result;
        }
        #endregion

    }
}
