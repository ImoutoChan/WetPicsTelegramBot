using System;
using System.Text;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot
{
    public static class Helpers
    {
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