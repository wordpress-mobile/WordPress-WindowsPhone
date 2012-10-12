using System;
using System.Net;
using System.Diagnostics;

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

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(this object obj, string message)
        {
            Debug.WriteLine(message);
        }
    }
}