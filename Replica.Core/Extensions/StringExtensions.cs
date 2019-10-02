using System.Text.RegularExpressions;

namespace Replica.Core.Extensions
{
    public static class StringExtensions
    {
        public static string StripHTML(this string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        public static string EscapeHTML(this string input)
        {
            return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}