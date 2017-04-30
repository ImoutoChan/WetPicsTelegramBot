using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WetPicsTelegramBot
{
    internal interface IChatSettings
    {
        List<ChatSetting> Settings { get; }

        Task ReloadSettingsAsync();

        Task Add(string chatId, string targetChatId);

        Task Remove(string chatId);

        event EventHandler ChatSettingsChanged;
    }
}