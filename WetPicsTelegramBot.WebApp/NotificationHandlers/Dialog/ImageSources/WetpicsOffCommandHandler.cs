using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources
{
    public class WetpicsOffCommandHandler : MessageHandler
    {
        private readonly IWetpicsService _wetpicsService;

        public WetpicsOffCommandHandler(ITgClient tgClient,
            ILogger<WetpicsOnCommandHandler> logger,
            ICommandsProvider commandsProvider,
            IMessagesProvider messagesProvider,
            IWetpicsService wetpicsService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _wetpicsService = wetpicsService;
        }

        protected override bool WantHandle(Message message, string command)
            => command == CommandsProvider.WetpicsOff;

        protected override async Task Handle(Message message,
            string command,
            CancellationToken cancellationToken)
        {
            await _wetpicsService.Disable(message.Chat.Id);

            await TgClient.Reply(message,
                MessagesProvider.WetpicsWasDeactivated,
                cancellationToken);
        }
    }
}