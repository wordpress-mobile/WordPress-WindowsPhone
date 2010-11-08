namespace WordPress.Model
{
    public class Constants
    {
        public const string WORDPRESS_XMLRPC_URL = "https://wordpress.com/xmlrpc.php";
        public const string WORDPRESS_APIKEY_URL = "https://public-api.wordpress.com/get-user-blogs/1.0";
        public const string WORDPRESS_SIGNUP_URL = "http://wordpress.com/signup";
        public const string WORDPRESS_DATEFORMAT = "yyyyMMddTH:mm:ss";
        public const string WORDPRESS_USERAGENT = "wp-windowsphone7/1.0.0";
    }

    public class XmlRPCRequestConstants
    {
        public const string POST = "post";
        public const string CONTENTTYPE = "text/xml";
    }

    public class XmlRPCResponseConstants
    {
        public const string METHODRESPONSE = "methodResponse";
        public const string PARAMS = "params";
        public const string PARAM = "param";
        public const string VALUE = "value";
        public const string ARRAY = "array";
        public const string DATA = "data";
        public const string STRUCT = "struct";
        public const string MEMBER = "member";
        public const string NAME = "name";
        public const string STRING = "string";
        public const string BOOLEAN = "boolean";
        public const string DATETIMEISO8601 = "dateTime.iso8601";
        public const string INT = "int";
        public const string FAULTCODE_VALUE = "faultCode";
        public const string FAULTSTRING_VALUE = "faultString";
        public const string APIKEY = "apikey";
        public const string FALSE_STRING = "0";

        public const string XELEMENTMISSINGCHILDELEMENTS_MESSAGE = "XElement does not contain any child nodes.";
        public const string XELEMENTUNEXPECTEDCHILDNODES_MESSAGE = "XElement contains an unexpected number of child nodes.";
        public const string UNABLETOPARSEEXPECTEDRESPONSE_MESSAGE = "Unable to parse expected value.";
        public const string RPCRETURNEDFAILURE_MESSAGE = "RPC returned a failure.";
    }

    public class WordPressMarkupTags
    {
        public const string BOLD_OPENING_TAG = "<strong>";
        public const string BOLD_CLOSING_TAG = "</strong>";
        public const string ITALICS_OPENING_TAG = "<em>";
        public const string ITALICS_CLOSING_TAG = "</em>";
        public const string UNDERLINE_OPENING_TAG = "<u>";
        public const string UNDERLINE_CLOSING_TAG = "</u>";
        public const string STRIKETHROUGH_OPENING_TAG = "<strike>";
        public const string STRIKETHROUGH_CLOSING_TAG = "</strike>";
        public const string BLOCKQUOTE_OPENING_TAG = "<blockquote>";
        public const string BLOCKQUOTE_CLOSING_TAG = "</blockquote>";
    }
}