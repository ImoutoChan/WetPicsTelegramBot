using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PixivApi.Objects;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Repositories;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services.PostingServices
{
    public class PixivPostingService : IPostingService
    {
        private readonly ITgClient _tgClient;
        private readonly ILogger _logger;
        private readonly IRepostService _repostService;
        private readonly IWetpicsService _wetpicsService;
        private readonly ITelegramImagePreparing _telegramImagePreparing;
        private readonly IPixivRepository _pixivRepository;


        public PixivPostingService(
            ITgClient tgClient,
            ILogger<PixivPostingService> logger,
            IRepostService repostService,
            IWetpicsService wetpicsService,
            ITelegramImagePreparing telegramImagePreparing,
            IPixivRepository pixivRepository)
        {
            _tgClient = tgClient;
            _logger = logger;
            _repostService = repostService;
            _wetpicsService = wetpicsService;
            _telegramImagePreparing = telegramImagePreparing;
            _pixivRepository = pixivRepository;
        }



        public async Task<bool> PostNext(long chatId, string sourceOptions)
        {
            if (!Enum.TryParse(sourceOptions, out PixivTopType pixivTopType))
            {
                _logger.LogError("Invalid source option.");
                throw new ArgumentException(nameof(sourceOptions));
            }

            try
            {
                _logger.LogTrace("Loading pixiv illust list");

                var loaded = await _pixivRepository.GetPixivTop(pixivTopType);

                var works = loaded
                   .SelectMany(x => x.Works)
                   .Select(x => x.Work)
                   .Where(x => x.Id != null)
                   .ToList();


                _logger.LogTrace("Selecting new one");
                var next = await _wetpicsService
                   .GetFirstUnpostedAsync(chatId,
                                          ImageSource.Pixiv,
                                          works.Select(x => (int)x.Id.Value).ToArray());

                if (next == null)
                {
                    _logger.LogDebug("There isn't any new illusts");
                    return false;
                }

                var work = works.First(x => x.Id == next);

                _logger.LogDebug($"Posting illust | IllustId: {work.Id}");
                await PostIllust(chatId, work, pixivTopType);

                _logger.LogDebug($"Adding posted illust | IllustId: {work.Id}");
                await _wetpicsService.AddPosted(chatId,
                                                ImageSource.Pixiv,
                                                (int)work.Id);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e);
                throw;
            }
        }

        private async Task PostIllust(long chatId,
                                      Work rankWork, PixivTopType type)
        {
            var imageUrl = rankWork.ImageUrls.Large;
            _logger.LogDebug($"Illust url: {imageUrl}");

            _logger.LogTrace($"Downloading stream");
            using (var content = await DownloadPixivStreamAsync(imageUrl))
            {
                var caption
                    = $"<a href=\"https://www.pixiv.net/member_illust.php?mode=medium&illust_id={rankWork.Id}\">" +
                    $"Pixiv {type.ToString()} # {EscapeHtml(rankWork.Title)} © {EscapeHtml(rankWork.User.Name)}</a>";
                
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

        private string EscapeHtml(string input)
            => input
               .Replace("<", "&lt;")
               .Replace(">", "&gt;")
               .Replace("&", "&amp;");
        

        private async Task<Stream> DownloadPixivStreamAsync(string image)
        {
            var myRequest = (HttpWebRequest)WebRequest.Create(image);

            myRequest.CookieContainer = new CookieContainer();

            myRequest.Method = "GET";
            myRequest.Headers = new WebHeaderCollection();
            myRequest.Accept
                = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            myRequest.Headers["User-Agent"]
                = "Mozilla/5.0 (Windows; U; Windows NT 6.0; zh-CN; rv:1.9.0.6) Gecko/2009011913 Firefox/3.0.6";

            myRequest.Headers["Accept-Language"] = "zh-cn,zh;q=0.7,ja;q=0.3";
            myRequest.Headers["Accept-Encoding"] = "gzip,deflate";
            myRequest.Headers["Accept-Charset"] = "gb18030,utf-8;q=0.7,*;q=0.7";
            myRequest.Headers["referer"]
                = "http://www.pixiv.net/member_illust.php?mode=manga&illust_id=38889015";

            // Get response

            var webResponse = await myRequest.GetResponseAsync();
            return _telegramImagePreparing.Prepare(webResponse.GetResponseStream(), webResponse.ContentLength);

        }
    }
}
