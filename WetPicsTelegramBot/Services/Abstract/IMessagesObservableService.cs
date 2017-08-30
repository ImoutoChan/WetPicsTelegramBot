using System;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.Services.Abstract
{
    interface IMessagesObservableService
    {
        IObservable<Message> BaseObservable { get; set; }

        IObservable<CallbackQuery> BaseCallbackObservable { get; set; }
    }
}