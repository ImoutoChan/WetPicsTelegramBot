using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Danbooru
{
    public class SelectDanbooruImageSourceReplyHandler : ReplyHandler
    {
        public SelectDanbooruImageSourceReplyHandler(ITgClient tgClient, 
                                                     ILogger<SelectDanbooruImageSourceReplyHandler> logger, 
                                                     ICommandsProvider commandsProvider, 
                                                     IMessagesProvider messagesProvider, 
                                                     IAwaitedRepliesService awaitedRepliesService) 
            : base(tgClient, logger, commandsProvider, messagesProvider, awaitedRepliesService)
        {
        }

        protected override bool WantHandle(Message message, string command)
            => IsMessageAwaited(message) 
                && Enum.TryParse(message.Text, out ImageSource imageSource) 
                && imageSource == ImageSource.Danbooru;

        protected override async Task Handle(Message message, 
                                             string command, 
                                             CancellationToken cancellationToken)
        {
            var mes = await TgClient.Reply(message,
                                           MessagesProvider.SelectModeMessage,
                                           cancellationToken,
                                           replyMarkup: TgClient.GetReplyKeyboardFromEnum<DanbooruTopType>());

            AwaitedRepliesService.AwaitedReplies.TryRemove(message.ReplyToMessage.MessageId, out _);
            AwaitedRepliesService.AwaitedReplies.TryAdd(mes.MessageId, new DanbooruModeAwaitedReply());
        }
    }
}