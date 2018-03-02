using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers
{
    public class IqdbSearchCommandHandler : MessageHandler
    {
        private readonly ITgClient _tgClient;
        private readonly ICommandsProvider _commandsProvider;
        private readonly IMessagesProvider _messagesProvider;

        public IqdbSearchCommandHandler(ITgClient tgClient,
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
            => command == _commandsProvider.SearchIqdbCommandText;

        protected override async Task Handle(Message message, string command, CancellationToken cancellationToken)
        {
            if (message.ReplyToMessage == null || !message.ReplyToMessage.Photo.Any())
            {
                await _tgClient.Reply(message, _messagesProvider.ReplyToImage, cancellationToken);
                return;
            }

            var searchResults = await _iqdbService.SearchImage(message.ReplyToMessage.Photo.Last().FileId);

            await _baseDialogService.Reply(message, searchResults, parseMode: ParseMode.Html);
        }
    }
}