using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Notifications;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract
{
    public abstract class MessageHandler : IMessageHandler
    {
        protected MessageHandler(ITgClient tgClient, 
                                 ILogger logger, 
                                 ICommandsProvider commandsProvider, 
                                 IMessagesProvider messagesProvider)
        {
            TgClient = tgClient;
            Logger = logger;
            CommandsProvider = commandsProvider;
            MessagesProvider = messagesProvider;
        }

        protected ITgClient TgClient { get; }
        protected ILogger Logger { get; }
        protected ICommandsProvider CommandsProvider { get; }
        protected IMessagesProvider MessagesProvider { get; }

        public async Task Handle(MessageNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                var command = await TgClient.GetCommand(notification.Message);

                if (!WantHandle(notification.Message, command))
                    return;


                Logger.LogInformation($"{command} command recieved");
                await Handle(notification.Message, command, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogMethodError(e);
            }
        }

        protected abstract bool WantHandle(Message message, string command);

        protected abstract Task Handle(Message message, string command, CancellationToken cancellationToken);
    }
}