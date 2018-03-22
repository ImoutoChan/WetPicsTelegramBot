using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Data.Entities.ImageSources;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data
{
    public interface IImageSourceRepository
    {
        Task<List<ImageSourcesChatSetting>> GetImageSourceChatSettingsAsync();
        Task AddImageSourceChatSettingAsync(long chatId, int intervalMinutes);

        Task UpdateLastPostedTimeAsync(long chatId, 
            DateTimeOffset time = default(DateTimeOffset));

        Task AddPostedAsync(int chatSettingId, ImageSource source, int postId);

        Task<int?> GetFirstUnpostedNativeAsync(int chatSettingId,
            ImageSource imageSource,
            int[] workIds);

        Task<List<ImageSourceSetting>> GetImageSourcesForChatAsync(int chatSettingId);
        Task AddImageSourceAsync(long chatId, ImageSource source, string options);
        Task RemoveImageSourceAsync(int imageSourceSettingId);
    }
}