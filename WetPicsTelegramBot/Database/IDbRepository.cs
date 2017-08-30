using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Database
{
    internal interface IDbRepository
    {
        Task<bool> AddOrUpdateVote(int userId, long chatId, int messageId);

        Task AddPhoto(int fromUserId, long chatId, int messageId);

        List<RepostSetting> GetRepostSettings();

        Task<List<RepostSetting>> GetRepostSettingsAsync();

        Task<int> GetVotes(int messageId, long chatId);

        Task RemoveRepostSettings(long chatId);

        Task SetRepostSettings(long chatId, string targetId);

        Task<Stats> GetStats(int userId);

        Task<List<Photo>> GetTop(int userId, int count = 10);
    }
}