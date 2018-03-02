using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Notifications;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract
{
    public abstract class MessageHandler : IMessageHandler
    {
        private readonly ITgClient _tgClient;
        private readonly ILogger _logger;

        protected MessageHandler(ITgClient tgClient, ILogger logger)
        {
            _tgClient = tgClient;
            _logger = logger;
        }

        public async Task Handle(MessageNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                var command = await GetCommand(notification.Message);

                if (!WantHandle(notification.Message, command))
                    return;


                _logger.LogInformation($"{command} command recieved");
                await Handle(notification.Message, command, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
            }
        }

        protected abstract bool WantHandle(Message message, string command);

        protected abstract Task Handle(Message message, string command, CancellationToken cancellationToken);

        protected async Task<string> GetCommand(Message message)
        {
            var me = await _tgClient.GetMe();
            var botUsername = me.Username;

            var text = message?.Text;

            if (String.IsNullOrWhiteSpace(text) || !text.StartsWith("/"))
            {
                return null;
            }

            var firstWord = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
            var isCommandWithId = firstWord.Contains("@") && firstWord.EndsWith(botUsername);
            var command = isCommandWithId ? firstWord.Split('@').First() : firstWord;

            return command;
        }
    }
}