using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Model;
using WetPicsTelegramBot.Services.Abstract;

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

        public List<RepostSetting> Settings { get; private set; }

        public async Task ReloadSettingsAsync()
        {
            Settings = await _dbRepository.GetRepostSettingsAsync();

            OnChatSettingsChanged();
        }

        private void ReloadSettings()
        {
            Settings = _dbRepository.GetRepostSettings();

            OnChatSettingsChanged();
        }

        public event EventHandler ChatSettingsChanged;

        private void OnChatSettingsChanged()
        {
            var handler = ChatSettingsChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public async Task Add(long chatId, string targetChatId)
        {
            await _dbRepository.SetRepostSettings(chatId, targetChatId);
            await ReloadSettingsAsync();
        }

        public async Task Remove(long chatId)
        {
            await _dbRepository.RemoveRepostSettings(chatId);
            await ReloadSettingsAsync();
        }
    }
}