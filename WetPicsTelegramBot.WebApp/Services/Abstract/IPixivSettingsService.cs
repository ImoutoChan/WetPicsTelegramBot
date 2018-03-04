using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IPixivSettingsService
    {
        Task Add(long chatId, PixivTopType type, int intervalMinutes);
        Task AddPosted(PixivSetting pixivSetting, int workId);
        Task<int?> GetFirstUnpostedAsync(int pixivSettingsId, int[] newWorkIds);
        Task<List<PixivSetting>> GetSettingsAsync();
        Task Remove(long chatId);
        Task UpdateLastPostedTime(PixivSetting pixivSetting);
    }
}