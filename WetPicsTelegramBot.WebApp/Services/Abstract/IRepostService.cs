﻿using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IRepostService
    {
        Task RepostWithLikes(Message message, string targetId, string caption);

        Task<string> TryGetRepostTargetChat(long forChatId);
    }
}
