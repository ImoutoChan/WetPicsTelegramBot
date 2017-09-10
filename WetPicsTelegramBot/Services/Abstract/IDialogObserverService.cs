﻿using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Models;

namespace WetPicsTelegramBot.Services.Abstract
{
    internal interface IDialogObserverService
    {
        IObservable<Command> MessageObservable { get; }

        IObservable<Message> RepliesObservable { get; }

        Task<Message> Reply(Message message, string text, ParseMode parseMode = ParseMode.Default, IReplyMarkup replyMarkup = null);
    }
}