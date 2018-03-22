using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace WetPicsTelegramBot.WebApp.Helpers
{
    class InjectableJobFactory : IJobFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConcurrentDictionary<IJob, IServiceScope> _scopes = new ConcurrentDictionary<IJob, IServiceScope>();

        public InjectableJobFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var scope = _serviceScopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;

            _scopes.TryAdd(job, scope);

            return job;
        }

        public void ReturnJob(IJob job)
        {
            if (_scopes.TryRemove(job, out var scope))
            {
                scope.Dispose();
            }

            (job as IDisposable)?.Dispose();
        }
    }
}