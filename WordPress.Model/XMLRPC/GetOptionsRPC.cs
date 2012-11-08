using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class GetOptionsRPC:XmlRemoteProcedureCall<Option>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.getOptions";

        #endregion

        #region constructors

        public GetOptionsRPC()
            : base()
        {
            _content = XMLRPCTable.wp_getOptions;
            MethodName = METHODNAME_VALUE;
        }

        public GetOptionsRPC(Blog blog)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_getOptions;
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

        protected override List<Option> ParseResponseContent(XDocument xDoc)
        {
            List<Option> result = new List<Option>();
            XElement outerstructElement = xDoc.Descendants(XmlRPCResponseConstants.STRUCT).First();
            foreach (XElement structElement in outerstructElement.Elements(XmlRPCResponseConstants.MEMBER))
            {
                Option current = new Option(structElement);
                result.Add(current);
            }

            return result;
        }

        #endregion

    }
}
