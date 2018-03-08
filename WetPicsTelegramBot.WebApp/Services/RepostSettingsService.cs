using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class RepostSettingsService : IRepostSettingsService
    {
        private readonly IDbRepository _dbRepository;

        public RepostSettingsService(IDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }
        
        public async Task Add(long sourceChatId, string targetChatId)
        {
            await _dbRepository.SetRepostSettings(sourceChatId, targetChatId);
        }

        public async Task Remove(long sourceChatId)
        {
            await _dbRepository.RemoveRepostSettings(sourceChatId);
        }

        public async Task<List<RepostSetting>> GetSettings()
            => await _dbRepository.GetRepostSettingsAsync();
    }
}