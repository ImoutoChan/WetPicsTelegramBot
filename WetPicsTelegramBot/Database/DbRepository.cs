using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public async Task AddPhoto(string fromUserId, string chatId, int messageId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var photo = await db.Photos.FirstOrDefaultAsync(x => x.ChatId == chatId && x.MessageId == messageId);

                    if (photo != null)
                    {
                        return;
                    }

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
                _logger.LogError($"db unable to add photo" + e.ToString());
                throw;
            }
        }

        public async Task AddOrUpdateVote(string userId, string chatId, int messageId, int? score = null, bool? isLiked = null)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var photoVote =
                        await db.PhotoVotes.FirstOrDefaultAsync(x => x.ChatId == chatId && x.MessageId == messageId && x.UserId == userId);

                    if (photoVote != null)
                    {
                        if (score.HasValue)
                        {
                            photoVote.Score = score.Value;
                        }
                        if (isLiked.HasValue)
                        {
                            photoVote.IsLiked = isLiked.Value;
                        }
                    }
                    else
                    {
                        photoVote = new PhotoVote
                        {
                            ChatId = chatId,
                            MessageId = messageId,
                            UserId = userId,
                            Score = score,
                            IsLiked = isLiked
                        };

                        await db.PhotoVotes.AddAsync(photoVote);
                    }

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to save vote" + e.ToString());
                throw;
            }
        }

        public async Task<Vote> GetVotes(long messageId, string chatId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var photoVotes = await db.PhotoVotes.Where(x => x.MessageId == messageId && x.ChatId == chatId).ToListAsync();

                    var result = new Vote
                    {
                        Scores =
                        {
                            [1] = photoVotes.Count(x => x.Score == 1),
                            [2] = photoVotes.Count(x => x.Score == 2),
                            [3] = photoVotes.Count(x => x.Score == 3),
                            [4] = photoVotes.Count(x => x.Score == 4),
                            [5] = photoVotes.Count(x => x.Score == 5)
                        },
                        Liked = photoVotes.Count(x => x.IsLiked == true),
                        Disliked = photoVotes.Count(x => x.IsLiked == false)
                    };



                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to get vote" + e.ToString());
                return default(Vote);
            }
        }

        public async Task RemoveChatSettings(string chatId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var chatSettings = await db.ChatSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (chatSettings != null)
                    {
                        db.ChatSettings.Remove(chatSettings);
                    }

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to remove chatSetting" + e.ToString());
                throw;
            }
        }

        public async Task SetChatSettings(string chatId, string targetId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var chatSettings = await db.ChatSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (chatSettings == null)
                    {
                        var newChatSettings = new ChatSetting
                        {
                            ChatId = chatId,
                            TargetId = targetId
                        };

                        await db.ChatSettings.AddAsync(newChatSettings);
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
                _logger.LogError($"db unable to set chatSetting" + e.ToString());
                throw;
            }
        }

        public async Task<List<ChatSetting>> GetChatSettingsAsync()
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var chatSettings = await db.ChatSettings.ToListAsync();
                    
                    return chatSettings;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to get chatSettings" + e.ToString());
                throw;
            }
        }

        public List<ChatSetting> GetChatSettings()
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var chatSettings = db.ChatSettings.ToList();

                    return chatSettings;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to get chatSettings" + e.ToString());
                throw;
            }
        }

        public async Task<Stats> GetStats(string userId)
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
                                    (photo, vote) => new { photo, vote})
                        .CountAsync(x => x.vote.IsLiked == true);

                    var setLikeCount = await db.PhotoVotes.CountAsync(x => x.UserId == userId);
                    var setSelfLikeCount = await 
                        db.Photos
                        .Where(x => x.FromUserId == userId)
                        .Join(db.PhotoVotes,
                                    photo => new { photo.MessageId, photo.ChatId },
                                    vote => new { vote.MessageId, vote.ChatId },
                                    (photo, vote) => new { photo, vote })
                        .CountAsync(x => x.vote.IsLiked == true && x.vote.UserId == userId);


                    return new Stats(picCount, getLikeCount, setLikeCount, setSelfLikeCount);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to get stats ({userId})" + e.ToString());
                throw;
            }
        }
    }
}
