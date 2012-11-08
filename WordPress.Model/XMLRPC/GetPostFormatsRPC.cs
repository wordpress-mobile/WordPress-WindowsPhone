using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace WordPress.Model
{
    public class GetPostFormatsRPC:XmlRemoteProcedureCall<PostFormat>
    {
        #region member variables

        private readonly string _content;

        private const string METHODNAME_VALUE = "wp.getPostFormats";

        #endregion

        #region constructors

        public GetPostFormatsRPC()
            : base()
        {
            _content = XMLRPCTable.wp_getPostFormats;
            MethodName = METHODNAME_VALUE;
        }

        public GetPostFormatsRPC(Blog blog)
            : base(blog.Xmlrpc, METHODNAME_VALUE, blog.Username, blog.Password)
        {
            _content = XMLRPCTable.wp_getPostFormats;
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

        protected override List<PostFormat> ParseResponseContent(XDocument xDoc)
        {
            Dictionary<string, string> supportedPostFormat = new Dictionary<string, string>();
            List<string> supportedPostFormatsKeys = new List<string>();
            List<PostFormat> returnObj = new List<PostFormat>();

            XElement structElement = xDoc.Descendants(XmlRPCResponseConstants.STRUCT).First(); //the outer struct
            foreach (XElement member in structElement.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                string memberName = member.Descendants(XmlRPCResponseConstants.NAME).First().Value;
                if ("all".Equals(memberName))
                {
                    supportedPostFormat = parseAllPostFormats(member.Descendants(XmlRPCResponseConstants.STRUCT).First());
                }
                else if ("supported".Equals(memberName))
                {
                    supportedPostFormatsKeys = parseSupportedPostFormatsKeys(member.Descendants(XmlRPCResponseConstants.ARRAY).First() );
                }
            }

            if (supportedPostFormat == null || supportedPostFormat.Count < 1 || supportedPostFormatsKeys == null || supportedPostFormatsKeys.Count < 1)
            {
                throw new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_CODE, XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }
            else
            {
                bool isStandarAvailable = false;
                supportedPostFormatsKeys.ForEach(element =>
                {
                    string value = null;
                    supportedPostFormat.TryGetValue(element, out value);
                    if (element.Equals("standard") && value != null)
                        isStandarAvailable = true;
                    if (value != null)
                    {
                        returnObj.Add(new PostFormat(element, value));
                    }
                });

                if(!isStandarAvailable)
                    returnObj.Add(new PostFormat("standard", "Standard"));
            }
            return returnObj;
        }

        private Dictionary<string, string> parseAllPostFormats(XElement allPostFormatsStructElement)
        {
            if (!allPostFormatsStructElement.HasElements)
            {
                throw new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_CODE, XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }
            Dictionary<string, string> allPostFormats = new Dictionary<string, string>();
            foreach (XElement currentStructMember in allPostFormatsStructElement.Descendants(XmlRPCResponseConstants.MEMBER))
            {
                string value = string.Empty;
                string key = string.Empty;
                key = currentStructMember.Element(XmlRPCResponseConstants.NAME).Value;
                value = currentStructMember.Element(XmlRPCResponseConstants.VALUE).Value;
                allPostFormats.Add(key, value);
            }
            return allPostFormats;
        }

        private List<string> parseSupportedPostFormatsKeys(XElement arrayElement)
        {
            if (!arrayElement.HasElements)
            {
                throw new XmlRPCParserException(XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_CODE, XmlRPCResponseConstants.XELEMENTMISSINGCHILDELEMENTS_MESSAGE);
            }

            List<string> supportedPostFormatsKeys = new List<string>();
            bool isStandarAvailable = false;
            foreach (XElement currentArrayMember in arrayElement.Descendants(XmlRPCResponseConstants.VALUE))
            {
                string value = currentArrayMember.Value;
                supportedPostFormatsKeys.Add(value);
                if (value.Equals("standard")) isStandarAvailable = true;
            }
            if (!isStandarAvailable) 
                supportedPostFormatsKeys.Add("standard");
            return supportedPostFormatsKeys;
        }

        #endregion

    }
}
