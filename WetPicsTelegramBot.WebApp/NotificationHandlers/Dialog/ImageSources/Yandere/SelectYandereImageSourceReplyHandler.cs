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

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Yandere
{
    public class SelectYandereImageSourceReplyHandler : ReplyHandler
    {
        public SelectYandereImageSourceReplyHandler(ITgClient tgClient, 
                                                    ILogger<SelectYandereImageSourceReplyHandler> logger, 
                                                    ICommandsProvider commandsProvider, 
                                                    IMessagesProvider messagesProvider, 
                                                    IAwaitedRepliesService awaitedRepliesService) 
            : base(tgClient, logger, commandsProvider, messagesProvider, awaitedRepliesService)
        {
        }

        protected override bool WantHandle(Message message, string command)
            => IsMessageAwaited(message) 
                && Enum.TryParse(message.Text, out ImageSource imageSource) 
                && imageSource == ImageSource.Yandere;

        protected override async Task Handle(Message message, 
                                             string command, 
                                             CancellationToken cancellationToken)
        {
            var mes = await TgClient.Reply(message,
                                           MessagesProvider.SelectModeMessage,
                                           cancellationToken,
                                           TgClient.GetReplyKeyboardFromEnum<YandereTopType>());

            AwaitedRepliesService.AwaitedReplies.TryRemove(message.ReplyToMessage.MessageId, out _);
            AwaitedRepliesService.AwaitedReplies.TryAdd(mes.MessageId, new YandereModeAwaitedReply());
        }
    }
}