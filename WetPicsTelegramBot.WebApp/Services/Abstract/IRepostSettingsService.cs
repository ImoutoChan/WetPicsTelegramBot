using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Data.Entities;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IRepostSettingsService
    {
        Task Add(long sourceChatId, string targetChatId);
        Task Remove(long sourceChatId);
        Task<List<RepostSetting>> GetSettings();
    }
}