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
        Task UpdateLastPostedTime(long chatId);
        Task AddPosted(PixivSetting pixivSetting, int workId);
    }
}