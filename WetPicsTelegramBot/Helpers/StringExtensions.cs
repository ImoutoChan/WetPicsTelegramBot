using System.Text.RegularExpressions;

namespace WetPicsTelegramBot.Helpers
{
    public static class StringExtensions
    {
        public static string RemoveWhiteSpaces(this string str)
        {
            return Regex.Replace(str, @"\s", "");
        }
    }
}