using MediatR;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Models
{
    public class ReplyNotification : INotification
    {
        public ReplyNotification(Message message)
        {
            Message = message;
        }

        public Message Message { get; set; }
    }
}