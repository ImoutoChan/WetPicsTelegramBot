﻿using System.Threading.Tasks;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Models;

namespace WetPicsTelegramBot.Services.Abstract
{
    interface ITopRatingService
    {
        Task PostTop(ChatId chatId,
                     int? messageId, 
                     TopSource topSource = TopSource.Reply, 
                     int count = 5, 
                     TopPeriod period = TopPeriod.AllTime, 
                     User user = null);

        Task PostUsersTop(long chatId, int messageMessageId, int count, TopPeriod period);
    }
}