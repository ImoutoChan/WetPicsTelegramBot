using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Services
{
    internal class ChatSettings : IChatSettings
    {
        private readonly IDbRepository _dbRepository;

        public ChatSettings(IDbRepository dbRepository)
        {
            _dbRepository = dbRepository;

            ReloadSettings();
        }

        public List<ChatSetting> Settings { get; private set; }

        public async Task ReloadSettingsAsync()
        {
            Settings = await _dbRepository.GetChatSettingsAsync();

            OnChatSettingsChanged();
        }

        private void ReloadSettings()
        {
            Settings = _dbRepository.GetChatSettings();

            OnChatSettingsChanged();
        }

        public event EventHandler ChatSettingsChanged;

        private void OnChatSettingsChanged()
        {
            var handler = ChatSettingsChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public async Task Add(string chatId, string targetChatId)
        {
            await _dbRepository.SetChatSettings(chatId, targetChatId);
            await ReloadSettingsAsync();
        }

        public async Task Remove(string chatId)
        {
            await _dbRepository.RemoveChatSettings(chatId);
            await ReloadSettingsAsync();
        }
    }
}