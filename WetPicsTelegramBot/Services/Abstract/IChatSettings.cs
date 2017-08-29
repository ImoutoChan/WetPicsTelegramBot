using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Services.Abstract
{
    internal interface IChatSettings
    {
        List<RepostSetting> Settings { get; }
        
        Task Add(long chatId, string targetChatId);

        Task Remove(long chatId);

        event EventHandler ChatSettingsChanged;
    }
}