using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Telegram.Bot.Exceptions;
using WetPicsTelegramBot.WebApp.Models;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Jobs
{
    class PostDayTopJob : IJob
    {
        private readonly IScheduledResultsService _dailyResultsService;
        private readonly IRepostSettingsService _repostSettingsService;
        private readonly ILogger<PostDayTopJob> _logger;

        public PostDayTopJob(IScheduledResultsService dailyResultsService,
                             IRepostSettingsService repostSettingsService,
                             ILogger<PostDayTopJob> logger)
        {
            _dailyResultsService = dailyResultsService;
            _repostSettingsService = repostSettingsService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var settings = await _repostSettingsService.GetSettings();

            foreach (var repostSetting in settings)
            {
                try
                {
                    await _dailyResultsService.PostResults(repostSetting.ChatId, ScheduledResultType.Daily);
                }
                catch (ApiRequestException ex)
                    when (ex.Message.StartsWith(
                        "Forbidden: bot was blocked by the user",
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Removing repost settings for chatId: " + repostSetting.ChatId);
                    await _repostSettingsService.Remove(repostSetting.ChatId);
                }
            }
        }
    }
}