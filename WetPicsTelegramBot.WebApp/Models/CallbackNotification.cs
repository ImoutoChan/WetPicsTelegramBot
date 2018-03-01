using MediatR;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Models
{
    public class CallbackNotification : INotification
    {
        public CallbackNotification(CallbackQuery callbackQuery)
        {
            CallbackQuery = callbackQuery;
        }

        public CallbackQuery CallbackQuery { get; set; }
    }
}