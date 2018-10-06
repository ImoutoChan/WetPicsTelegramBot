using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Data.Context;
using WetPicsTelegramBot.Data.Entities.ImageSources;
using WetPicsTelegramBot.Data.Helpers;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.Data
{
    public class ImageSourceRepository : IImageSourceRepository
    {
        private readonly ILogger<ImageSourceRepository> _logger;
        private readonly WetPicsDbContext _context;
        private readonly IMemoryCache _imageSourceSettingsCache;

        public ImageSourceRepository(WetPicsDbContext context, 
                                     ILogger<ImageSourceRepository> logger,
                                     IMemoryCache imageSourceSettingsCache)
        {
            _context = context;
            _logger = logger;
            _imageSourceSettingsCache = imageSourceSettingsCache;
        }

        public async Task<List<ImageSourcesChatSetting>> GetImageSourceChatSettingsAsync()
        {
            try
            {
                return await _context
                    .ImageSourcesChatSettings
                    .ToListAsync();
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task AddImageSourceChatSettingAsync(long chatId, int intervalMinutes)
        {
            try
            {
                var settings = await _context.ImageSourcesChatSettings
                    .FirstOrDefaultAsync(x => x.ChatId == chatId);

                if (settings == null)
                {
                    settings = new ImageSourcesChatSetting
                    {
                        ChatId = chatId
                    };
                    await _context.AddAsync(settings);
                }

                settings.MinutesInterval = intervalMinutes;
                settings.LastPostedTime = null;

                await _context.SaveChangesAsync();

            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task UpdateLastPostedTimeAsync(long chatId, 
            DateTimeOffset time = default(DateTimeOffset))
        {
            if (time == default(DateTimeOffset))
            {
                time = DateTimeOffset.Now;
            }

            try
            {
                var settings = await _context.ImageSourcesChatSettings
                    .FirstOrDefaultAsync(x => x.ChatId == chatId);

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
        
        public async Task AddPostedAsync(long chatId, ImageSource source, int postId)
        {
            try
            {
                var setting = await _context.ImageSourcesChatSettings.FirstOrDefaultAsync(x => x.ChatId == chatId);

                if (setting == null)
                {
                    setting = new ImageSourcesChatSetting
                    {
                        ChatId = chatId,
                        MinutesInterval = Int32.MaxValue,
                        LastPostedTime = DateTimeOffset.MaxValue
                    };

                    await _context.ImageSourcesChatSettings.AddAsync(setting);
                }

                await _context.PostedImages.AddAsync(new PostedImage
                {
                    ImageSourcesChatSetting = setting,
                    ImageSource =  source,
                    PostId = postId
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task<int?> GetFirstUnpostedNativeAsync(long chatId,
                                                            ImageSource imageSource,
                                                            int[] workIds)
        {
            try
            {
                var alreadyPosted = await _context.PostedImages
                    .Where(x => x.ImageSourcesChatSetting.ChatId == chatId
                                && x.ImageSource == imageSource)
                    .Select(x => x.PostId)
                    .Where(x => workIds.Contains(x))
                    .ToListAsync();

                var result = workIds.Except(alreadyPosted).FirstOrDefault();

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

        public async Task<List<ImageSourceSetting>> GetImageSourcesForChatAsync(long chatId)
        {
            try
            {
                if (!_imageSourceSettingsCache.TryGetValue<List<ImageSourceSetting>>(chatId, out var result))
                {
                    result = await _context
                       .ImageSourceSettings
                       .Where(x => x.ImageSourcesChatSetting.ChatId == chatId)
                       .ToListAsync();

                    _imageSourceSettingsCache.Set(chatId, result);
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task AddImageSourceAsync(long chatId, ImageSource source, string options)
        {
            try
            {
                var chatSetting = await _context
                    .ImageSourcesChatSettings
                    .FirstOrDefaultAsync(x => x.ChatId == chatId);

                if (chatSetting == null)
                {
                    chatSetting = new ImageSourcesChatSetting
                    {
                        ChatId = chatId,
                        LastPostedTime = DateTimeOffset.MaxValue
                    };

                    _context.Attach(chatSetting);
                }

                var imageSource = new ImageSourceSetting
                {
                    ImageSource = source,
                    ImageSourcesChatSetting = chatSetting,
                    Options = options
                };

                await _context.AddAsync(imageSource);
                await _context.SaveChangesAsync();

                _imageSourceSettingsCache.Remove(chatId);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task RemoveImageSourceAsync(int imageSourceSettingId)
        {
            try
            {
                var imageSource = await _context
                   .ImageSourceSettings
                   .Include(x => x.ImageSourcesChatSetting)
                   .FirstOrDefaultAsync(x => x.Id == imageSourceSettingId);

                if (imageSource == null)
                {
                    return;
                }

                _context.Remove(imageSource);
                await _context.SaveChangesAsync();


                _imageSourceSettingsCache.Remove(imageSource.ImageSourcesChatSetting.ChatId);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }
    }
}
