using System;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace WetPicsTelegramBot.Helpers
{
    static class TelegramApiHelpers
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
