using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Base;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Extensions;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services.PostingServices
{
    public class YanderePostingService : IPostingService
    {
        private readonly ITgClient _tgClient;
        private readonly ILogger _logger;
        private readonly IRepostService _repostService;
        private readonly IWetpicsService _wetpicsService;
        private readonly ITelegramImagePreparing _telegramImagePreparing;
        private readonly YandereLoader _yandereLoader;
        private readonly HttpClient _httpClient;


        public YanderePostingService(ITgClient tgClient,
                                     ILogger<YanderePostingService> logger,
                                     IRepostService repostService, 
                                     IWetpicsService wetpicsService, 
                                     ITelegramImagePreparing telegramImagePreparing,
                                     YandereLoader yandereLoader,
                                     HttpClient httpClient)
        {
            _tgClient = tgClient;
            _logger = logger;
            _repostService = repostService;
            _wetpicsService = wetpicsService;
            _telegramImagePreparing = telegramImagePreparing;
            _yandereLoader = yandereLoader;
            _httpClient = httpClient;
        }

        public async Task<bool> PostNext(long chatId, string sourceOptions)
        {
            if (!Enum.TryParse(sourceOptions, out YandereTopType yandereTopType))
            {
                _logger.LogError("Invalid source option.");
                throw new ArgumentException(nameof(sourceOptions));
            }

            try
            {
                _logger.LogTrace("Loading yandere illust list");

                var popularType = MapType(yandereTopType);
                var posts = await _yandereLoader.LoadPopularAsync(popularType);
                

                _logger.LogTrace("Selecting new one");
                var next = await _wetpicsService
                   .GetFirstUnpostedAsync(chatId,
                                          ImageSource.Yandere,
                                          posts.Results.Select(x => x.Id).ToArray());

                if (next == null)
                {
                    _logger.LogDebug("There isn't any new illusts");
                    return false;
                }

                var work = posts.Results.First(x => x.Id == next);

                _logger.LogDebug($"Posting illust | IllustId: {work.Id}");
                await PostIllust(chatId, work, popularType);

                _logger.LogDebug($"Adding posted illust | IllustId: {work.Id}");
                await _wetpicsService.AddPosted(chatId, ImageSource.Yandere, work.Id);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        private PopularType MapType(YandereTopType yandereTopType)
        {
            switch (yandereTopType)
            {
                case YandereTopType.Day:
                    return PopularType.Day;
                case YandereTopType.Week:
                    return PopularType.Week;
                case YandereTopType.Month:
                    return PopularType.Month;
                default:
                    throw new ArgumentOutOfRangeException(nameof(yandereTopType), yandereTopType, null);
            }
        }

        private async Task PostIllust(long chatId, PreviewEntry postEntry, PopularType type)
        {
            var post = await _yandereLoader.LoadPostAsync(postEntry.Id);

            var imageUrl = post.OriginalUrl;

            _logger.LogDebug($"Illust url: {imageUrl}");

            _logger.LogTrace($"Downloading stream");
            using (var content = await DownloadStreamAsync(imageUrl))
            {
                var caption 
                    = $"<a href=\"https://yande.re/post/show/{post.PostId}\">Yandere {type.MakeAdverb()} top # {post.PostId}</a>";
                _logger.LogDebug($"Caption: {caption}");

                _logger.LogTrace($"Sending image to chat");
                var sendedMessage 
                    = await _tgClient.Client.SendPhotoAsync(chatId,
                                                            new InputOnlineFile(content),
                                                            caption,
                                                            ParseMode.Html);

                _logger.LogTrace($"Reposting image to channel");

                var target = await _repostService.TryGetRepostTargetChat(sendedMessage.Chat.Id);
                if (target == null)
                    return;

                await _repostService.RepostWithLikes(sendedMessage, target, caption);
            }
        }

        private async Task<Stream> DownloadStreamAsync(string image)
        {
            var response = await _httpClient.GetAsync(image);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();

            if (!response.TryGetLength(out var length))
                throw new WebException("Incorrect file length.");

            return _telegramImagePreparing.Prepare(responseStream, length);
        }
    }
}
