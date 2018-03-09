﻿using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Jobs;

namespace WetPicsTelegramBot.WebApp
{
    public class QuartzStartup
    {
        private IScheduler _scheduler;
        private readonly IServiceProvider _container;

        public QuartzStartup(IServiceProvider container)
        {
            _container = container;
        }

        public void Start()
        {
            StartAsync().Wait();
        }

        public async Task StartAsync()
        {
            if (_scheduler != null)
            {
                throw new InvalidOperationException("Already started.");
            }

            var schedulerFactory = new StdSchedulerFactory();
            _scheduler = await schedulerFactory.GetScheduler();
            _scheduler.JobFactory = new InjectableJobFactory(_container);
            await _scheduler.Start();


            await SchedulePixiv(_scheduler);
            await ScheduleDaily(_scheduler);
            await ScheduleMonthly(_scheduler);
        }

        private static async Task SchedulePixiv(IScheduler scheduler)
        {
            var dailyJob = JobBuilder
                          .Create<PostNextPixivJob>()
                          .WithIdentity("PostNextPixivJob")
                          .Build();

            var dailyTrigger = TriggerBuilder
                              .Create()
                              .WithIdentity("PostEveryMinuteAtTimeTrigger")
                              .StartNow()
                              .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
                              .Build();

            await scheduler.ScheduleJob(dailyJob, dailyTrigger);
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

        public void Stop()
        {
            if (_scheduler == null)
            {
                return;
            }

            // give running jobs 30 sec (for example) to stop gracefully
            if (_scheduler.Shutdown(waitForJobsToComplete: true).Wait(30000))
            {
                _scheduler = null;
            }
            else
            {
            }
        }
    }
}