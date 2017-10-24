using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PixivApi;
using PixivApi.Objects;
using SixLabors.ImageSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Database.Model;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Services.Abstract;
using User = Telegram.Bot.Types.User;

namespace WetPicsTelegramBot.Services
{
    class PixivService
    {
        private static readonly int _timerTriggerTime = 1 * 60 * 1000;
        private static readonly int _photoSizeLimit = 1024 * 1024 * 5;

        private readonly AppSettings _settings;
        private readonly IPixivSettingsService _pixivSettings;
        private readonly ITelegramBotClient _telegramApi;
        private readonly ILogger<PixivService> _logger;
        private readonly IImageRepostService _imageRepostService;
        private Tokens _pixivApi;
        private Timer _timer;
        private User _me;

        public PixivService(AppSettings settings,
                            IPixivSettingsService pixivSettings,
                            ITelegramBotClient telegramApi,
                            ILogger<PixivService> logger,
                            IImageRepostService imageRepostService)
        {
            _settings = settings;
            _pixivSettings = pixivSettings;
            _telegramApi = telegramApi;
            _logger = logger;
            _imageRepostService = imageRepostService;

            RunTimer();
        }

        private void RunTimer()
        {
            _timer = new Timer(TimerHandler, null, 0, _timerTriggerTime);
        }

        private async void TimerHandler(object state)
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            
            _logger.LogTrace("Timer elapsed");
            try
            {
                await GetPixivApi();
                
                _logger.LogDebug($"The number of active pixiv settings: {_pixivSettings.Settings.Count}");
                foreach (var pixivSetting in _pixivSettings.Settings)
                {
                    if (!IsTimeToPost(pixivSetting))
                        continue;

                    _logger.LogDebug($"It's time to post | Chat id: {pixivSetting.ChatId}");

                    await PostNext(pixivSetting);

                    _logger.LogInformation($"Illust posting is finished | Chat id: {pixivSetting.ChatId}");

                    await _pixivSettings.UpdateLastPostedTime(pixivSetting);

                    _logger.LogDebug($"LastPostedTime updated | Chat id: {pixivSetting.ChatId}");
                }
            }
            catch (NullReferenceException e)
            {
                _logger.LogError(e, "Pixiv auth exception");
                _pixivApi = null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred in pixiv timer handler");
            }
            finally
            {
                _timer.Change(_timerTriggerTime, _timerTriggerTime);
            }
        }

        private async Task<Tokens> GetPixivApi()
        {
            return _pixivApi ?? (_pixivApi = await Auth.AuthorizeAsync(_settings.PixivConfiguration.Login,
                                                                       _settings.PixivConfiguration.Password,
                                                                       _settings.PixivConfiguration.ClientId,
                                                                       _settings.PixivConfiguration.ClientSecret));
        }

        private static bool IsTimeToPost(PixivSetting pixivSetting)
        {
            return !pixivSetting.LastPostedTime.HasValue 
                || (pixivSetting.LastPostedTime.Value.LocalDateTime <= DateTimeOffset.Now.LocalDateTime.AddMinutes(-pixivSetting.MinutesInterval));
        }

        private async Task PostNext(PixivSetting pixivSetting)
        {
            _logger.LogTrace("Requesting pixivApi");
            var pixivApi = await GetPixivApi();

            _logger.LogTrace("Loading pixiv illust list");
            var imgs = await pixivApi.GetRankingAllAsync(mode: pixivSetting.PixivTopType.GetEnumDescription(), page: 1, perPage: 100);

            _logger.LogTrace("Selecting new one");
            var newIllust = imgs
                .SelectMany(x => x.Works)
                .FirstOrDefault(x => x.Work.Id != null && !pixivSetting.PixivImagePostsSet.Contains((int) x.Work.Id));

            if (newIllust == null)
            {
                _logger.LogDebug("There is no new illusts");
                return;
            }

            _logger.LogDebug($"Posting illust | IllustId: {newIllust.Work.Id}");
            await PostIllust(pixivSetting, newIllust.Work);

            _logger.LogDebug($"Adding posted illust | IllustId: {newIllust.Work.Id}");
            await _pixivSettings.AddPosted(pixivSetting, (int)newIllust.Work.Id);
        }

        private async Task<User> GetMe()
        {
            return _me ?? (_me = await _telegramApi.GetMeAsync());
        }

        private async Task PostIllust(PixivSetting pixivSetting, Work rankWork)
        {
            var imageUrl = rankWork.ImageUrls.Large;
            _logger.LogDebug($"Illust url: {imageUrl}");

            _logger.LogTrace($"Downloading stream");
            using (var content = await DownloadPixivStreamAsync(imageUrl))
            {
                var caption = $"{rankWork.Title} © {rankWork.User.Name}";
                _logger.LogDebug($"Caption: {caption}");

                _logger.LogTrace($"Sending image to chat");
                var mes = await _telegramApi.SendPhotoAsync(pixivSetting.ChatId, new FileToSend("name", content), caption);

                _logger.LogTrace($"Reposting image to channel");
                await _imageRepostService.PostToTargetIfExists(mes.Chat.Id, caption, mes.Photo.Last().FileId, (await GetMe()).Id);
            }
        }

        private async Task<Stream> DownloadPixivStreamAsync(string image)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(image);

            myRequest.CookieContainer = new CookieContainer();

            myRequest.Method = "GET";
            myRequest.Headers = new WebHeaderCollection();
            myRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            myRequest.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.0; zh-CN; rv:1.9.0.6) Gecko/2009011913 Firefox/3.0.6";

            myRequest.Headers["Accept-Language"] = "zh-cn,zh;q=0.7,ja;q=0.3";
            myRequest.Headers["Accept-Encoding"] = "gzip,deflate";
            myRequest.Headers["Accept-Charset"] = "gb18030,utf-8;q=0.7,*;q=0.7";
            myRequest.Headers["referer"] = "http://www.pixiv.net/member_illust.php?mode=manga&illust_id=38889015";

            // Get response

            var webResponse = await myRequest.GetResponseAsync();
            
            return webResponse.ContentLength >= _photoSizeLimit
                ? await Task.Run(() => Resize(webResponse.GetResponseStream()))
                : webResponse.GetResponseStream();
        }

        private Stream Resize(Stream stream)
        {
            var image = Image.Load(stream);
            stream.Dispose();
            var outStream = new MemoryStream();

            image.Mutate(x => x
                .Resize((int)(image.Width * 0.9), (int)(image.Height * 0.9)));

            image.SaveAsJpeg(outStream);
            outStream.Seek(0, SeekOrigin.Begin);

            return outStream.Length >= _photoSizeLimit 
                ? Resize(outStream) 
                : outStream;
        }
    }
}
