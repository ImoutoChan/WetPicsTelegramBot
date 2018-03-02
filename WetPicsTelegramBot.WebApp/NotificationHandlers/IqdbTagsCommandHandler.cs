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
    public class IqdbTagsCommandHandler : MessageHandler
    {
        private readonly ITgClient _tgClient;
        private readonly ICommandsProvider _commandsProvider;
        private readonly IMessagesProvider _messagesProvider;
        private readonly IIqdbService _iqdbService;

        public IqdbTagsCommandHandler(ITgClient tgClient,
                                      ICommandsProvider commandsProvider,
                                      ILogger<IqdbTagsCommandHandler> logger,
                                      IMessagesProvider messagesProvider,
                                      IIqdbService iqdbService)
            : base(tgClient, logger)
        {
            _tgClient = tgClient;
            _commandsProvider = commandsProvider;
            _messagesProvider = messagesProvider;
            _iqdbService = iqdbService;
        }

        protected override bool WantHandle(Message message, string command)
            => command == _commandsProvider.GetTagsCommandText;

        protected override async Task Handle(Message message, string command, CancellationToken cancellationToken)
        {
            if (message.ReplyToMessage == null || !message.ReplyToMessage.Photo.Any())
            {
                await _tgClient.Reply(message, _messagesProvider.ReplyToImage, cancellationToken);
                return;
            }

            var tagsResult = await _iqdbService.SearchTags(message.ReplyToMessage.Photo.Last().FileId);

            await _tgClient.Reply(message, tagsResult, cancellationToken, ParseMode.Html);
        }
    }
}