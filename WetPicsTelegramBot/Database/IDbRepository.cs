using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Database
{
    internal interface IDbRepository
    {
        Task<bool> AddOrUpdateVote(int userId, long chatId, int messageId, bool? isLiked = default(bool?));

        Task AddPhoto(int fromUserId, long chatId, int messageId);

        List<ChatSetting> GetChatSettings();

        Task<List<ChatSetting>> GetChatSettingsAsync();

        Task<Vote> GetVotes(int messageId, long chatId);

        Task RemoveChatSettings(long chatId);

        Task SetChatSettings(long chatId, string targetId);

        Task<Stats> GetStats(int userId);
    }
}