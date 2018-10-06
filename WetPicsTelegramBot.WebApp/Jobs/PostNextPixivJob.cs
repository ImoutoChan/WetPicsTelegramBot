using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Jobs
{
    internal class PostNextImageSourceJob : IJob
    {
        private readonly IImageSourcePostingService _imageSourcePostingService;
        private readonly ILogger<PostNextImageSourceJob> _logger;

        private static readonly SemaphoreSlim _postNextSemahore = new SemaphoreSlim(1);

        public PostNextImageSourceJob(IImageSourcePostingService imageSourcePostingService,
                                      ILogger<PostNextImageSourceJob> logger)
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