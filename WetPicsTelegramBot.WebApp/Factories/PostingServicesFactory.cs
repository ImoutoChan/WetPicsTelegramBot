using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Factories
{
    public class PostingServicesFactory : IPostingServicesFactory
    {
        private readonly PixivPostingService _pixivService;


        public PostingServicesFactory(PixivPostingService pixivService)
        {
            _pixivService = pixivService;
        }


        public IPostingService GetPostingService(ImageSource source)
        {
            switch (source)
            {
                default:
                case ImageSource.Pixiv:
                    return _pixivService;
            }
        }
    }
}