using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class NotificationService : INotificationService, INotification
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IMediator _mediator;
        private readonly ITelegramClient _client;

        public NotificationService(ILogger<NotificationService> logger, 
                                   IMediator mediator,
                                   ITelegramClient client)
        {
            _logger = logger;
            _mediator = mediator;
            _client = client;
        }

        public async Task NotifyAsync(Update update)
        {
            try
            {
                var notif = await CreateNotification(update);
                if (notif == null)
                {
                    return;
                }

                await _mediator.Publish(notif);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
            }
        }

        private async Task<INotification> CreateNotification(Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (await IsReplyToMe(update))
                    {
                        return new ReplyNotification(update.Message);
                    }
                    return new MessageNotification(update.Message);
                case UpdateType.CallbackQuery:
                    return new CallbackNotification(update.CallbackQuery);
                default:
                    return null;
            }
        }

        private async Task<bool> IsReplyToMe(Update update)
        {
            return update.Message.ReplyToMessage?.From != null
                    && update.Message.ReplyToMessage?.From.Id == (await _client.GetMe()).Id;
        }
    }

    public interface ITelegramClient
    {
        Task<User> GetMe();
    }
}