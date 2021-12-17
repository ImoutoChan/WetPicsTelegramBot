using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Imouto.BooruParser.Loaders;
using Imouto.BooruParser.Model.Base;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Extensions;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services.PostingServices
{
    public class DanbooruPostingService : IPostingService
    {
        private static readonly IMemoryCache TopCache = new MemoryCache(new MemoryCacheOptions());

        private readonly ITgClient _tgClient;
        private readonly ILogger _logger;
        private readonly IRepostService _repostService;
        private readonly IWetpicsService _wetPicsService;
        private readonly ITelegramImagePreparing _telegramImagePreparing;
        private readonly DanbooruLoader _danbooruLoader;
        private readonly HttpClient _httpClient;
        private static readonly string[] SupportedImageExtensions
            = { "jpg", "png", "jpeg", "bmp", "gif" };


        public DanbooruPostingService(ITgClient tgClient,
                                      ILogger<DanbooruPostingService> logger,
                                      IRepostService repostService,
                                      IWetpicsService wetPicsService,
                                      ITelegramImagePreparing telegramImagePreparing,
                                      DanbooruLoader danbooruLoader,
                                      HttpClient httpClient)
        {
            _tgClient = tgClient;
            _logger = logger;
            _repostService = repostService;
            _wetPicsService = wetPicsService;
            _telegramImagePreparing = telegramImagePreparing;
            _danbooruLoader = danbooruLoader;
            _httpClient = httpClient;
        }

        public async Task<bool> PostNext(long chatId, string sourceOptions)
        {
            return await PostWithCustomSkip(chatId, sourceOptions);
        }

        private async Task<bool> PostWithCustomSkip(long chatId, string sourceOptions, List<int> skip = null)
        {
            skip = skip ?? new List<int>();

            if (!Enum.TryParse(sourceOptions, out DanbooruTopType danbooruTopType))
            {
                _logger.LogError("Invalid source option.");
                throw new ArgumentException(nameof(sourceOptions));
            }

            try
            {
                _logger.LogTrace("Loading danbooru illust list");

                var popularType = MapType(danbooruTopType);

                var posts = await TopCache.GetOrCreateAsync(
                    "danbooru_" + popularType,
                    async (entry) =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                        return await _danbooruLoader.LoadPopularAsync(popularType);
                    });

                _logger.LogTrace("Selecting new one");
                var next = await _wetPicsService
                   .GetFirstUnpostedAsync(chatId,
                                          ImageSource.Danbooru,
                                          posts.Results.Where(x => skip.All(s => s != x.Id)).Select(x => x.Id)
                                             .ToArray());

                if (next == null)
                {
                    _logger.LogDebug("There isn't any new illusts");
                    return false;
                }

                var work = posts.Results.First(x => x.Id == next);

                _logger.LogDebug($"Posting illust | IllustId: {work.Id}");
                var result = await PostIllust(chatId, work, popularType);

                if (!result)
                {
                    skip.Add(work.Id);
                    return await PostWithCustomSkip(chatId, sourceOptions, skip);
                }

                _logger.LogDebug($"Adding posted illust | IllustId: {work.Id}");
                await _wetPicsService.AddPosted(chatId, ImageSource.Danbooru, work.Id);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        private PopularType MapType(DanbooruTopType danbooruTopType)
        {
            switch (danbooruTopType)
            {
                case DanbooruTopType.Day:
                    return PopularType.Day;
                case DanbooruTopType.Week:
                    return PopularType.Week;
                case DanbooruTopType.Month:
                    return PopularType.Month;
                default:
                    throw new ArgumentOutOfRangeException(nameof(danbooruTopType), danbooruTopType, null);
            }
        }

        private async Task<bool> PostIllust(long chatId, PreviewEntry postEntry, PopularType type)
        {
            var post = await _danbooruLoader.LoadPostAsync(postEntry.Id);

            var imageUrl = post.OriginalUrl;

            if (!IsImage(imageUrl))
            {
                return false;
            }

            _logger.LogDebug($"Illust url: {imageUrl}");

            _logger.LogTrace($"Downloading stream");
            using (var content = await DownloadStreamAsync(imageUrl))
            {
                var caption =
                    $"<a href=\"https://danbooru.donmai.us/posts/{post.PostId}\">Danbooru {type.MakeAdverb()} top # {post.PostId}</a>";
                _logger.LogDebug($"Caption: {caption}");

                _logger.LogTrace($"Sending image to chat");
                var sentMessage
                    = await _tgClient.Client.SendPhotoAsync(chatId,
                                                            new InputOnlineFile(content),
                                                            caption,
                                                            ParseMode.Html);

                _logger.LogTrace($"Reposting image to channel");

                var target = await _repostService.TryGetRepostTargetChat(sentMessage.Chat.Id);
                if (target == null)
                    return true;

                await _repostService.RepostWithLikes(sentMessage, target, caption);
            }

            return true;
        }

        private static bool IsImage(string imageUrl)
            => SupportedImageExtensions.Any(imageUrl.EndsWith);

        private async Task<Stream> DownloadStreamAsync(string image)
        {
            var url = image.StartsWith("/") ? $"https://danbooru.donmai.us{image}" : image;

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseStream = await response.Content.ReadAsStreamAsync();

            if (!response.TryGetLength(out var length))
                throw new WebException("Incorrect file length.");

            return _telegramImagePreparing.Prepare(responseStream, length);
        }
    }
}
