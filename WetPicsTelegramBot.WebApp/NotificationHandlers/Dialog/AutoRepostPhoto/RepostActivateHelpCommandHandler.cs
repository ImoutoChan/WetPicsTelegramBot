﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.AutoRepostPhoto
{
    public class RepostActivateHelpCommandHandler : MessageHandler
    {

        public RepostActivateHelpCommandHandler(ITgClient tgClient,
                                                ILogger<RepostActivateHelpCommandHandler> logger,
                                                ICommandsProvider commandsProvider,
                                                IMessagesProvider messagesProvider)
            : base(tgClient,
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
        }

        protected override bool WantHandle(Message message, string command)
            => command == CommandsProvider.ActivatePhotoRepostHelpCommandText;

        protected override Task Handle(Message message,
                                       string command,
                                       CancellationToken cancellationToken)
            => TgClient.Reply(message, MessagesProvider.RepostHelpMessage, cancellationToken);
    }
}