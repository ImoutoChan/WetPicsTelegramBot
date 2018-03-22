using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public ImageSourceRepository(WetPicsDbContext context, 
            ILogger<ImageSourceRepository> logger)
        {
            _context = context;
            _logger = logger;
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


        public async Task AddPostedAsync(int chatSettingId, ImageSource source, int postId)
        {
            try
            {
                await _context.PostedImages.AddAsync(new PostedImage
                {
                    ImageSourcesChatSettingId = chatSettingId,
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

        public async Task<int?> GetFirstUnpostedNativeAsync(int chatSettingId,
            ImageSource imageSource,
            int[] workIds)
        {
            try
            {
                var alreadyPosted = await _context.PostedImages
                    .Where(x => x.ImageSourcesChatSettingId == chatSettingId
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


        public async Task<List<ImageSourceSetting>> GetImageSourcesForChatAsync(int chatSettingId)
        {
            try
            {
                return await _context
                    .ImageSourceSettings
                    .Where(x => x.ImageSourcesChatSettingId == chatSettingId)
                    .ToListAsync();
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

                var imageSource = new ImageSourceSetting
                {
                    ImageSource = source,
                    ImageSourcesChatSetting = chatSetting,
                    Options = options
                };

                await _context.AddAsync(imageSource);
                await _context.SaveChangesAsync();
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
                var imageSource = _context.ImageSourceSettings.FirstOrDefaultAsync(x => x.Id == imageSourceSettingId);

                if (imageSource == null)
                {
                    return;
                }

                _context.Remove(imageSource);
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
