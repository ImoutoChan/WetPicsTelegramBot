using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data
{
    public interface IDbRepository
    {
        Task<bool> AddOrUpdateVote(int userId, long chatId, int messageId);

        Task AddPhoto(int fromUserId, long chatId, int messageId);

        List<RepostSetting> GetRepostSettings();

        Task<List<RepostSetting>> GetRepostSettingsAsync();

        Task<int> GetVotes(int messageId, long chatId);

        Task RemoveRepostSettings(long chatId);

        Task SetRepostSettings(long chatId, string targetId);

        Task<Stats> GetStats(int userId, long? chatId = null);

        [Obsolete]
        Task<List<Photo>> GetTop(int? userId = null, int count = 10);

        Task<List<TopEntry>> GetTopImagesSlow(int? userId = null, 
                                              int count = 10, 
                                              DateTimeOffset from = default(DateTimeOffset), 
                                              DateTimeOffset to = default(DateTimeOffset),
                                              long? sourceChat = null);

        Task<GlobalStats> GetGlobalStats(DateTimeOffset? from = null, DateTimeOffset? to = null);

        Task SaveOrUpdateUser(int userId, string firstname, string lastname, string username);

        Task<List<TopUsersEntry>> GetTopUsersSlow(int count = 10,
                                                  DateTimeOffset from = default(DateTimeOffset),
                                                  DateTimeOffset to = default(DateTimeOffset),
                                                  long? sourceChatId = null);
    }
}