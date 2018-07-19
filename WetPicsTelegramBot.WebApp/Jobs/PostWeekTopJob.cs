using System.Threading.Tasks;
using Quartz;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Jobs
{
    class PostWeekTopJob : IJob
    {
        private readonly IScheduledResultsService _dailyResultsService;
        private readonly IRepostSettingsService _repostSettingsService;

        public PostWeekTopJob(IScheduledResultsService dailyResultsService,
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
                await _dailyResultsService.PostWeeklyResults(repostSetting.ChatId);
            }
        }
    }
}