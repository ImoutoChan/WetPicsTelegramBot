using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data
{
    public interface IPixivRepository
    {
        Task AddPosted(int pixivSettingId, int workId);
        Task<List<PixivSetting>> GetPixivSettingsAsync();
        Task RemovePixivSettings(long chatId);
        Task SetPixivSettings(long chatId, PixivTopType type, int intervalMinutes);
        Task UpdateLastPostedTime(long chatId, DateTimeOffset time = default(DateTimeOffset));
        Task<int?> GetFirstUnpostedNativeAsync(int pixivSettingId, int[] workIds);
    }
}