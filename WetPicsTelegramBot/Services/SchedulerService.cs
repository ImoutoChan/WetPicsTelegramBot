using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using WetPicsTelegramBot.Jobs;
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

            await ScheduleDaily(scheduler);
            await ScheduleMonthly(scheduler);
        }

        private static async Task ScheduleDaily(IScheduler scheduler)
        {
            var dailyJob = JobBuilder
                            .Create<PostDayTopJob>()
                            .WithIdentity("PostDailyStatsJob")
                            .Build();

            var dailyTrigger = TriggerBuilder
                                .Create()
                                .WithIdentity("PostDailyAtTimeTrigger")
                                .StartNow()
                                .WithSchedule(CronScheduleBuilder
                                    .DailyAtHourAndMinute(20, 00)
                                    .InTimeZone(TimeZoneInfo.Utc))
                                .Build();

            await scheduler.ScheduleJob(dailyJob, dailyTrigger);
        }

        private static async Task ScheduleMonthly(IScheduler scheduler)
        {
            var monthlyJob = JobBuilder
                            .Create<PostMonthTopJob>()
                            .WithIdentity("PostMonthlyStatsJob")
                            .Build();

            var monthlyTrigger = TriggerBuilder
                                .Create()
                                .WithIdentity("PostMonthlyAtTimeTrigger")
                                .StartNow()
                                .WithSchedule(CronScheduleBuilder
                                                .MonthlyOnDayAndHourAndMinute(1, 17, 05)
                                                .InTimeZone(TimeZoneInfo.Utc))
                                .Build();

            await scheduler.ScheduleJob(monthlyJob, monthlyTrigger);
        }
    }
}
