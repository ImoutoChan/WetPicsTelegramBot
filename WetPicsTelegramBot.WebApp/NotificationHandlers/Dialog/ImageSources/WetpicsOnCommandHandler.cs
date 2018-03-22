using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources
{
    public class WetpicsOnCommandHandler : MessageHandler
    {
        private readonly IAwaitedRepliesService _awaitedRepliesService;

        public WetpicsOnCommandHandler(ITgClient tgClient, 
            ILogger<WetpicsOnCommandHandler> logger, 
            ICommandsProvider commandsProvider, 
            IMessagesProvider messagesProvider,
            IAwaitedRepliesService awaitedRepliesService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _awaitedRepliesService = awaitedRepliesService;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.WetpicsOn;

        protected override async Task Handle(Message message,
            string command,
            CancellationToken cancellationToken)
        {
            
            var mes = 
                await TgClient.Reply(message, 
                                    MessagesProvider.SelectWetpicsInterval,
                                    cancellationToken);

            _awaitedRepliesService.AwaitedReplies
                .TryAdd(mes.MessageId, new WetpicsIntervalAwaitedReply());
        }
    }
}