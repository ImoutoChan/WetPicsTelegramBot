﻿using System.Threading.Tasks;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IPostingService
    {
        Task<bool> PostNext(long chatId, string sourceOptions);
    }
}