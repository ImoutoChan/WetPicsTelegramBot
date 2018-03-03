using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
    public class PixivRepository : IPixivRepository
    {
        private readonly ILogger<DbRepository> _logger;
        private readonly WetPicsDbContext _context;

        public PixivRepository(ILogger<DbRepository> logger, WetPicsDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<int?> GetFirstUnpostedAsync(int pixivSettingId, long[] workIds)
        {
            try
            {
                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText =
                            $@"SELECT newWorkIs
                                FROM unnest(ARRAY[{String.Join(", ", workIds)}]) newWorkIs
                                WHERE NOT EXISTS 
	                                (SELECT * 
 	                                 FROM ""PixivImagePosts"" pip 
 	                                 WHERE pip.""PixivIllustrationId"" = newWorkIs 
                                            AND pip.""PixivSettingId"" = {pixivSettingId})
                                LIMIT 1";
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                                return null;

                            await reader.ReadAsync();
                            return reader.GetInt32(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task<long?> GetFirstUnpostedNativeAsync(int pixivSettingId, long[] workIds)
        {
            try
            {
                var alreadyPosted = 
                    await _context
                        .PixivImagePosts
                        .Select(x => x.PixivIllustrationId)
                        .Where(x => workIds.Contains(x))
                        .ToListAsync();

                var result = workIds.Except(alreadyPosted.Cast<long>()).FirstOrDefault();

                if (result != 0)
                {
                    return result;
                }

                return null;
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task<List<PixivSetting>> GetPixivSettingsAsync()
        {
            try
            {
                return await _context
                    .PixivSettings
                    .ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task SetPixivSettings(long chatId, PixivTopType type, int intervalMinutes)
        {
            try
            {
                var settings = await _context.PixivSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                if (settings == null)
                {
                    settings = new PixivSetting
                    {
                        ChatId = chatId
                    };
                    await _context.AddAsync(settings);
                }

                settings.MinutesInterval = intervalMinutes;
                settings.PixivTopType = type;
                settings.LastPostedTime = null;

                await _context.SaveChangesAsync();
                
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task RemovePixivSettings(long chatId)
        {
            try
            {
                var settings = await _context.PixivSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                if (settings == null)
                {
                    return;
                }

                _context.PixivSettings.Remove(settings);

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task UpdateLastPostedTime(long chatId, DateTimeOffset time = default(DateTimeOffset))
        {
            if (time == default(DateTimeOffset))
            {
                time = DateTimeOffset.Now;
            }

            try
            {
                var settings = await _context.PixivSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                if (settings == null)
                {
                    return;
                }

                settings.LastPostedTime = time;

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task AddPosted(int pixivSettingId, int workId)
        {
            try
            {
                await _context.PixivImagePosts.AddAsync(new PixivImagePost
                {
                    PixivIllustrationId = workId,
                    PixivSettingId = pixivSettingId
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }
    }
}
