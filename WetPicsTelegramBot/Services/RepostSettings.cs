using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Model;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    internal class RepostSettingsService : IRepostSettingsService
    {
        private readonly IDbRepository _dbRepository;

        public RepostSettingsService(IDbRepository dbRepository)
        {
            _dbRepository = dbRepository;

            ReloadSettings();
        }

        public List<RepostSetting> Settings { get; private set; }

        public async Task ReloadSettingsAsync()
        {
            Settings = await _dbRepository.GetRepostSettingsAsync();

            OnRepostSettingsChanged();
        }

        private void ReloadSettings()
        {
            Settings = _dbRepository.GetRepostSettings();

            OnRepostSettingsChanged();
        }

        public event EventHandler RepostSettingsChanged;

        private void OnRepostSettingsChanged()
        {
            var handler = RepostSettingsChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public async Task Add(long sourceChatId, string targetChatId)
        {
            await _dbRepository.SetRepostSettings(sourceChatId, targetChatId);
            await ReloadSettingsAsync();
        }

        public async Task Remove(long sourceChatId)
        {
            await _dbRepository.RemoveRepostSettings(sourceChatId);
            await ReloadSettingsAsync();
        }
    }
}