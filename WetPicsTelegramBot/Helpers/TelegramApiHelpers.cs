using System;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Helpers
{
    static class TelegramApiHelpers
    {
        public static string GetBeautyName(this User user)
        {
            return GetBeautyName(user.FirstName, user.LastName, user.Username, user.Id);
        }

        public static string GetBeautyName(this ChatUser user)
        {
            return GetBeautyName(user.FirstName, user.LastName, user.Username, user.Id);
        }

        public static string GetBeautyName(string firstName, string lastName, string username, int id)
        {
            var userSb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(firstName))
            {
                userSb.Append(firstName + " ");
            }
            if (!String.IsNullOrWhiteSpace(lastName))
            {
                userSb.Append(lastName + " ");
            }
            if (!String.IsNullOrWhiteSpace(username))
            {
                userSb.Append(userSb.Length == 0
                                  ? $"@{username}" :
                                  $"(@{username})");
            }
            if (userSb.Length == 0)
            {
                userSb.Append(id);
            }

            var userName = userSb.ToString().Trim();
            return userName;
        }

        public static async Task<Message> Reply(this ITelegramBotClient api,
                                                 Message message,
                                                 string text,
                                                 ParseMode parseMode = ParseMode.Default,
                                                 IReplyMarkup replyMarkup = null)
        {
            return await api.SendTextMessageAsync(message.Chat.Id,
                                                  text,
                                                  parseMode,
                                                  replyToMessageId: message.MessageId,
                                                  replyMarkup: replyMarkup);
        }
    }
}
