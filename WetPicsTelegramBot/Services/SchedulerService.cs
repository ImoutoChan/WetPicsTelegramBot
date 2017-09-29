using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    class SchedulerService : ISchedulerService
    {
        private readonly IJobFactory _jobFactory;

        public SchedulerService(IJobFactory jobFactory)
        {
            _jobFactory = jobFactory;
        }

        public async Task StartScheduler()
        {
            var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            scheduler.JobFactory = _jobFactory;
            await scheduler.Start();

            var job = JobBuilder.Create<PostDayTopJob>()
                                       .WithIdentity("PostDailyStatsJob")
                                       .Build();
            
            var trigger = TriggerBuilder.Create()
                                             .WithIdentity("PostDailyAtTimeTrigger")
                                             .StartNow()
                                             .WithSchedule(CronScheduleBuilder
                                                .DailyAtHourAndMinute(20, 00)
                                                .InTimeZone(TimeZoneInfo.Utc))
                                             .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    class PostDayTopJob : IJob
    {
        private readonly IDailyResultsService _dailyResultsService;
        private readonly IRepostSettingsService _repostSettingsService;

        public PostDayTopJob(IDailyResultsService dailyResultsService,
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
