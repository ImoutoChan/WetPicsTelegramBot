using MediatR;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Notifications
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