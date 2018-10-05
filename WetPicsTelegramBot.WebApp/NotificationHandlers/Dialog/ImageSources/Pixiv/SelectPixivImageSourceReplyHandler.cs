using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
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
                                           MessagesProvider.SelectModeMessage,
                                           cancellationToken,
                                           replyMarkup: TgClient.GetReplyKeyboardFromEnum<PixivTopType>());

            AwaitedRepliesService.AwaitedReplies.TryRemove(FoundReplyTo.Value, out _);
            AwaitedRepliesService.AwaitedReplies.TryAdd(mes.MessageId, new PixivModeAwaitedReply());
        }
    }
}