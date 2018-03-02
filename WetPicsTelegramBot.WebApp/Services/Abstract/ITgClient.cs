﻿using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface ITgClient
    {
        Task<User> GetMe();

        ITelegramBotClient Client { get; }
    }
}