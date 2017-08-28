using System;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.Helpers
{
    public static class StringExtensions
    {
        public static string RemoveWhiteSpaces(this string str) 
            => str == null ? null : Regex.Replace(str, @"\s", "");

        public static string GetBeautyName(this User user)
        {
            var userSb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(user.FirstName))
            {
                userSb.Append(user.FirstName + " ");
            }
            if (!String.IsNullOrWhiteSpace(user.LastName))
            {
                userSb.Append(user.LastName + " ");
            }
            if (!String.IsNullOrWhiteSpace(user.Username))
            {
                userSb.Append(userSb.Length == 0
                    ? $"@{user.Username}" :
                    $"(@{user.Username})");
            }
            if (userSb.Length == 0)
            {
                userSb.Append(user.Id);
            }

            var userName = userSb.ToString().Trim();
            return userName;
        }
    }
}