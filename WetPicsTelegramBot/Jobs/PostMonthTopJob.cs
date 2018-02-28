using System.Threading.Tasks;
using Quartz;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Jobs
{
    class PostMonthTopJob : IJob
    {
        private readonly IScheduledResultsService _dailyResultsService;
        private readonly IRepostSettingsService _repostSettingsService;

        public PostMonthTopJob(IScheduledResultsService dailyResultsService,
                               IRepostSettingsService repostSettingsService)
        {
            _dailyResultsService = dailyResultsService;
            _repostSettingsService = repostSettingsService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            foreach (var repostSetting in _repostSettingsService.Settings)
            {
                await _dailyResultsService.PostMonthlyResults(repostSetting.ChatId);
            }
        }
    }
}