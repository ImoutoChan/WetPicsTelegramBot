using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.Pixiv
{
    public class PixivActivateModelReplyHandler : MessageHandler
    {
        private readonly IPendingPixivRepliesService _pendingPixivRepliesService;

        public PixivActivateModelReplyHandler(ITgClient tgClient, 
                                              ILogger<PixivActivateModelReplyHandler> logger, 
                                              ICommandsProvider commandsProvider, 
                                              IMessagesProvider messagesProvider,
                                              IPendingPixivRepliesService pendingPixivRepliesService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _pendingPixivRepliesService = pendingPixivRepliesService;
        }

        protected override bool WantHandle(Message message, string command)
            => message.ReplyToMessage != null
               && _pendingPixivRepliesService
                 .AwaitModeReply
                 .ContainsKey(message.ReplyToMessage.MessageId);

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(message.Text, out PixivTopType selectedMode) || !selectedMode.IsDefined())
            {
                await TgClient.Reply(message, MessagesProvider.PixivIncorrectMode, cancellationToken);
                return;
            }

            var mes = await TgClient.Reply(message,
                                           String.Format(MessagesProvider.SelectPixivIntervalMessageF, message.Text),
                                           cancellationToken,
                                           replyMarkup: new ForceReplyMarkup { Selective = true });

            _pendingPixivRepliesService.AwaitIntervalReply.TryAdd(mes.MessageId, selectedMode);
            _pendingPixivRepliesService.AwaitModeReply.TryRemove(message.ReplyToMessage.MessageId, out _);
        }
    }
}