using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public class GetApiKeyRPC:XmlRemoteProcedureCall<Blog>
    {
        #region member variables

        private const string URI_VALUE = "https://public-api.wordpress.com/getuserblogs.php";
        private const string METHODNAME_VALUE = "";

        private Blog _blog;
        
        #endregion

        #region constructors

        public GetApiKeyRPC(Blog blog)
            : base(URI_VALUE, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _blog = blog;
        }

        #endregion

        #region methods

        protected override string BuildPostContentString()
        {
            return string.Empty;
        }

        protected override List<Blog> ParseResponseContent(XDocument xDoc)
        {
            XElement apikeyElement = xDoc.Descendants(XmlRPCResponseConstants.APIKEY).First();

            if (null == apikeyElement || string.IsNullOrEmpty(apikeyElement.Value))
            {
                throw new Exception(XmlRPCResponseConstants.UNABLETOPARSEEXPECTEDRESPONSE_MESSAGE);
            }

            _blog.ApiKey = apikeyElement.Value;

            List<Blog> result = new List<Blog>();
            result.Add(_blog);

            return result;
        }

        #endregion
    }
}
