using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Database.Context;
using WetPicsTelegramBot.Database.Model;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Models;

namespace WetPicsTelegramBot.Database
{
    internal class DbRepository : Repository<WetPicsDbContext>, IDbRepository
    {
        private readonly ILogger<DbRepository> _logger;

        public DbRepository(ILogger<DbRepository> logger, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = logger;
        }

        public async Task SaveOrUpdateUser(int userId, string firstname, string lastname, string username)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var user = await db.ChatUsers.FirstOrDefaultAsync(x => x.UserId == userId);

                    if (user == null)
                    {
                        user = new ChatUser { UserId = userId };
                        db.ChatUsers.Add(user);
                    }
                    
                    user.FirstName = firstname;
                    user.LastName = lastname;
                    user.Username = username;
                    
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e, nameof(SaveOrUpdateUser));
                throw;
            }
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

        public async Task<List<Photo>> GetTop(int? userId = null, int count = 10)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    if (userId != null)
                    {
                        var top =
                            await db
                                .Photos
                                .FromSql("SELECT ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\", ph.\"AddedDate\", ph.\"ModifiedDate\"\r\n" +
                                         "FROM \"Photos\" ph\r\nINNER JOIN \"PhotoVotes\" phv ON ph.\"MessageId\" = phv.\"MessageId\" AND ph.\"ChatId\" = phv.\"ChatId\"\r\n" +
                                         $"WHERE ph.\"FromUserId\" = {userId}\r\n" +
                                         "GROUP BY ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\", ph.\"AddedDate\", ph.\"ModifiedDate\"\r\n" +
                                         "ORDER BY count(*) DESC, ph.\"Id\" DESC\r\n" +
                                         $"LIMIT {count}").ToListAsync();

                        return top;

                    }
                    else
                    {
                        var top =
                            await db
                                .Photos
                                .FromSql("SELECT ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\", ph.\"AddedDate\", ph.\"ModifiedDate\"\r\n" +
                                         "FROM \"Photos\" ph\r\nINNER JOIN \"PhotoVotes\" phv ON ph.\"MessageId\" = phv.\"MessageId\" AND ph.\"ChatId\" = phv.\"ChatId\"\r\n" +
                                         "GROUP BY ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\", ph.\"AddedDate\", ph.\"ModifiedDate\"\r\n" +
                                         "ORDER BY count(*) DESC, ph.\"Id\" DESC\r\n" +
                                         $"LIMIT {count}").ToListAsync();

                        return top;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetTop)} method (userId: {userId})");
                throw;
            }
        }

        public async Task<List<TopEntry>> GetTopImagesSlow(int? userId = null, 
                                                           int count = 10, 
                                                           DateTimeOffset from = default(DateTimeOffset), 
                                                           DateTimeOffset to = default(DateTimeOffset))
        {
            var allowDateNull = false;
            if (from == default(DateTimeOffset))
            {
                from = DateTimeOffset.MinValue;
            }
            if (to == default(DateTimeOffset))
            {
                to = DateTimeOffset.MaxValue;
            }
            if (from == default(DateTimeOffset) && to == default(DateTimeOffset))
            {
                allowDateNull = true;
            }
            
            try
            {
                using (var db = GetDbContext())
                {
                    var photos = db
                        .Photos
                        .Where(x => allowDateNull || x.AddedDate != null)
                        .Where(x => x.AddedDate == null || x.AddedDate >= from && x.AddedDate <= to);

                    if (userId != null)
                    {
                        photos = photos
                            .Where(x => x.FromUserId == userId);
                    }

                    var result = await photos
                        .Join(db.PhotoVotes,
                              photo => new {photo.MessageId, photo.ChatId},
                              vote => new {vote.MessageId, vote.ChatId},
                              (photo, vote) => new {photo, vote})
                        .Join(db.ChatUsers,
                              photoVote => photoVote.photo.FromUserId,
                              user => user.UserId,
                              (photoVote, user) => new {photoVote.photo, photoVote.vote, user})
                        .GroupBy(x => new { x.photo, x.user })
                        .OrderByDescending(x => x.Count())
                        .ThenByDescending(x => x.Key.photo.Id)
                        .Take(count)
                        .Select(x => new TopEntry {Photo = x.Key.photo, Likes = x.Count(), User = x.Key.user})
                        .ToListAsync();

                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetTopImagesSlow)} method (userId: {userId})");
                throw;
            }
        }


        public async Task<List<TopUsersEntry>> GetTopUsersSlow(int count = 10,
                                                               DateTimeOffset from = default(DateTimeOffset),
                                                               DateTimeOffset to = default(DateTimeOffset))
        {
            var allowDateNull = false;
            if (from == default(DateTimeOffset))
            {
                from = DateTimeOffset.MinValue;
            }
            if (to == default(DateTimeOffset))
            {
                to = DateTimeOffset.MaxValue;
            }
            if (from == default(DateTimeOffset) && to == default(DateTimeOffset))
            {
                allowDateNull = true;
            }

            try
            {
                // TODO optimize 
                using (var db = GetDbContext())
                {
                    var photos = db
                        .Photos
                        .Where(x => allowDateNull || x.AddedDate != null)
                        .Where(x => x.AddedDate == null || x.AddedDate >= from && x.AddedDate <= to);

                    var photoLikes = await photos
                                     .Join(db.PhotoVotes,
                                           photo => new { photo.MessageId, photo.ChatId },
                                           vote => new { vote.MessageId, vote.ChatId },
                                           (photo, vote) => new { photo, vote })
                                     .Join(db.ChatUsers,
                                           photoVote => photoVote.photo.FromUserId,
                                           user => user.UserId,
                                           (photoVote, user) => new { photoVote.photo, photoVote.vote, user })
                                     .GroupBy(x => x.user)
                                     .Take(count)
                                     .Select(x => new { Likes = x.Count(), User = x.Key })
                                     .ToListAsync();

                    var photoCount = await photos
                                             .Join(db.ChatUsers,
                                                   photo => photo.FromUserId,
                                                   user => user.UserId,
                                                   (photo, user) => new {photo, user})
                                             .GroupBy(x => x.user)
                                             .Select(x => new {User = x.Key, Photos = x.Count()})
                                             .ToListAsync();

                    var result = photoLikes.Join(photoCount,
                                    likes => likes.User.UserId,
                                    phCount => phCount.User.UserId,
                                    (likes, phCount) => new TopUsersEntry
                                    {
                                        User = likes.User,
                                        Likes = likes.Likes,
                                        Photos = phCount.Photos
                                    });

                    return result.ToList();
                }
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task<GlobalStats> GetGlobalStats(DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var picCount = await db
                        .Photos
                        .FilterByDates(from, to)
                        .CountAsync();

                    var likesCount = await db
                        .PhotoVotes
                        .FilterByDates(from, to)
                        .CountAsync();

                    var picAnyLiked = await db.Photos
                        .FilterByDates(from, to)
                        .Join(db.PhotoVotes,
                                photo => new { photo.MessageId, photo.ChatId },
                                vote => new { vote.MessageId, vote.ChatId },
                                (photo, vote) => new { PId = photo.Id, VId = vote.Id })
                        .GroupBy(x => x.PId)
                        .CountAsync();


                    return new GlobalStats(picCount, likesCount, picAnyLiked);
                }
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e, nameof(GetGlobalStats));
                throw;
            }
        }
    }
}
