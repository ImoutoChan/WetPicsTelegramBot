using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Database
{
    class PixivRepository : Repository<WetPicsDbContext>, IPixivRepository
    {
        private readonly ILogger<DbRepository> _logger;

        public PixivRepository(ILogger<DbRepository> logger, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = logger;
        }

        #region PixivSettings

        public List<PixivSetting> GetPixivSettings()
        {
            try
            {
                using (var db = GetDbContext())
                {
                    return db.PixivSettings.Include(x => x.PixivImagePosts).ToList();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to get pixivSettings" + e.ToString());
                throw;
            }
        }

        public async Task<List<PixivSetting>> GetPixivSettingsAsync()
        {
            try
            {
                using (var db = GetDbContext())
                {
                    return await db.PixivSettings.Include(x => x.PixivImagePosts).ToListAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to get pixivSettings async" + e.ToString());
                throw;
            }
        }

        public async Task SetPixivSettings(long chatId, PixivTopType type, int intervalMinutes)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var settings = await db.PixivSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (settings == null)
                    {
                        settings = new PixivSetting
                        {
                            ChatId = chatId
                        };
                        await db.AddAsync(settings);
                    }

                    settings.MinutesInterval = intervalMinutes;
                    settings.PixivTopType = type;
                    settings.LastPostedTime = null;

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to set pixivSettings" + e.ToString());
                throw;
            }
        }

        public async Task RemovePixivSettings(long chatId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    var settings = await db.PixivSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (settings == null)
                    {
                        return;
                    }

                    db.PixivSettings.Remove(settings);

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to remove pixivSettings" + e.ToString());
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
                using (var db = GetDbContext())
                {
                    var settings = await db.PixivSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                    if (settings == null)
                    {
                        return;
                    }

                    settings.LastPostedTime = time;

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to set pixivSettings datetime" + e.ToString());
                throw;
            }
        }

        public async Task AddPosted(PixivSetting pixivSetting, int workId)
        {
            try
            {
                using (var db = GetDbContext())
                {
                    await db.PixivImagePosts.AddAsync(new PixivImagePost
                    {
                        PixivIllustrationId = workId,
                        PixivSettingId = pixivSetting.Id
                    });

                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"db unable to add posted" + e.ToString());
                throw;
            }
        }

        #endregion
    }
}
