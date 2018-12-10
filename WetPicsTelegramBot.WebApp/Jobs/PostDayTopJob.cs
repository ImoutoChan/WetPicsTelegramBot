using System;
using System.Threading.Tasks;
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

        public PostDayTopJob(IScheduledResultsService dailyResultsService,
                             IRepostSettingsService repostSettingsService)
        {
            _dailyResultsService = dailyResultsService;
            _repostSettingsService = repostSettingsService;
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
                    await _repostSettingsService.Remove(repostSetting.ChatId);
                }
            }
        }
    }
}