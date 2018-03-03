using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class PixivSettingsService : IPixivSettingsService
    {
        private readonly IPixivRepository _pixivRepository;

        public PixivSettingsService(IPixivRepository pixivRepository)
        {
            _pixivRepository = pixivRepository;
        }
        
        public async Task<List<PixivSetting>> GetSettingsAsync()
        {
            return await _pixivRepository.GetPixivSettingsAsync();
        }

        public async Task<long?> GetFirstUnpostedAsync(int pixivSettingsId, long[] newWorkIds)
        {
            return await _pixivRepository.GetFirstUnpostedNativeAsync(pixivSettingsId, newWorkIds);
        }

        public async Task Add(long chatId, PixivTopType type, int intervalMinutes)
        {
            await _pixivRepository.SetPixivSettings(chatId, type, intervalMinutes);
        }

        public async Task Remove(long chatId)
        {
            await _pixivRepository.UpdateLastPostedTime(chatId, DateTimeOffset.MaxValue);
        }

        public async Task UpdateLastPostedTime(PixivSetting pixivSetting)
        {
            await _pixivRepository.UpdateLastPostedTime(pixivSetting.ChatId);
            pixivSetting.LastPostedTime = DateTimeOffset.Now;
        }

        public async Task AddPosted(PixivSetting pixivSetting, int workId)
        {
            await _pixivRepository.AddPosted(pixivSetting.Id, workId);
        }
    }
}