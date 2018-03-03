using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Data.Context;
using WetPicsTelegramBot.Data.Entities;
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

        #region PixivSettings

        public List<PixivSetting> GetPixivSettings()
        {
            try
            {
                
                return _context.PixivSettings.Include(x => x.PixivImagePosts).ToList();
                
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetPixivSettings)} method");
                throw;
            }
        }

        public async Task<List<PixivSetting>> GetPixivSettingsAsync()
        {
            try
            {
                return await _context.PixivSettings.Include(x => x.PixivImagePosts).ToListAsync();
                
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(GetPixivSettingsAsync)} method");
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
                _logger.LogError(e, $"Error occurred in {nameof(SetPixivSettings)} method");
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
                _logger.LogError(e, $"Error occurred in {nameof(RemovePixivSettings)} method");
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
                _logger.LogError(e, $"Error occurred in {nameof(UpdateLastPostedTime)} method");
                throw;
            }
        }

        public async Task AddPosted(PixivSetting pixivSetting, int workId)
        {
            try
            {
                await _context.PixivImagePosts.AddAsync(new PixivImagePost
                {
                    PixivIllustrationId = workId,
                    PixivSettingId = pixivSetting.Id
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(AddPosted)} method");
                throw;
            }
        }

        #endregion
    }
}
