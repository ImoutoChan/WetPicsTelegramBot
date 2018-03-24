using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Jobs
{
    class PostNextPixivJob : IJob
    {
        private readonly IPixivService _pixivService;
        private readonly ILogger<PostNextPixivJob> _logger;

        private static readonly SemaphoreSlim _postNextSemahore = new SemaphoreSlim(1);

        public PostNextPixivJob(IPixivService pixivService,
                                ILogger<PostNextPixivJob> logger)
        {
            _pixivService = pixivService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {

            if (!_postNextSemahore.Wait(0))
            {
                _logger.LogWarning("PostNext is already executing");
                return;
            }

            try
            {
                await _pixivService.TriggerPostNext();
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
            }
            finally
            {
                _postNextSemahore.Release();
            }
        }
    }
}