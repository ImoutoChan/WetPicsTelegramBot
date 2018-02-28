using System.Threading.Tasks;
using Quartz;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Jobs
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
            foreach (var repostSetting in _repostSettingsService.Settings)
            {
                await _dailyResultsService.PostDailyResults(repostSetting.ChatId);
            }
        }
    }
}