using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources
{
    public class WetpicsIntervalReplyHandler : ReplyHandler
    {
        private readonly IWetpicsService _wetpicsService;

        public WetpicsIntervalReplyHandler(ITgClient tgClient, 
            ILogger<WetpicsOnCommandHandler> logger, 
            ICommandsProvider commandsProvider, 
            IMessagesProvider messagesProvider,
            IAwaitedRepliesService awaitedRepliesService,
            IWetpicsService wetpicsService)
            : base(tgClient, logger, commandsProvider, messagesProvider, awaitedRepliesService)
        {
            _wetpicsService = wetpicsService;
        }

        protected override bool WantHandle(Message message, string command) 
            => IsMessageAwaited(message);

        protected override async Task Handle(Message message,
            string command,
            CancellationToken cancellationToken)
        {
            var repliedTo = message.ReplyToMessage.MessageId;

            if (!int.TryParse(message.Text, out var interval))
            {
                await TgClient.Reply(message,
                    MessagesProvider.WetpicsIncorrectInterval,
                    cancellationToken);
                return;
            }

            await _wetpicsService.Enable(message.Chat.Id, interval);

            await TgClient.Reply(message, MessagesProvider.WetpicsWasActivated, cancellationToken);

            AwaitedRepliesService.AwaitedReplies.TryRemove(message.ReplyToMessage.MessageId, out _);
        }
    }
}