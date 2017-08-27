using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace WetPicsTelegramBot.Services
{
    internal interface IBaseDialogService
    {
        IObservable<Command> MessageObservable { get; }

        Task Reply(Message message,
            string text,
            ParseMode parseMode = ParseMode.Default,
            IReplyMarkup replyMarkup = null);
    }
}