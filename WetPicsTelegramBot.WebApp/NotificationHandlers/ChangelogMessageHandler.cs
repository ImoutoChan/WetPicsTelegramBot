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
    public class ChangelogMessageHandler : MessageHandler
    {
        private readonly ITgClient _tgClient;
        private readonly ICommandsProvider _commandsProvider;
        private readonly IMessagesProvider _messagesProvider;

        protected ChangelogMessageHandler(ITgClient tgClient,
                                          ICommandsProvider commandsProvider,
                                          ILogger<ChangelogMessageHandler> logger,
                                          IMessagesProvider messagesProvider)
            : base(tgClient, logger)
        {
            _tgClient = tgClient;
            _commandsProvider = commandsProvider;
            _messagesProvider = messagesProvider;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == _commandsProvider.ChangeLogCommandText;

        protected override Task Handle(Message message,
                                       string command,
                                       CancellationToken cancellationToken)
            => _tgClient.Reply(message, _messagesProvider.ChangeLogMessage, cancellationToken);
    }
}