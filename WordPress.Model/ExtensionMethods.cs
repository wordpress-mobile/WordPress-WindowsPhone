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

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLog(this object obj, string message)
        {
            Debug.WriteLine(message);
        }
    }
}