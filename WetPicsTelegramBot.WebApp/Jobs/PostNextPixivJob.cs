using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Services;

namespace WetPicsTelegramBot.WebApp.Jobs
{
    class PostNextPixivJob : IJob
    {
        private readonly IImageSourcePostingService _imageSourcePostingService;
        private readonly ILogger<PostNextPixivJob> _logger;

        private static readonly SemaphoreSlim _postNextSemahore = new SemaphoreSlim(1);

        public PostNextPixivJob(IImageSourcePostingService imageSourcePostingService,
                                ILogger<PostNextPixivJob> logger)
        {
            _imageSourcePostingService = imageSourcePostingService;
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
                await _imageSourcePostingService.TriggerPostNext();
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