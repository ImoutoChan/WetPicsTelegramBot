using System.Text.RegularExpressions;

namespace WetPicsTelegramBot.WebApp.Helpers
{
    public static class StringExtensions
    {
        public static string RemoveWhiteSpaces(this string str) 
            => str == null ? null : Regex.Replace(str, @"\s", "");
    }
}