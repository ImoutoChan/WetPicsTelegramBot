using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers
{
    public class StartMessageHandler : MessageHandler
    {
        private readonly ITgClient _tgClient;
        private readonly ICommandsProvider _commandsProvider;
        private readonly IMessagesProvider _messagesProvider;

        public StartMessageHandler(ITgClient tgClient, 
                                      ICommandsProvider commandsProvider, 
                                      ILogger<StartMessageHandler> logger,
                                      IMessagesProvider messagesProvider) 
            : base(tgClient, logger)
        {
            _tgClient = tgClient;
            _commandsProvider = commandsProvider;
            _messagesProvider = messagesProvider;
        }

        protected override bool WantHandle(Message message, string command)
        {
            return command == _commandsProvider.StartCommandText 
                   || command == _commandsProvider.HelpCommandText;
        }

        protected override Task Handle(Message message,
                                       string command,
                                       CancellationToken cancellationToken) 
            => _tgClient.Reply(message, _messagesProvider.HelpMessage, cancellationToken);
    }
}