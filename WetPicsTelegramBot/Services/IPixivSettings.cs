using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Services.Abstract
{
    internal interface IPixivSettings
    {
        List<PixivSetting> Settings { get; }
        
        Task Add(long chatId, PixivTopType type = PixivTopType.DailyR18, int intervalMinutes = 15);

        Task Remove(long chatId);

        event EventHandler PixivSettingsChanged;

        Task UpdateLastPostedTime(PixivSetting pixivSetting);

        Task AddPosted(PixivSetting pixivSetting, int workId);
    }
}