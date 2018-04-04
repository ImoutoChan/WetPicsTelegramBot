using System.Text.RegularExpressions;
using Imouto.BooruParser.Loaders;

namespace WetPicsTelegramBot.WebApp.Helpers
{
    public static class StringExtensions
    {
        public static string RemoveWhiteSpaces(this string str) 
            => str == null ? null : Regex.Replace(str, @"\s", "");
        
        public static string MakeAdverb(this PopularType type) 
            => type == PopularType.Day
                ? "Daily" 
                : $"{type}ly";
    }
}