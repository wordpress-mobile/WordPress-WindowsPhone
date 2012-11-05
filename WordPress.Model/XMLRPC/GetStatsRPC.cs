using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml;
using System.Net.NetworkInformation;

namespace WordPress.Model
{
    /// <summary>
    /// Queries the WordPress statistic service for the given statistic type over the selected
    /// time period.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GetStatsRPC<T> where T : INotifyPropertyChanged
    {
        #region member variables

        private static Dictionary<string, string> htmlEntities = getHtmlEntities();

        public static Dictionary<string, string> getHtmlEntities()
        {
            Dictionary<string, string> htmlEntities = new Dictionary<string, string>();
            htmlEntities.Add("&nbsp;", "160");
            htmlEntities.Add("&iexcl;", "161");
            htmlEntities.Add("&cent;", "162");
            htmlEntities.Add("&pound;", "163");
            htmlEntities.Add("&curren;", "164");
            htmlEntities.Add("&yen;", "165");
            htmlEntities.Add("&brvbar;", "166");
            htmlEntities.Add("&sect;", "167");
            htmlEntities.Add("&uml;", "168");
            htmlEntities.Add("&copy;", "169");
            htmlEntities.Add("&ordf;", "170");
            htmlEntities.Add("&laquo;", "171");
            htmlEntities.Add("&not;", "172");
            htmlEntities.Add("&shy;", "173");
            htmlEntities.Add("&reg;", "174");
            htmlEntities.Add("&macr;", "175");
            htmlEntities.Add("&deg;", "176");
            htmlEntities.Add("&plusmn;", "177");
            htmlEntities.Add("&sup2;", "178");
            htmlEntities.Add("&sup3;", "179");
            htmlEntities.Add("&acute;", "180");
            htmlEntities.Add("&micro;", "181");
            htmlEntities.Add("&para;", "182");
            htmlEntities.Add("&middot;", "183");
            htmlEntities.Add("&cedil;", "184");
            htmlEntities.Add("&sup1;", "185");
            htmlEntities.Add("&ordm;", "186");
            htmlEntities.Add("&raquo;", "187");
            htmlEntities.Add("&frac14;", "188");
            htmlEntities.Add("&frac12;", "189");
            htmlEntities.Add("&frac34;", "190");
            htmlEntities.Add("&iquest;", "191");
            htmlEntities.Add("&Agrave;", "192");
            htmlEntities.Add("&Aacute;", "193");
            htmlEntities.Add("&Acirc;", "194");
            htmlEntities.Add("&Atilde;", "195");
            htmlEntities.Add("&Auml;", "196");
            htmlEntities.Add("&Aring;", "197");
            htmlEntities.Add("&AElig;", "198");
            htmlEntities.Add("&Ccedil;", "199");
            htmlEntities.Add("&Egrave;", "200");
            htmlEntities.Add("&Eacute;", "201");
            htmlEntities.Add("&Ecirc;", "202");
            htmlEntities.Add("&Euml;", "203");
            htmlEntities.Add("&Igrave;", "204");
            htmlEntities.Add("&Iacute;", "205");
            htmlEntities.Add("&Icirc;", "206");
            htmlEntities.Add("&Iuml;", "207");
            htmlEntities.Add("&ETH;", "208");
            htmlEntities.Add("&Ntilde;", "209");
            htmlEntities.Add("&Ograve;", "210");
            htmlEntities.Add("&Oacute;", "211");
            htmlEntities.Add("&Ocirc;", "212");
            htmlEntities.Add("&Otilde;", "213");
            htmlEntities.Add("&Ouml;", "214");
            htmlEntities.Add("&times;", "215");
            htmlEntities.Add("&Oslash;", "216");
            htmlEntities.Add("&Ugrave;", "217");
            htmlEntities.Add("&Uacute;", "218");
            htmlEntities.Add("&Ucirc;", "219");
            htmlEntities.Add("&Uuml;", "220");
            htmlEntities.Add("&Yacute;", "221");
            htmlEntities.Add("&THORN;", "222");
            htmlEntities.Add("&szlig;", "223");
            htmlEntities.Add("&agrave;", "224");
            htmlEntities.Add("&aacute;", "225");
            htmlEntities.Add("&acirc;", "226");
            htmlEntities.Add("&atilde;", "227");
            htmlEntities.Add("&auml;", "228");
            htmlEntities.Add("&aring;", "229");
            htmlEntities.Add("&aelig;", "230");
            htmlEntities.Add("&ccedil;", "231");
            htmlEntities.Add("&egrave;", "232");
            htmlEntities.Add("&eacute;", "233");
            htmlEntities.Add("&ecirc;", "234");
            htmlEntities.Add("&euml;", "235");
            htmlEntities.Add("&igrave;", "236");
            htmlEntities.Add("&iacute;", "237");
            htmlEntities.Add("&icirc;", "238");
            htmlEntities.Add("&iuml;", "239");
            htmlEntities.Add("&eth;", "240");
            htmlEntities.Add("&ntilde;", "241");
            htmlEntities.Add("&ograve;", "242");
            htmlEntities.Add("&oacute;", "243");
            htmlEntities.Add("&ocirc;", "244");
            htmlEntities.Add("&otilde;", "245");
            htmlEntities.Add("&ouml;", "246");
            htmlEntities.Add("&divide;", "247");
            htmlEntities.Add("&oslash;", "248");
            htmlEntities.Add("&ugrave;", "249");
            htmlEntities.Add("&uacute;", "250");
            htmlEntities.Add("&ucirc;", "251");
            htmlEntities.Add("&uuml;", "252");
            htmlEntities.Add("&yacute;", "253");
            htmlEntities.Add("&thorn;", "254");
            htmlEntities.Add("&yuml;", "255");
            htmlEntities.Add("&fnof;", "402");
            htmlEntities.Add("&Alpha;", "913");
            htmlEntities.Add("&Beta;", "914");
            htmlEntities.Add("&Gamma;", "915");
            htmlEntities.Add("&Delta;", "916");
            htmlEntities.Add("&Epsilon;", "917");
            htmlEntities.Add("&Zeta;", "918");
            htmlEntities.Add("&Eta;", "919");
            htmlEntities.Add("&Theta;", "920");
            htmlEntities.Add("&Iota;", "921");
            htmlEntities.Add("&Kappa;", "922");
            htmlEntities.Add("&Lambda;", "923");
            htmlEntities.Add("&Mu;", "924");
            htmlEntities.Add("&Nu;", "925");
            htmlEntities.Add("&Xi;", "926");
            htmlEntities.Add("&Omicron;", "927");
            htmlEntities.Add("&Pi;", "928");
            htmlEntities.Add("&Rho;", "929");
            htmlEntities.Add("&Sigma;", "931");
            htmlEntities.Add("&Tau;", "932");
            htmlEntities.Add("&Upsilon;", "933");
            htmlEntities.Add("&Phi;", "934");
            htmlEntities.Add("&Chi;", "935");
            htmlEntities.Add("&Psi;", "936");
            htmlEntities.Add("&Omega;", "937");
            htmlEntities.Add("&alpha;", "945");
            htmlEntities.Add("&beta;", "946");
            htmlEntities.Add("&gamma;", "947");
            htmlEntities.Add("&delta;", "948");
            htmlEntities.Add("&epsilon;", "949");
            htmlEntities.Add("&zeta;", "950");
            htmlEntities.Add("&eta;", "951");
            htmlEntities.Add("&theta;", "952");
            htmlEntities.Add("&iota;", "953");
            htmlEntities.Add("&kappa;", "954");
            htmlEntities.Add("&lambda;", "955");
            htmlEntities.Add("&mu;", "956");
            htmlEntities.Add("&nu;", "957");
            htmlEntities.Add("&xi;", "958");
            htmlEntities.Add("&omicron;", "959");
            htmlEntities.Add("&pi;", "960");
            htmlEntities.Add("&rho;", "961");
            htmlEntities.Add("&sigmaf;", "962");
            htmlEntities.Add("&sigma;", "963");
            htmlEntities.Add("&tau;", "964");
            htmlEntities.Add("&upsilon;", "965");
            htmlEntities.Add("&phi;", "966");
            htmlEntities.Add("&chi;", "967");
            htmlEntities.Add("&psi;", "968");
            htmlEntities.Add("&omega;", "969");
            htmlEntities.Add("&thetasym;", "977");
            htmlEntities.Add("&upsih;", "978");
            htmlEntities.Add("&piv;", "982");
            htmlEntities.Add("&bull;", "8226");
            htmlEntities.Add("&hellip;", "8230");
            htmlEntities.Add("&prime;", "8242");
            htmlEntities.Add("&Prime;", "8243");
            htmlEntities.Add("&oline;", "8254");
            htmlEntities.Add("&frasl;", "8260");
            htmlEntities.Add("&weierp;", "8472");
            htmlEntities.Add("&image;", "8465");
            htmlEntities.Add("&real;", "8476");
            htmlEntities.Add("&trade;", "8482");
            htmlEntities.Add("&alefsym;", "8501");
            htmlEntities.Add("&larr;", "8592");
            htmlEntities.Add("&uarr;", "8593");
            htmlEntities.Add("&rarr;", "8594");
            htmlEntities.Add("&darr;", "8595");
            htmlEntities.Add("&harr;", "8596");
            htmlEntities.Add("&crarr;", "8629");
            htmlEntities.Add("&lArr;", "8656");
            htmlEntities.Add("&uArr;", "8657");
            htmlEntities.Add("&rArr;", "8658");
            htmlEntities.Add("&dArr;", "8659");
            htmlEntities.Add("&hArr;", "8660");
            htmlEntities.Add("&forall;", "8704");
            htmlEntities.Add("&part;", "8706");
            htmlEntities.Add("&exist;", "8707");
            htmlEntities.Add("&empty;", "8709");
            htmlEntities.Add("&nabla;", "8711");
            htmlEntities.Add("&isin;", "8712");
            htmlEntities.Add("&notin;", "8713");
            htmlEntities.Add("&ni;", "8715");
            htmlEntities.Add("&prod;", "8719");
            htmlEntities.Add("&sum;", "8721");
            htmlEntities.Add("&minus;", "8722");
            htmlEntities.Add("&lowast;", "8727");
            htmlEntities.Add("&radic;", "8730");
            htmlEntities.Add("&prop;", "8733");
            htmlEntities.Add("&infin;", "8734");
            htmlEntities.Add("&ang;", "8736");
            htmlEntities.Add("&and;", "8743");
            htmlEntities.Add("&or;", "8744");
            htmlEntities.Add("&cap;", "8745");
            htmlEntities.Add("&cup;", "8746");
            htmlEntities.Add("&int;", "8747");
            htmlEntities.Add("&there4;", "8756");
            htmlEntities.Add("&sim;", "8764");
            htmlEntities.Add("&cong;", "8773");
            htmlEntities.Add("&asymp;", "8776");
            htmlEntities.Add("&ne;", "8800");
            htmlEntities.Add("&equiv;", "8801");
            htmlEntities.Add("&le;", "8804");
            htmlEntities.Add("&ge;", "8805");
            htmlEntities.Add("&sub;", "8834");
            htmlEntities.Add("&sup;", "8835");
            htmlEntities.Add("&nsub;", "8836");
            htmlEntities.Add("&sube;", "8838");
            htmlEntities.Add("&supe;", "8839");
            htmlEntities.Add("&oplus;", "8853");
            htmlEntities.Add("&otimes;", "8855");
            htmlEntities.Add("&perp;", "8869");
            htmlEntities.Add("&sdot;", "8901");
            htmlEntities.Add("&lceil;", "8968");
            htmlEntities.Add("&rceil;", "8969");
            htmlEntities.Add("&lfloor;", "8970");
            htmlEntities.Add("&rfloor;", "8971");
            htmlEntities.Add("&lang;", "9001");
            htmlEntities.Add("&rang;", "9002");
            htmlEntities.Add("&loz;", "9674");
            htmlEntities.Add("&spades;", "9824");
            htmlEntities.Add("&clubs;", "9827");
            htmlEntities.Add("&hearts;", "9829");
            htmlEntities.Add("&diams;", "9830");
            htmlEntities.Add("&quot;", "34");
            htmlEntities.Add("&amp;", "38");
            htmlEntities.Add("&lt;", "60");
            htmlEntities.Add("&gt;", "62");
            htmlEntities.Add("&OElig;", "338");
            htmlEntities.Add("&oelig;", "339");
            htmlEntities.Add("&Scaron;", "352");
            htmlEntities.Add("&scaron;", "353");
            htmlEntities.Add("&Yuml;", "376");
            htmlEntities.Add("&circ;", "710");
            htmlEntities.Add("&tilde;", "732");
            htmlEntities.Add("&ensp;", "8194");
            htmlEntities.Add("&emsp;", "8195");
            htmlEntities.Add("&thinsp;", "8201");
            htmlEntities.Add("&zwnj;", "8204");
            htmlEntities.Add("&zwj;", "8205");
            htmlEntities.Add("&lrm;", "8206");
            htmlEntities.Add("&rlm;", "8207");
            htmlEntities.Add("&ndash;", "8211");
            htmlEntities.Add("&mdash;", "8212");
            htmlEntities.Add("&lsquo;", "8216");
            htmlEntities.Add("&rsquo;", "8217");
            htmlEntities.Add("&sbquo;", "8218");
            htmlEntities.Add("&ldquo;", "8220");
            htmlEntities.Add("&rdquo;", "8221");
            htmlEntities.Add("&bdquo;", "8222");
            htmlEntities.Add("&dagger;", "8224");
            htmlEntities.Add("&Dagger;", "8225");
            htmlEntities.Add("&permil;", "8240");
            htmlEntities.Add("&lsaquo;", "8249");
            htmlEntities.Add("&rsaquo;", "8250");
            htmlEntities.Add("&euro;", "8364");

            return htmlEntities;
        }


        protected const string STATSURL_FORMATSTRING = "http://stats.wordpress.com/api/1.0/?api_key={0}&blog_uri={1}&format=xml&table={2}&end={3}&days={4}&limit=-1{5}";

        private SendOrPostCallback onCompletedDelegate;

        #endregion

        #region events

        public event XMLRPCCompletedEventHandler<T> Completed;

        #endregion

        #region constructors

        public GetStatsRPC()
        {
            onCompletedDelegate = new SendOrPostCallback(NotifyCompleted);
        }

        public GetStatsRPC(Blog blog)
            : this()
        {
            Blog = blog;
        }

        #endregion

        #region properties

        public Blog Blog { get; private set; }

        public eStatisticPeriod StatisicPeriod { get; set; }

        public eStatisticType StatisticType { get; set; }

        protected virtual Uri Uri
        {
            get
            {
                string url = string.Format(STATSURL_FORMATSTRING,
                    Blog.ApiKey,
                    Blog.Url,
                    StatisticType.ToString().ToLower(),
                    DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString(),
                    Days,
                    Period
                    );

                return new Uri(url, UriKind.Absolute);
            }
        }

        private int Days
        {
            get
            {
                switch (StatisicPeriod)
                {
                    case eStatisticPeriod.LastWeek:
                        return 7;
                    case eStatisticPeriod.LastMonth:
                        return 30;
                    case eStatisticPeriod.LastQuarter:
                        return 12;
                    case eStatisticPeriod.LastYear:
                        return 11;
                    case eStatisticPeriod.AllTime:
                        return -1;
                    default:
                        return -1;
                }
            }
        }

        private string Period
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                switch (StatisicPeriod)
                {
                    case eStatisticPeriod.LastWeek:
                        //do nothing here
                        break;
                    case eStatisticPeriod.LastMonth:
                        //do nothing here
                        break;
                    case eStatisticPeriod.LastQuarter:
                        builder.Append("&period=week");
                        break;
                    case eStatisticPeriod.LastYear:
                        builder.Append("&period=month");
                        break;
                    case eStatisticPeriod.AllTime:
                        builder.Append("&period=month");
                        break;
                    default:
                        break;
                }

                return builder.ToString();
            }
        }

        #endregion

        #region methods

        private void CompletionMethod(List<T> items, Exception exception, bool canceled, AsyncOperation asyncOp)
        {
            //package the results of the operation in an XMLRPCCompletedEventArgs object
            XMLRPCCompletedEventArgs<T> args = new XMLRPCCompletedEventArgs<T>(items, exception, canceled, asyncOp.UserSuppliedState);

            asyncOp.PostOperationCompleted(onCompletedDelegate, args);
        }

        private void NotifyCompleted(object state)
        {
            XMLRPCCompletedEventArgs<T> args = state as XMLRPCCompletedEventArgs<T>;
            if (null != Completed)
            {
                Completed(this, args);
            }
        }

        protected virtual void ValidateValues()
        {
            if (null == Blog)
            {
                throw new ArgumentException("Blog may not be null", "Blog");
            }
            if (string.IsNullOrEmpty(Blog.ApiKey))
            {
                throw new ArgumentException("Blog.ApiKey may not be null or empty", "Blog.ApiKey");
            }
        }

        public void ExecuteAsync()
        {
            ExecuteAsync(Guid.NewGuid());
        }

        public void ExecuteAsync(object taskId)
        {
            ValidateValues();

            AsyncOperation operation = AsyncOperationManager.CreateOperation(taskId);

            ThreadPool.QueueUserWorkItem((state) =>
            {
                FetchStats(operation);
            }, operation);
        }

        private void FetchStats(AsyncOperation operation)
        {

            bool hasNetworkConnection = NetworkInterface.GetIsNetworkAvailable();
            if (!hasNetworkConnection)
            {
                Exception connErr = new NoConnectionException();
                CompletionMethod(null, connErr, false, operation);
                return;
            }

            HttpWebRequest request = HttpWebRequest.Create(Uri) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.ContentType = XmlRPCRequestConstants.CONTENTTYPE;
            request.Method = XmlRPCRequestConstants.POST;
            request.UserAgent = Constants.WORDPRESS_USERAGENT;

            request.BeginGetResponse(responseResult =>
            {
                try
                {
                    HttpWebResponse response = request.EndGetResponse(responseResult) as HttpWebResponse;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseContent = reader.ReadToEnd();
                        XDocument xDoc = ParseDocument(responseContent);

                        Exception exception = null;
                        List<T> items = null;

                        var fault = xDoc.Descendants().Where(element => XmlRPCResponseConstants.NAME == element.Name && XmlRPCResponseConstants.FAULTCODE_VALUE == element.Value);
                        if (null != fault && 0 < fault.Count())
                        {
                            exception = ParseFailureInfo(xDoc.Descendants(XmlRPCResponseConstants.STRUCT).First());
                        }
                        else
                        {
                            try
                            {
                                items = ParseResponseContent(xDoc);
                            }
                            catch (Exception ex)
                            {
                                exception = ex;
                            }
                        }

                        CompletionMethod(items, exception, false, operation);
                    }
                }
                catch (Exception ex)
                {
                    CompletionMethod(new List<T>(), ex, false, operation);
                }
            }, request);
        }

        private XDocument ParseDocument(string content)
        {
            XDocument xDoc = null;
            try
            {
                xDoc = XDocument.Parse(content, LoadOptions.None);
            }
            catch (Exception outerEx)
            {
                //Re-parse the response after some cleaning stuff
                string originalServerResponse = null; //Keep a copy of the server response "as-is", without cleaning it.
                if (!String.IsNullOrEmpty(content))
                {
                    originalServerResponse = String.Copy(content);
                    Regex regex = new Regex("\n", RegexOptions.IgnoreCase);
                    content = regex.Replace(content, String.Empty);
                    regex = new Regex("\t", RegexOptions.IgnoreCase);
                    content = regex.Replace(content, String.Empty);

                    //Use numeric entities instead of the named version by looping over pairs with foreach
                    foreach (KeyValuePair<string, string> pair in htmlEntities)
                    {
                        content = content.Replace( pair.Key, "&#" + pair.Value + ";" );
                    }

                    //remove the un-replaced entities
                    regex = new Regex("&([a-zA-Z]+;)", RegexOptions.IgnoreCase);
                    content = regex.Replace(content, String.Empty);

                    //note: We are not removing 'non-utf-8 characters'. We are removing utf-8 characters that may not appear in well-formed XML documents.
                    string pattern = @"#x((10?|[2-F])FFF[EF]|FDD[0-9A-F]|[19][0-9A-F]|7F|8[0-46-9A-F]|0?[1-8BCEF])";
                    regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    if (regex.IsMatch(content))
                    {
                        this.DebugLog("found characters that must not appear in the XML-RPC response");
                        content = regex.Replace(content, String.Empty);
                    }

                    if (!content.StartsWith("<?xml"))
                    {
                        //clean the junk b4 the xml preamble
                        this.DebugLog("cleaning the junk before the xml preamble");
                        int indexOfFirstLt = content.IndexOf("<?xml");
                        if (indexOfFirstLt > -1)
                            content = content.Substring(indexOfFirstLt);
                    }
                }

                try
                {
                    xDoc = XDocument.Parse(content, LoadOptions.None);
                }
                catch (Exception ex)
                {

                    //something went wrong during the parsing process
                    this.DebugLog("Parser error: " + ex.Message); //this is the original error, that should not be shown to the user.

                    if (ex is XmlException)
                    {
                        //most likely case here is that the API key is not for an administrator's account.
                        //try to read the error from the response
                        string errorDescription = ParseErrorDescription(originalServerResponse);
                        if (!string.IsNullOrEmpty(errorDescription))
                        {
                            throw new Exception(errorDescription, ex);
                        }
                    }

                    //Keep track of the original exception by adding the response from the server. If the recovery fails we should throw this exception.
                    if (!String.IsNullOrEmpty(originalServerResponse))
                    {
                        ex = new Exception("\n Server Response --> " + originalServerResponse, ex);
                    }
                    throw new XmlRPCParserException(XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_CODE, XmlRPCResponseConstants.SERVER_RETURNED_INVALID_XML_RPC_MESSAGE, ex);
                }
            }
            return xDoc;
        }

        private string ParseErrorDescription(string content)
        {
            string line = string.Empty;
            string result = string.Empty;
            string error = "Error:";

            using (StringReader reader = new StringReader(content))
            {
                line = reader.ReadLine();

                //TODO: will the string be I18N'ed?
                if (line.StartsWith(error))
                {
                    result = line.Replace(error, string.Empty).Trim();
                }
            }
            return result;
        }

        private Exception ParseFailureInfo(XElement element)
        {
            int faultCode = -1;
            string message = string.Empty;

            XElement valueElement = null;
            foreach (XElement nameElement in element.Descendants(XmlRPCResponseConstants.NAME))
            {
                if (XmlRPCResponseConstants.FAULTCODE_VALUE.Equals(nameElement.Value))
                {
                    valueElement = ((XElement)nameElement.NextNode).DescendantNodes().First() as XElement;
                    if (!int.TryParse(valueElement.Value, out faultCode))
                    {
                        return new ArgumentException("Unable to parse fault code from response");
                    }
                }
                else if (XmlRPCResponseConstants.FAULTSTRING_VALUE.Equals(nameElement.Value))
                {
                    valueElement = ((XElement)nameElement.NextNode).DescendantNodes().First() as XElement;
                    message = valueElement.Value;
                }
            }

            return new XmlRPCException(faultCode, message);
        }

        protected abstract List<T> ParseResponseContent(XDocument xDoc);

        #endregion
    }
}
