using System;
using System.Text;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot
{
    public static class Helpers
    {
        public static string GetBeautyName(this Message message)
        {
            var userSb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(message.From.FirstName))
            {
                userSb.Append(message.From.FirstName + " ");
            }
            if (!String.IsNullOrWhiteSpace(message.From.LastName))
            {
                userSb.Append(message.From.LastName + " ");
            }
            if (!String.IsNullOrWhiteSpace(message.From.Username))
            {
                userSb.Append(userSb.Length == 0
                    ? $"@{message.From.Username}" :
                    $"(@{message.From.Username})");
            }
            if (userSb.Length == 0)
            {
                userSb.Append(message.From.Id);
            }

            var userName = userSb.ToString().Trim();
            return userName;
        }
    }
}