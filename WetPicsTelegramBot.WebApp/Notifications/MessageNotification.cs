using MediatR;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Notifications
{
    public class MessageNotification : INotification
    {
        public MessageNotification(Message message)
        {
            Message = message;
        }

        public Message Message { get; }
    }
}