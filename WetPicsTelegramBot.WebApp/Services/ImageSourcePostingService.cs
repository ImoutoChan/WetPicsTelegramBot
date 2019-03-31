using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using WetPicsTelegramBot.Data.Entities.ImageSources;
using WetPicsTelegramBot.WebApp.Factories;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class ChatState
    {
        public int CurrentSourceIndex { get; set; }

        public int SkipTimesAfterException { get; set; } = 0;
    }


    public class ImageSourcePostingService : IImageSourcePostingService
    {
        private readonly ILogger<ImageSourcePostingService> _logger;
        private readonly IWetpicsService _wetpicsService;
        private readonly IPostingServicesFactory _postingServicesFactory;
        private static readonly Dictionary<long, ChatState> _chatSessionStore = new Dictionary<long, ChatState>();


        public ImageSourcePostingService(ILogger<ImageSourcePostingService> logger, 
                                         IWetpicsService wetpicsService,
                                         IPostingServicesFactory postingServicesFactory)
        {
            _logger = logger;
            _wetpicsService = wetpicsService;
            _postingServicesFactory = postingServicesFactory;
        }
        
        public async Task TriggerPostNext()
        {
            _logger.LogTrace("ImageSource PostNext triggered");

            try
            {
                var chatSettings = await _wetpicsService.GetChatSettings();

                _logger.LogDebug($"The number of active image source posting settings: {chatSettings.Count}");

                foreach (var chatSetting in chatSettings)
                {
                    if (!IsTimeToPost(chatSetting))
                        continue;

                    if (ShouldSkipChat(chatSetting))
                        continue;

                    _logger.LogDebug($"It's time to post | Chat id: {chatSetting.ChatId}");

                    try
                    {
                        await PostNext(chatSetting);
                    }
                    catch (ApiRequestException ex) when (ex.Message.StartsWith(
                        "Forbidden: bot was blocked by the user",
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        _logger.LogWarning("Removing wetpics settings for chatId: " + chatSetting.ChatId);
                        await _wetpicsService.Disable(chatSetting.ChatId);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        SetSkipTimes(chatSetting);
                        _logger.LogError(ex, $"Error in posting next | Chat id: {chatSetting.ChatId}");
                        continue;
                    }

                    _logger.LogInformation($"Illust posting is finished | Chat id: {chatSetting.ChatId}");

                    await _wetpicsService.UpdateLastPostedTime(chatSetting);

                    _logger.LogDebug($"LastPostedTime updated | Chat id: {chatSetting.ChatId}");
                }
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        private async Task PostNext(ImageSourcesChatSetting chatSetting, int retry = 0)
        {
            var sources = await _wetpicsService.GetImageSources(chatSetting.ChatId);

            if (!sources.Any())
                _logger.LogTrace("Sources are empty.");

            if (sources.Count <= retry)
            {
                _logger.LogWarning("All sources are empty");
                return;
            }

            var nextSourceIndex = GetNextSourceIndex(chatSetting);
            _logger.LogInformation($"Next source plain: {nextSourceIndex}");
            
            var index = nextSourceIndex % sources.Count;
            _logger.LogInformation($"Next source index: {index} / {sources.Count}");
            
            var nextSource = sources[index];
            _logger.LogInformation($"Selected source: {nextSource.ImageSource} / {nextSource.Options}");
            

            var posted = await _postingServicesFactory
               .GetPostingService(nextSource.ImageSource)
               .PostNext(chatSetting.ChatId, nextSource.Options);

            if (!posted)
                await PostNext(chatSetting, retry + 1);
        }

        private static int GetNextSourceIndex(ImageSourcesChatSetting chatSetting)
        {
            if (!_chatSessionStore.ContainsKey(chatSetting.ChatId))
                _chatSessionStore.Add(chatSetting.ChatId, new ChatState());

            var nextSource = _chatSessionStore[chatSetting.ChatId].CurrentSourceIndex;
            _chatSessionStore[chatSetting.ChatId].CurrentSourceIndex++;
            return nextSource;
        }

        private static bool ShouldSkipChat(ImageSourcesChatSetting chatSetting)
        {
            if (!_chatSessionStore.ContainsKey(chatSetting.ChatId))
                _chatSessionStore.Add(chatSetting.ChatId, new ChatState());

            var skipTimes = _chatSessionStore[chatSetting.ChatId].SkipTimesAfterException;
            if (skipTimes > 0)
                _chatSessionStore[chatSetting.ChatId].SkipTimesAfterException--;
            return skipTimes > 0;
        }

        private static void SetSkipTimes(ImageSourcesChatSetting chatSetting, int times = 50)
        {

            if (!_chatSessionStore.ContainsKey(chatSetting.ChatId))
                _chatSessionStore.Add(chatSetting.ChatId, new ChatState());

            _chatSessionStore[chatSetting.ChatId].SkipTimesAfterException = times;
        }

        private static bool IsTimeToPost(ImageSourcesChatSetting chatSetting) 
            => !chatSetting.LastPostedTime.HasValue
                || chatSetting.LastPostedTime.Value.LocalDateTime 
                <= DateTimeOffset.Now.LocalDateTime.AddMinutes(-chatSetting.MinutesInterval);
    }
}