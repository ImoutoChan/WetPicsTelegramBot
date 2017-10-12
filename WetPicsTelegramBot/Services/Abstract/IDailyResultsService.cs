﻿using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.Services.Abstract
{
    internal interface IDailyResultsService
    {
        Task PostDailyResults(ChatId chatId);
    }
}