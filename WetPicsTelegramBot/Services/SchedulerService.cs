using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace WetPicsTelegramBot.Services
{
    class SchedulerService
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
                                             .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(22, 00))
                                             .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    class PostDayTopJob : IJob
    {
        public PostDayTopJob()
        {
            
        }

        public async Task Execute(IJobExecutionContext context)
        {
        }
    }
}
