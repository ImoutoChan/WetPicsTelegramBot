using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv
{
    public class SelectPixivImageSourceReplyHandler : ReplyHandler
    {
        public SelectPixivImageSourceReplyHandler(ITgClient tgClient, 
                                                  ILogger<SelectPixivImageSourceReplyHandler> logger, 
                                                  ICommandsProvider commandsProvider, 
                                                  IMessagesProvider messagesProvider, 
                                                  IAwaitedRepliesService awaitedRepliesService) 
            : base(tgClient, logger, commandsProvider, messagesProvider, awaitedRepliesService)
        {
        }

        protected override bool WantHandle(Message message, string command)
            => IsMessageAwaited(message) 
                && Enum.TryParse(message.Text, out ImageSource imageSource) 
                && imageSource == ImageSource.Pixiv;

        protected override async Task Handle(Message message, 
                                             string command, 
                                             CancellationToken cancellationToken)
        {
            var mes = await TgClient.Reply(message,
                                           MessagesProvider.SelectPixivModeMessage,
                                           cancellationToken,
                                           replyMarkup: GetPixivModesKeyboard());

            AwaitedRepliesService.AwaitedReplies.TryRemove(message.ReplyToMessage.MessageId, out _);
            AwaitedRepliesService.AwaitedReplies.TryAdd(mes.MessageId, new PixivModeAwaitedReply());
        }

        private ReplyKeyboardMarkup GetPixivModesKeyboard()
        {
            var buttons = Enum
               .GetNames(typeof(PixivTopType))
               .Select(x => new KeyboardButton(x))
               .ToList();

            return new ReplyKeyboardMarkup(new[] 
                {
                    buttons.Take(6).ToArray(),
                    buttons.Skip(6).ToArray(),
                },
                resizeKeyboard: true,
                oneTimeKeyboard: true)
            {
                Selective = true
            };
        }
    }
}