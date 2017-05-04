using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Database
{
    interface IPixivRepository
    {
        List<PixivSetting> GetPixivSettings();
        Task<List<PixivSetting>> GetPixivSettingsAsync();
        Task RemovePixivSettings(long chatId);
        Task SetPixivSettings(long chatId, PixivTopType type, int intervalMinutes);
        Task UpdateLastPostedTime(long chatId, DateTimeOffset time = default(DateTimeOffset));
        Task AddPosted(PixivSetting pixivSetting, int workId);
    }
}