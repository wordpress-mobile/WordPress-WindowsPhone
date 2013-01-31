using System;
using System.Net;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace WordPress.Model
{
    public static class Extensions
    {

        public static string HtmlEncode(this string value)
        {  
            return HttpUtility.HtmlEncode(value);
        }

        public static string HtmlDecode(this string value)
        {
            return HttpUtility.HtmlDecode(value);
        }

        // NOTE: we use unicode entities instead of &amp; &gt; &lt; etc. since some hosts (powweb, fatcow, and similar)
        // have a weird PHP/libxml2 combination that ignores regular entities
        public static string XmlEscape(this string value)
        {
            if (value == null) return null;
            value = value.Replace("&", "&#38;").Replace(">", "&#62;").Replace("<", "&#60;").Replace("\"", "&quot;").Replace("'", "&apos;");
            return value;
        }


        public static string GetValueAsString(this XElement member, bool tryIntFirst)
        {
            string value = null;
            if (tryIntFirst == true)
            {
                try
                {
                    value = member.Descendants(XmlRPCResponseConstants.INT).First().Value;
                }
                catch (Exception)
                {
                    try
                    {

                        value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                try
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                }
                catch (Exception)
                {
                    try
                    {
                        value = member.Descendants(XmlRPCResponseConstants.INT).First().Value;
                    }
                    catch (Exception) { }
                }
            }

            if (value == null)
            {
                throw new Exception("Can't read the value");
            }

            return value;
        }

        public static int GetValueAsInt(this XElement member, bool tryIntFirst)
        {
            string value = null;
            int integerValue = 0;
            if (tryIntFirst == true)
            {
                try
                {
                    value = member.Descendants(XmlRPCResponseConstants.INT).First().Value;
                }
                catch (Exception)
                {
                    try
                    {

                        value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                try
                {
                    value = member.Descendants(XmlRPCResponseConstants.STRING).First().Value;
                }
                catch (Exception)
                {
                    try
                    {
                        value = member.Descendants(XmlRPCResponseConstants.INT).First().Value;
                    }
                    catch (Exception) { }
                }
            }

            if (value == null || !int.TryParse(value, out integerValue))
            {
                throw new Exception("Can't convert the value to int");
            }

            return integerValue;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(this object obj, string message)
        {
            Debug.WriteLine(message);
        }
    }
}