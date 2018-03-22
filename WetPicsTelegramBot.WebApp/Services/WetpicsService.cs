using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class WetpicsService : IWetpicsService
    {
        private readonly ILogger<PixivService> _logger;
        private readonly IImageSourceRepository _imageSourceRepository;

        public WetpicsService(ILogger<PixivService> logger, 
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
    }
}