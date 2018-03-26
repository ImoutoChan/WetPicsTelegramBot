using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Factories
{
    public class PostingServicesFactory : IPostingServicesFactory
    {
        private readonly PixivPostingService _pixivService;
        private readonly YanderePostingService _yandereService;


        public PostingServicesFactory(PixivPostingService pixivService, 
                                      YanderePostingService yandereService)
        {
            _pixivService = pixivService;
            _yandereService = yandereService;
        }

        public IPostingService GetPostingService(ImageSource source)
        {
            switch (source)
            {
                default:
                case ImageSource.Yandere:
                    return _yandereService;
                case ImageSource.Pixiv:
                    return _pixivService;
            }
        }
    }
}