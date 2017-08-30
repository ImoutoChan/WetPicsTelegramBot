using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PixivApi;
using PixivApi.Objects;
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
            _timer = new Timer(TimerHandler, new object(), 0, _timerTriggerTime);
        }

        private async void TimerHandler(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                if (_pixivApi == null)
                {
                    _pixivApi = await Auth.AuthorizeAsync(_settings.PixivConfiguration.Login,
                                                          _settings.PixivConfiguration.Password,
                                                          _settings.PixivConfiguration.ClientId,
                                                          _settings.PixivConfiguration.ClientSecret);
                }

                foreach (var pixivSetting in _pixivSettings.Settings)
                {
                    if (!IsTimeToPost(pixivSetting.LastPostedTime, pixivSetting.MinutesInterval))
                        continue;

                    await PostNext(pixivSetting);
                    await _pixivSettings.UpdateLastPostedTime(pixivSetting);
                }
            }
            catch (NullReferenceException e)
            {
                _logger.LogError(e, "Pixiv auth exception");
                _pixivApi = null;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to process timer handler ");
            }
            finally
            {
                _timer.Change(_timerTriggerTime, _timerTriggerTime);
            }
        }

        private static bool IsTimeToPost(DateTimeOffset? lastPostedDateTime, int intervalMinutes)
        {
            return !lastPostedDateTime.HasValue 
                || (lastPostedDateTime.Value.LocalDateTime <= DateTimeOffset.Now.LocalDateTime.AddMinutes(-intervalMinutes));
        }

        private async Task PostNext(PixivSetting pixivSetting)
        {
            var imgs = await _pixivApi.GetRankingAllAsync(mode: pixivSetting.PixivTopType.GetEnumDescription(), page: 1, perPage: 100);

            foreach (var rankWork in imgs.SelectMany(x => x.Works).ToList())
            {
                if (rankWork.Work.Id == null 
                    || pixivSetting.PixivImagePostsSet.Contains((int) rankWork.Work.Id))
                    continue;

                await PostIllust(pixivSetting, rankWork.Work);
                await _pixivSettings.AddPosted(pixivSetting, (int)rankWork.Work.Id);
                break;
            }
        }

        private async Task<User> GetMe()
        {
            return _me ?? (_me = await _telegramApi.GetMeAsync());
        }

        private async Task PostIllust(PixivSetting pixivSetting, Work rankWork)
        {
            var imageUrl = rankWork.ImageUrls.Large;

            var content = await DownloadPixivStreamAsync(imageUrl);
            var caption = $"{rankWork.Title} © {rankWork.User.Name}";
            var mes = await _telegramApi.SendPhotoAsync(pixivSetting.ChatId, new FileToSend("name", content), caption);
            await _imageRepostService.PostToTargetIfExists(mes.Chat.Id, caption, mes.Photo.Last().FileId, (await GetMe()).Id);
            content.Dispose();
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
            
            return (await myRequest.GetResponseAsync()).GetResponseStream();
        }
    }
}
