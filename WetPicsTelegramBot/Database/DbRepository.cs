using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Database.Context;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Database
{
    internal class DbRepository : Repository<WetPicsDbContext>, IDbRepository
    {
        private readonly ILogger<DbRepository> _logger;

        public DbRepository(ILogger<DbRepository> logger, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = logger;
        }

        public async Task AddPhoto(int fromUserId, long chatId, int messageId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var newPhoto = new Photo
                    {
                        ChatId = chatId,
                        FromUserId = fromUserId,
                        MessageId = messageId
                    };

                    await db.Photos.AddAsync(newPhoto);

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(AddPhoto)} method");
                throw;
            }
        }

        public async Task<bool> AddOrUpdateVote(int userId, long chatId, int messageId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var photoVote = await db.PhotoVotes
                        .FirstOrDefaultAsync(x => x.ChatId == chatId 
                                                    && x.MessageId == messageId
                                                    && x.UserId == userId);

                    if (photoVote != null)
                    {
                        return false;
                    }

                    photoVote = new PhotoVote
                    {
                        ChatId = chatId,
                        MessageId = messageId,
                        UserId = userId
                    };

                    await db.PhotoVotes.AddAsync(photoVote);

                    await db.SaveChangesAsync();

                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(AddOrUpdateVote)} method");
                throw;
            }
        }

        public async Task<int> GetVotes(int messageId, long chatId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var result = await db.PhotoVotes.Where(x => x.MessageId == messageId && x.ChatId == chatId).CountAsync();
                    
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetVotes)} method");
                throw;
            }
        }

        public async Task RemoveRepostSettings(long chatId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var chatSettings = await db.RepostSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (chatSettings != null)
                    {
                        db.RepostSettings.Remove(chatSettings);

                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(RemoveRepostSettings)} method");
                throw;
            }
        }

        public async Task SetRepostSettings(long chatId, string targetId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var chatSettings = await db.RepostSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (chatSettings == null)
                    {
                        var newChatSettings = new RepostSetting
                        {
                            ChatId = chatId,
                            TargetId = targetId
                        };

                        await db.RepostSettings.AddAsync(newChatSettings);
                    }
                    else
                    {
                        chatSettings.TargetId = targetId;
                    }

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(SetRepostSettings)} method");
                throw;
            }
        }

        public async Task<List<RepostSetting>> GetRepostSettingsAsync()
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var chatSettings = await db.RepostSettings.ToListAsync();
                    
                    return chatSettings;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetRepostSettingsAsync)} method");
                throw;
            }
        }

        public List<RepostSetting> GetRepostSettings()
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var chatSettings = db.RepostSettings.ToList();

                    return chatSettings;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetRepostSettings)} method");
                throw;
            }
        }

        public async Task<Stats> GetStats(int userId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var picCount = await db.Photos.CountAsync(x => x.FromUserId == userId);

                    var getLikeCount = await
                        db.Photos
                        .Where(x => x.FromUserId == userId)
                        .Join(db.PhotoVotes,
                                    photo => new { photo.MessageId, photo.ChatId },
                                    vote => new { vote.MessageId, vote.ChatId },  
                                    (photo, vote) => new { photo, vote })
                        .CountAsync();

                    var setLikeCount = await db.PhotoVotes.CountAsync(x => x.UserId == userId);

                    var setSelfLikeCount = await 
                        db.Photos
                        .Where(x => x.FromUserId == userId)
                        .Join(db.PhotoVotes,
                                    photo => new { photo.MessageId, photo.ChatId },
                                    vote => new { vote.MessageId, vote.ChatId },
                                    (photo, vote) => new { photo, vote })
                        .CountAsync(x => x.vote.UserId == userId);


                    return new Stats(picCount, getLikeCount, setLikeCount, setSelfLikeCount);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetStats)} method (userId: {userId})");
                throw;
            }
        }

        public async Task<List<Photo>> GetTop(int userId, int count = 10)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var top = await db.Photos.FromSql("SELECT ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\"\r\n" +
                                                        "FROM \"Photos\" ph\r\nINNER JOIN \"PhotoVotes\" phv ON ph.\"MessageId\" = phv.\"MessageId\" AND ph.\"ChatId\" = phv.\"ChatId\"\r\n" +
                                                        $"WHERE ph.\"FromUserId\" = {userId}\r\n" +
                                                        "GROUP BY ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\"\r\n" +
                                                        "ORDER BY count(*) DESC, ph.\"Id\" DESC\r\n" +
                                                        $"LIMIT {count}").ToListAsync();

                    return top;

                    // TODO BAD PERFOMANCE - GROUPBY
                    //var userPhotos = db
                    //    .Photos
                    //    .Where(x => x.FromUserId == userId)
                    //    .Join(db.PhotoVotes,
                    //          photo => new {photo.MessageId, photo.ChatId},
                    //          vote => new {vote.MessageId, vote.ChatId},
                    //          (photo, vote) => new {photo, vote})
                    //    .GroupBy(x => x.photo)
                    //    .OrderByDescending(x => x.Count())
                    //    .Take(10);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetTop)} method (userId: {userId})");
                throw;
            }
        }
    }
}
