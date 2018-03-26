using System;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Services;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Factories
{
    public class PostingServicesFactory : IPostingServicesFactory
    {
        private readonly PixivPostingService _pixivService;
        private readonly YanderePostingService _yandereService;
        private readonly DanbooruPostingService _danbooruPostingService;


        public PostingServicesFactory(PixivPostingService pixivService, 
                                      YanderePostingService yandereService,
                                      DanbooruPostingService danbooruPostingService)
        {
            _pixivService = pixivService;
            _yandereService = yandereService;
            _danbooruPostingService = danbooruPostingService;
        }

        public IPostingService GetPostingService(ImageSource source)
        {
            switch (source)
            {
                case ImageSource.Danbooru:
                    return _danbooruPostingService;
                case ImageSource.Yandere:
                    return _yandereService;
                case ImageSource.Pixiv:
                    return _pixivService;
                default:
                    throw new NotImplementedException(source.ToString());
            }
        }
    }
}