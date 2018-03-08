using System;
using Quartz;
using Quartz.Spi;

namespace WetPicsTelegramBot.WebApp.Helpers
{
    class InjectableJobFactory : IJobFactory
    {
        protected readonly IServiceProvider Container;

        public InjectableJobFactory(IServiceProvider container)
        {
            Container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return Container.GetService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}