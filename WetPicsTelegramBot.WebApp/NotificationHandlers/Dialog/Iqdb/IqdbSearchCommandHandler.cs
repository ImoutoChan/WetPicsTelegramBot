﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.Iqdb
{
    public class IqdbSearchCommandHandler : MessageHandler
    {
        private readonly IIqdbService _iqdbService;

        public IqdbSearchCommandHandler(ITgClient tgClient,
                                        ICommandsProvider commandsProvider,
                                        ILogger<IqdbSearchCommandHandler> logger,
                                        IMessagesProvider messagesProvider,
                                        IIqdbService iqdbService)
            : base(tgClient,
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
            _iqdbService = iqdbService;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.SearchIqdbCommandText;

        protected override async Task Handle(Message message, string command, CancellationToken cancellationToken)
        {
            if (message.ReplyToMessage == null || !message.ReplyToMessage.Photo.Any())
            {
                await TgClient.Reply(message, MessagesProvider.ReplyToImage, cancellationToken);
                return;
            }

            var searchResults = await _iqdbService.SearchImage(message.ReplyToMessage.Photo.Last().FileId);

            await TgClient.Reply(message, searchResults, cancellationToken, ParseMode.Html);
        }
    }
}