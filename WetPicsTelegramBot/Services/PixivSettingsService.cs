using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Model;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    internal class PixivSettingsService : IPixivSettingsService
    {
        private readonly IPixivRepository _pixivRepository;

        public PixivSettingsService(IPixivRepository pixivRepository)
        {
            _pixivRepository = pixivRepository;

            ReloadSettings();
        }

        public List<PixivSetting> Settings { get; private set; }

        private async Task ReloadSettingsAsync()
        {
            Settings = await _pixivRepository.GetPixivSettingsAsync();

            Compress();

            OnPixivSettingsChanged();
        }

        private void ReloadSettings()
        {
            Settings = _pixivRepository.GetPixivSettings();

            Compress();

            OnPixivSettingsChanged();
        }

        private void Compress()
        {
            foreach (var pixivSetting in Settings)
            {
                pixivSetting.PixivImagePostsSet =
                    new HashSet<int>(pixivSetting.PixivImagePosts.Select(x => x.PixivIllustrationId));

                pixivSetting.PixivImagePosts = null;
            }
        }

        public event EventHandler PixivSettingsChanged;

        private void OnPixivSettingsChanged()
        {
            var handler = PixivSettingsChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public async Task Add(long chatId, PixivTopType type, int intervalMinutes)
        {
            await _pixivRepository.SetPixivSettings(chatId, type, intervalMinutes);
            await ReloadSettingsAsync();
        }

        public async Task Remove(long chatId)
        {
            await _pixivRepository.UpdateLastPostedTime(chatId, DateTimeOffset.MaxValue);
            await ReloadSettingsAsync();
        }

        public async Task UpdateLastPostedTime(PixivSetting pixivSetting)
        {
            await _pixivRepository.UpdateLastPostedTime(pixivSetting.ChatId);
            pixivSetting.LastPostedTime = DateTimeOffset.Now;
        }

        public async Task AddPosted(PixivSetting pixivSetting, int workId)
        {
            await _pixivRepository.AddPosted(pixivSetting, workId);
            pixivSetting.PixivImagePostsSet.Add(workId);
        }
    }
}