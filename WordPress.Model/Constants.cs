namespace WordPress.Model
{
    public class Constants
    {
        public const string WORDPRESS_XMLRPC_URL = "https://wordpress.com/xmlrpc.php";
        public const string WORDPRESS_LOGIN_URL = "http://wordpress.com/wp-login.php";
        public const string WORDPRESS_READER_URL = "http://wordpress.com/reader/mobile/";
        public const string WORDPRESS_APIKEY_URL = "https://public-api.wordpress.com/get-user-blogs/1.0";
        public const string WORDPRESS_SIGNUP_URL = "http://wordpress.com/signup";
        public const string WORDPRESS_FORUMS_URL = "http://windowsphone.forums.wordpress.org/";
        public const string WORDPRESS_FAQ_URL = "http://windowsphone.wordpress.org/faq/";
        public const string WORDPRESS_PRIVACY_URL = "http://automattic.com/privacy/";
        public const string WORDPRESS_TOS_URL = "http://www.wordpress.com/tos/";
        public const string JETPACK_SITE_URL = "http://jetpack.me";
        public const string WORDPRESS_CRASHREPORT_EMAIL = "windowsphone-crashreport@automattic.com";
        public const string WORDPRESS_DATEFORMAT = "yyyyMMddTH:mm:ss";
        public const string WORDPRESS_USERAGENT = "wp-windowsphone/2.1.3";
    }

    public class XmlRPCRequestConstants
    {
        public const string POST = "post";
        public const string CONTENTTYPE = "text/xml";
        public const string DATETIMEFORMATSTRING = "{0:yyyyMMdd}T{0:HH}:{0:mm}:{0:ss}";
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

        public const int SERVER_RETURNED_INVALID_XML_RPC_CODE = -1001;
        public const int XML_RPC_OPERATION_FAILED_CODE = -1002;
        public const int XELEMENTMISSINGCHILDELEMENTS_CODE = -1003;

        public const string SERVER_RETURNED_INVALID_XML_RPC_MESSAGE = "Sorry, the server returned invalid data. Please try again.";
        public const string XML_RPC_OPERATION_FAILED_MESSAGE = "Sorry, the requested action failed. Please try again.";
        public const string XELEMENTMISSINGCHILDELEMENTS_MESSAGE = "Sorry, the server returned invalid data. Please try again."; 
                 
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
        public const string UL_OPENING_TAG = "<ul>";
        public const string UL_CLOSING_TAG = "</ul>";
        public const string OL_OPENING_TAG = "<ol>";
        public const string OL_CLOSING_TAG = "</ol>";
        public const string LI_OPENING_TAG = "<li>";
        public const string LI_CLOSING_TAG = "</li>";
        public const string CODE_OPENING_TAG = "<code>";
        public const string CODE_CLOSING_TAG = "</code>";

    }

    public class MimeTypes
    {
        public const string JPEG = "image/jpeg";
        public const string PNG = "image/png";
        public const string BMP = "image/bmp";
        public const string UNKNOWN = "application/octet-stream";
    }
}