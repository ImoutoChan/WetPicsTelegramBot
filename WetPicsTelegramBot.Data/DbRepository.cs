using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Data.Context;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.Data.Helpers;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data
{
    public class DbRepository : IDbRepository
    {
        private readonly ILogger<DbRepository> _logger;
        private readonly WetPicsDbContext _context;

        public DbRepository(ILogger<DbRepository> logger, 
                            WetPicsDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task SaveOrUpdateUser(int userId, string firstname, string lastname, string username)
        {
            try
            {
                var user = await _context.ChatUsers.FirstOrDefaultAsync(x => x.UserId == userId);

                if (user == null)
                {
                    user = new ChatUser { UserId = userId };
                    _context.ChatUsers.Add(user);
                }
                    
                user.FirstName = firstname;
                user.LastName = lastname;
                user.Username = username;
                    
                await _context.SaveChangesAsync();
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
                var newPhoto = new Photo
                {
                    ChatId = chatId,
                    FromUserId = fromUserId,
                    MessageId = messageId
                };

                await _context.Photos.AddAsync(newPhoto);

                await _context.SaveChangesAsync();
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
                var photoVote = await _context.PhotoVotes
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

                await _context.PhotoVotes.AddAsync(photoVote);

                await _context.SaveChangesAsync();

                return true;
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
                var result = await _context.PhotoVotes.Where(x => x.MessageId == messageId && x.ChatId == chatId).CountAsync();
                    
                return result;
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
                    var chatSettings = await _context.RepostSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (chatSettings != null)
                    {
                        _context.RepostSettings.Remove(chatSettings);

                        await _context.SaveChangesAsync();
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
                var chatSettings = await _context.RepostSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                if (chatSettings == null)
                {
                    var newChatSettings = new RepostSetting
                    {
                        ChatId = chatId,
                        TargetId = targetId
                    };

                    await _context.RepostSettings.AddAsync(newChatSettings);
                }
                else
                {
                    chatSettings.TargetId = targetId;
                }

                await _context.SaveChangesAsync();
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
                    var chatSettings = await _context.RepostSettings.ToListAsync();
                    
                    return chatSettings;
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
                var chatSettings = _context.RepostSettings.ToList();

                return chatSettings;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetRepostSettings)} method");
                throw;
            }
        }

        public async Task<Stats> GetStats(int userId, long? chatId = null)
        {
            try
            {
                var photos = _context.Photos.AsQueryable();

                if (chatId != null)
                    photos = FilterPhotosBySourceChat(chatId, photos);

                var picCount = await photos.CountAsync(x => x.FromUserId == userId);

                var getLikeCount 
                    = await photos
                        .Where(x => x.FromUserId == userId)
                        .Join(_context.PhotoVotes,
                              photo => new { photo.MessageId, photo.ChatId },
                              vote => new { vote.MessageId, vote.ChatId },  
                              (photo, vote) => new { photo, vote })
                        .CountAsync();

                var setLikeCount 
                    = await photos
                        .Join(_context.PhotoVotes,
                              photo => new { photo.MessageId, photo.ChatId },
                              vote => new { vote.MessageId, vote.ChatId },
                              (photo, vote) => new { photo, vote })
                        .CountAsync(x => x.vote.UserId == userId);

                var setSelfLikeCount 
                    = await photos
                        .Where(x => x.FromUserId == userId)
                        .Join(_context.PhotoVotes,
                              photo => new { photo.MessageId, photo.ChatId },
                              vote => new { vote.MessageId, vote.ChatId },
                              (photo, vote) => new { photo, vote })
                        .CountAsync(x => x.vote.UserId == userId);
                
                return new Stats(picCount, getLikeCount, setLikeCount, setSelfLikeCount);
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
                if (userId != null)
                {
                    var top =
                        await _context
                            .Photos
                            .FromSqlRaw("SELECT ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\", ph.\"AddedDate\", ph.\"ModifiedDate\"\r\n" +
                                        "FROM \"Photos\" ph\r\nINNER JOIN \"PhotoVotes\" phv ON ph.\"MessageId\" = phv.\"MessageId\" AND ph.\"ChatId\" = phv.\"ChatId\"\r\n" +
                                        $"WHERE ph.\"FromUserId\" = {userId}\r\n" +
                                        "GROUP BY ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\", ph.\"AddedDate\", ph.\"ModifiedDate\"\r\n" +
                                        "ORDER BY count(*) DESC, ph.\"Id\" DESC\r\n" +
                                        $"LIMIT {count}")
                            .ToListAsync();

                    return top;

                }
                else
                {
                    var top =
                        await _context
                            .Photos
                            .FromSqlRaw("SELECT ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\", ph.\"AddedDate\", ph.\"ModifiedDate\"\r\n" +
                                        "FROM \"Photos\" ph\r\nINNER JOIN \"PhotoVotes\" phv ON ph.\"MessageId\" = phv.\"MessageId\" AND ph.\"ChatId\" = phv.\"ChatId\"\r\n" +
                                        "GROUP BY ph.\"Id\", ph.\"MessageId\", ph.\"FromUserId\", ph.\"ChatId\", ph.\"AddedDate\", ph.\"ModifiedDate\"\r\n" +
                                        "ORDER BY count(*) DESC, ph.\"Id\" DESC\r\n" +
                                        $"LIMIT {count}")
                            .ToListAsync();

                    return top;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetTop)} method (userId: {userId})");
                throw;
            }
        }

        public async Task<List<TopEntry>> GetTopImagesSlow(
            int? userId = null, 
            int count = 10, 
            DateTimeOffset from = default, 
            DateTimeOffset to = default,
            long? sourceChat = null)
        {
            var allowDateNull = from == default 
                                && to == default;

            if (from == default)
                from = DateTimeOffset.MinValue;
            if (to == default)
                to = DateTimeOffset.MaxValue;

            try
            {
                var photos = _context
                               .Photos
                               .Where(x => allowDateNull || x.AddedDate != null)
                               .Where(x => x.AddedDate == null || x.AddedDate >= from 
                                          && x.AddedDate <= to);

                if (userId.HasValue)
                    photos = photos.Where(x => x.FromUserId == userId);

                if (sourceChat.HasValue)
                    photos = FilterPhotosBySourceChat(sourceChat, photos);

                var result = await photos
                    .Join(
                        _context.PhotoVotes,
                        photo => new {photo.MessageId, photo.ChatId},
                        vote => new {vote.MessageId, vote.ChatId},
                        (photo, vote) => new {photo, vote})
                    .Join(
                        _context.ChatUsers,
                        photoVote => photoVote.photo.FromUserId,
                        user => user.UserId,
                        (photoVote, user) => new {photoVote.photo, photoVote.vote, user})
                    .ToListAsync();

                return result
                    .GroupBy(x => new { x.photo, x.user })
                    .OrderByDescending(x => x.Count())
                    .ThenByDescending(x => x.Key.photo.Id)
                    .Take(count)
                    .Select(x => new TopEntry
                    {
                        Photo = x.Key.photo,
                        Likes = x.Count(),
                        User = x.Key.user
                    })
                    .ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetTopImagesSlow)} method (userId: {userId})");
                throw;
            }
        }


        public async Task<List<TopUsersEntry>> GetTopUsersSlow(int count = 10,
                                                               DateTimeOffset from = default,
                                                               DateTimeOffset to = default,
                                                               long? sourceChatId = default)
        {
            var allowDateNull = from == default && to == default;

            if (from == default)
                from = DateTimeOffset.MinValue;
            if (to == default)
                to = DateTimeOffset.MaxValue;

            try
            {
                // TODO optimize 
                var photos = _context
                    .Photos
                    .Where(x => allowDateNull || x.AddedDate != null)
                    .Where(x => x.AddedDate == null || x.AddedDate >= from && x.AddedDate <= to);


                if (sourceChatId.HasValue)
                    photos = FilterPhotosBySourceChat(sourceChatId, photos);

                var photoLikesRaw 
                    = await photos
                        .Join(_context.PhotoVotes,
                            photo => new { photo.MessageId, photo.ChatId },
                            vote => new { vote.MessageId, vote.ChatId },
                            (photo, vote) => new { photo, vote })
                        .Join(_context.ChatUsers,
                            photoVote => photoVote.photo.FromUserId,
                            user => user.UserId,
                            (photoVote, user) => new { photoVote.photo, photoVote.vote, user })
                        .ToListAsync();
                var photoLikes = photoLikesRaw
                        .GroupBy(x => x.user)
                        .OrderByDescending(x => x.Count())
                        .ThenBy(x => x.Key.UserId)
                        .Take(count)
                        .Select(x => new { Likes = x.Count(), User = x.Key })
                        .ToList();

                var photoCountRaw
                    = await photos
                                .Join(_context.ChatUsers,
                                    photo => photo.FromUserId,
                                    user => user.UserId,
                                    (photo, user) => new { photo, user })
                                .ToListAsync();
                var photoCount = photoCountRaw
                            .GroupBy(x => x.user)
                            .Select(x => new { User = x.Key, Photos = x.Count() })
                            .ToList();

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
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        private IQueryable<Photo> FilterPhotosBySourceChat(long? sourceChatId, IQueryable<Photo> photos) 
            => photos
                   .Join(_context.RepostSettings,
                         photo => photo.ChatId.ToString(),
                         setting => setting.TargetId,
                         (photo, setting) => new { Photo = photo, Setting = setting })
                   .Where(x => x.Setting.ChatId == sourceChatId.Value)
                   .Select(x => x.Photo);

        public async Task<GlobalStats> GetGlobalStats(DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            try
            {
                var picCount = await _context
                    .Photos
                    .FilterByDates(from, to)
                    .CountAsync();

                var likesCount = await _context
                    .PhotoVotes
                    .FilterByDates(from, to)
                    .CountAsync();

                var picAnyLiked = await _context.Photos
                    .FilterByDates(from, to)
                    .Join(_context.PhotoVotes,
                            photo => new { photo.MessageId, photo.ChatId },
                            vote => new { vote.MessageId, vote.ChatId },
                            (photo, vote) => new { PId = photo.Id, VId = vote.Id })
                    .GroupBy(x => x.PId)
                    .CountAsync();


                return new GlobalStats(picCount, likesCount, picAnyLiked);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e, nameof(GetGlobalStats));
                throw;
            }
        }
    }
}
