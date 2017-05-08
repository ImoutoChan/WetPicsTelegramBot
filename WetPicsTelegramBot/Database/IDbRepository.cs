using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Database
{
    internal interface IDbRepository
    {
        Task<bool> AddOrUpdateVote(string userId, string chatId, int messageId, bool? isLiked = default(bool?));

        Task AddPhoto(string fromUserId, string chatId, int messageId);

        List<ChatSetting> GetChatSettings();

        Task<List<ChatSetting>> GetChatSettingsAsync();

        Task<Vote> GetVotes(long messageId, string chatId);

        Task RemoveChatSettings(string chatId);

        Task SetChatSettings(string chatId, string targetId);

        Task<Stats> GetStats(string userId);
    }
}