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
    public abstract class CallbackHandler : ICallbackHandler
    {
        protected CallbackHandler(ITgClient tgClient, 
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

        public async Task Handle(CallbackNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                if (!WantHandle(notification.CallbackQuery))
                    return;


                Logger.LogInformation($"{notification.CallbackQuery.Data} callback recieved");
                await Handle(notification.CallbackQuery, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogMethodError(e);
            }
        }

        protected abstract bool WantHandle(CallbackQuery query);

        protected abstract Task Handle(CallbackQuery query, CancellationToken cancellationToken);
    }
}