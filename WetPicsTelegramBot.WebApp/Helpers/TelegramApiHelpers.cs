using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.WebApp.Models;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Helpers
{
    static class TelegramApiHelpers
    {
        public static string GetBeautyName(this ChatUser user, 
                                           bool disableMention = false, 
                                           bool linkMention = false)
        {
            return GetBeautyName(user.FirstName, 
                                 user.LastName, 
                                 user.Username, 
                                 user.Id, 
                                 disableMention, 
                                 linkMention);
        }
        
        public static string GetBeautyName(this User user, 
                                           bool disableMention = false, 
                                           bool linkMention = false)
        {
            return GetBeautyName(user.FirstName, 
                                 user.LastName, 
                                 user.Username, 
                                 user.Id, 
                                 disableMention, 
                                 linkMention);
        }
        
        public static string GetBeautyName(string firstName, string lastName, string username, int id, bool disableMention = false, bool linkMention = false)
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
            
            if (!String.IsNullOrWhiteSpace(username) && !linkMention)
            {
                var mention = disableMention ? String.Empty : "@";

                userSb.Append(userSb.Length == 0
                                  ? $"{mention}{username}" :
                                  $"({mention}{username})");
            }
            
            if (userSb.Length == 0)
            {
                userSb.Append(id);
            }
            
            if (linkMention)
            {
                userSb.Insert(0, "[");
                userSb.Append($"](tg://user?id={id})");
            }

            var userName = userSb.ToString().Trim();
            return userName;
        }
        

        public static Task<Message> Reply(this ITgClient client,
                                          Message to,
                                          ReplyMessage reply,
                                          CancellationToken cancellationToken,
                                          IReplyMarkup replyMarkup = null)
            => client.Client.SendTextMessageAsync(to.Chat.Id,
                                                    reply.Message,  
                                                    reply.ParseMode,
                                                    replyToMessageId: to.MessageId,
                                                    replyMarkup: replyMarkup,
                                                    cancellationToken: cancellationToken);

        public static Task<Message> Reply(this ITgClient client,
                                          Message to,
                                          string reply,
                                          CancellationToken cancellationToken,
                                          ParseMode parseMode = ParseMode.Default,
                                          IReplyMarkup replyMarkup = null)
            => client.Client.SendTextMessageAsync(to.Chat.Id,
                                                  reply,
                                                  parseMode,
                                                  replyToMessageId: to.MessageId,
                                                  replyMarkup: replyMarkup,
                                                  cancellationToken: cancellationToken);
    }
}
