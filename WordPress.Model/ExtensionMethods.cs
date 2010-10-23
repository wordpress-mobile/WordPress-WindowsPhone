using System;
using System.Net;

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
    }
}