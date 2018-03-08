using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Notifications;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class NotificationService : INotificationService, INotification
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IMediator _mediator;

        public NotificationService(ILogger<NotificationService> logger, 
                                   IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task NotifyAsync(Update update)
        {
            _logger.LogTrace($"Notification about {update.Type}");

            try
            {
                var notification = GetNotification(update);

                if (notification == null)
                {
                    return;
                }

                await _mediator.Publish(notification);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
            }
        }

        private INotification GetNotification(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.CallbackQuery:
                    return new CallbackNotification(update.CallbackQuery);
                case UpdateType.Message:
                    return new MessageNotification(update.Message);
                default:
                    return null;
            }
        }
    }
}