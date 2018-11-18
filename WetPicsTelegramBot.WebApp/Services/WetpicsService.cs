using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.Data.Entities.ImageSources;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Services.Abstract;
using WetPicsTelegramBot.WebApp.Services.PostingServices;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class WetpicsService : IWetpicsService
    {
        private readonly ILogger<PixivPostingService> _logger;
        private readonly IImageSourceRepository _imageSourceRepository;

        public WetpicsService(ILogger<PixivPostingService> logger, 
            IImageSourceRepository imageSourceRepository)
        {
            _logger = logger;
            _imageSourceRepository = imageSourceRepository;
        }

        public async Task Enable(long chatId, int interval)
        {
            try
            {
                await _imageSourceRepository.AddImageSourceChatSettingAsync(chatId, interval);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }
        
        public async Task Disable(long chatId)
        {
            try
            {
                await _imageSourceRepository.UpdateLastPostedTimeAsync(chatId, DateTimeOffset.MaxValue);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public async Task AddImageSource(long chatId, ImageSource source, string options)
        {
            try
            {
                await _imageSourceRepository.AddImageSourceAsync(chatId, source, options);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public Task<List<ImageSourceSetting>> GetImageSources(long chatId)
        {
            try
            {
                return _imageSourceRepository.GetImageSourcesForChatAsync(chatId);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public Task RemoveImageSource(int imageSourceSettingId)
        {
            try
            {
                return _imageSourceRepository.RemoveImageSourceAsync(imageSourceSettingId);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public Task<List<ImageSourcesChatSetting>> GetChatSettings()
        {
            try
            {
                return _imageSourceRepository.GetImageSourceChatSettingsAsync();
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public Task UpdateLastPostedTime(ImageSourcesChatSetting chatSetting)
        {
            try
            {
                return _imageSourceRepository.UpdateLastPostedTimeAsync(chatSetting.ChatId);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public Task<int?> GetFirstUnpostedAsync(long chatId, ImageSource source, int[] ids)
        {
            try
            {
                return _imageSourceRepository.GetFirstUnpostedNativeAsync(chatId, source, ids);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        public Task AddPosted(long chatId, ImageSource source, int workId)
        {
            try
            {
                return _imageSourceRepository.AddPostedAsync(chatId, source, workId);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }
    }
}