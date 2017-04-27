using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace WetPicsTelegramBot.Database
{
    internal interface IDbRepository
    {
        Task AddOrUpdateVote(string userId, string chatId, int messageId, int? score = default(int?), bool? isLiked = default(bool?));

        Task AddPhoto(string fromUserId, string chatId, int messageId);

        List<ChatSetting> GetChatSettings();

        Task<List<ChatSetting>> GetChatSettingsAsync();

        Task<Vote> GetVotes(long messageId);

        Task RemoveChatSettings(string chatId);

        Task SetChatSettings(string chatId, string targetId);

        Task<Stats> GetStats(string userId);
    }
}