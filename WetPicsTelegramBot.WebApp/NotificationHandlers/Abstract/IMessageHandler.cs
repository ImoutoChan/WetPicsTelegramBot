﻿using MediatR;
using WetPicsTelegramBot.WebApp.Notifications;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract
{
    public interface IMessageHandler : INotificationHandler<MessageNotification>
    {
    }
}