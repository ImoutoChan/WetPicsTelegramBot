using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.Pixiv
{
    public class PixivActivateIntervalReplyHandler : MessageHandler
    {
        private readonly IAwaitedRepliesService _pendingPixivRepliesService;
        private readonly IPixivSettingsService _pixivSettingsService;

        public PixivActivateIntervalReplyHandler(ITgClient tgClient, 
                                                 ILogger<PixivActivateIntervalReplyHandler> logger, 
                                                 ICommandsProvider commandsProvider, 
                                                 IMessagesProvider messagesProvider,
                                                 IAwaitedRepliesService pendingPixivRepliesService,
                                                 IPixivSettingsService pixivSettingsService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _pendingPixivRepliesService = pendingPixivRepliesService;
            _pixivSettingsService = pixivSettingsService;
        }

        protected override bool WantHandle(Message message, string command)
            => message.ReplyToMessage != null
               && _pendingPixivRepliesService
                 .AwaitIntervalReply
                 .ContainsKey(message.ReplyToMessage.MessageId);

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var repliedTo = message.ReplyToMessage.MessageId;

            if (!_pendingPixivRepliesService
                .AwaitIntervalReply
                .TryGetValue(repliedTo, out var mode)
                || !int.TryParse(message.Text, out var interval))
            {
                await TgClient.Reply(message, MessagesProvider.PixivIncorrectInterval, cancellationToken);
                return;
            }

            await _pixivSettingsService.Add(message.Chat.Id, mode, interval);

            await TgClient.Reply(message, MessagesProvider.PixivWasActivated, cancellationToken);

            _pendingPixivRepliesService.AwaitIntervalReply.TryRemove(repliedTo, out _);
        }
    }
}