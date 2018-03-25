using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Data.Entities.ImageSources;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IWetpicsService
    {
        Task Disable(long chatId);
        Task Enable(long chatId, int interval);
        Task AddImageSource(long chatId, ImageSource source, string options);
        Task<List<ImageSourceSetting>> GetImageSources(long chatId);
        Task RemoveImageSource(int imageSourceSettingId);
    }
}