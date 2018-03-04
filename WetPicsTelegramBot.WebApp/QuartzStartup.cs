using System;
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
            if (_scheduler != null)
            {
                throw new InvalidOperationException("Already started.");
            }

            var schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler().Result;
            _scheduler.JobFactory = new InjectableJobFactory(_container);
            _scheduler.Start().Wait();


            SchedulePixiv(_scheduler);
        }

        private static void SchedulePixiv(IScheduler scheduler)
        {
            var dailyJob = JobBuilder
                          .Create<PostNextPixivJob>()
                          .WithIdentity("PostNextPixivJob")
                          .Build();

            var dailyTrigger = TriggerBuilder
                              .Create()
                              .WithIdentity("PostEveryMinuteAtTimeTrigger")
                              .StartNow()
                              .WithSimpleSchedule(x => x.WithIntervalInSeconds(15).RepeatForever())
                              .Build();

            scheduler.ScheduleJob(dailyJob, dailyTrigger).Wait();
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