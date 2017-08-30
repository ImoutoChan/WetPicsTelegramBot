﻿using System;
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
                _logger.LogError(e, $"Error occured in {nameof(AddPhoto)} method");
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

                    if (photoVote == null)
                    {
                        photoVote = new PhotoVote
                        {
                            ChatId = chatId,
                            MessageId = messageId,
                            UserId = userId
                        };

                        await db.PhotoVotes.AddAsync(photoVote);
                    }

                    await db.SaveChangesAsync();

                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured in {nameof(AddOrUpdateVote)} method");
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
                _logger.LogError(e, $"Error occured in {nameof(GetVotes)} method");
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
                    }

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured in {nameof(RemoveRepostSettings)} method");
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
                _logger.LogError(e, $"Error occured in {nameof(SetRepostSettings)} method");
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
                _logger.LogError(e, $"Error occured in {nameof(GetRepostSettingsAsync)} method");
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
                _logger.LogError(e, $"Error occured in {nameof(GetRepostSettings)} method");
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
                _logger.LogError(e, $"Error occured in {nameof(GetStats)} method (userId: {userId})");
                throw;
            }
        }
    }
}
