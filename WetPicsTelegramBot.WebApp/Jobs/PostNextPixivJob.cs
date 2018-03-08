using System.Threading.Tasks;
using Quartz;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Jobs
{
    class PostNextPixivJob : IJob
    {
        private readonly IPixivService _pixivService;

        public PostNextPixivJob(IPixivService pixivService)
        {
            _pixivService = pixivService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _pixivService.TriggerPostNext();
        }
    }
}