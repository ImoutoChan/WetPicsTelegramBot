using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using PixivApi;
using PixivApi.Objects;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

using Telegram.Bot.Types.InputFiles;

using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class PixivService : IPixivService
    {
        private static readonly int _photoSizeLimit = 1024 * 1024 * 5;
        private static readonly int _photoHeightLimit = 1024 * 5 - 1;
        private static readonly int _photoWidthLimit = 1024 * 5 - 1;

        private readonly AppSettings _settings;
        private readonly IPixivSettingsService _pixivSettings;
        private readonly ITgClient _tgClient;
        private readonly ILogger<PixivService> _logger;
        private readonly IRepostService _repostService;
        private Tokens _pixivApi;

        public PixivService(AppSettings settings,
                            IPixivSettingsService pixivSettings,
                            ITgClient tgClient,
                            ILogger<PixivService> logger,
                            IRepostService repostService)
        {
            _settings = settings;
            _pixivSettings = pixivSettings;
            _tgClient = tgClient;
            _logger = logger;
            _repostService = repostService;
        }

        public async Task TriggerPostNext()
        {
            _logger.LogTrace("Pixiv PostNext triggered");

            try
            {
                await GetPixivApi();

                var settings = await _pixivSettings.GetSettingsAsync();

                _logger.LogDebug($"The number of active pixiv settings: {settings.Count}");

                foreach (var pixivSetting in settings)
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
                _logger.LogMethodError(e);
            }
        }

        private async Task<Tokens> GetPixivApi()
        {
            return _pixivApi ?? (_pixivApi = await Auth.AuthorizeAsync(_settings.PixivConfiguration.Login, _settings.PixivConfiguration.Password, _settings.PixivConfiguration.ClientId, _settings.PixivConfiguration.ClientSecret));
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
            var works = (await pixivApi
                          .GetRankingAllAsync(mode: pixivSetting.PixivTopType.GetEnumDescription(),
                                              page: 1,
                                              perPage: 100))
                      .SelectMany(x => x.Works)
                      .Select(x => x.Work)
                      .Where(x => x.Id != null)
                      .ToList();




            _logger.LogTrace("Selecting new one");
            var next = await _pixivSettings
                               .GetFirstUnpostedAsync(pixivSetting.Id,
                                                      works.Select(x => (int)x.Id.Value).ToArray());
            if (next == null)
            {
                _logger.LogDebug("There is no new illusts");
                return;
            }

            var work = works.First(x => x.Id == next);

            _logger.LogDebug($"Posting illust | IllustId: {work.Id}");
            await PostIllust(pixivSetting, work);

            _logger.LogDebug($"Adding posted illust | IllustId: {work.Id}");
            await _pixivSettings.AddPosted(pixivSetting, (int)work.Id);
        }
        
        private async Task PostIllust(PixivSetting pixivSetting, 
                                      Work rankWork)
        {
            var imageUrl = rankWork.ImageUrls.Large;
            _logger.LogDebug($"Illust url: {imageUrl}");

            _logger.LogTrace($"Downloading stream");
            using (var content = await DownloadPixivStreamAsync(imageUrl))
            {
                var caption = $"{rankWork.Title} © {rankWork.User.Name}";
                _logger.LogDebug($"Caption: {caption}");

                _logger.LogTrace($"Sending image to chat");
                var sendedMessage = await _tgClient.Client.SendPhotoAsync(pixivSetting.ChatId, 
                                                                new InputOnlineFile(content), 
                                                                caption);

                _logger.LogTrace($"Reposting image to channel");

                var target = await _repostService.TryGetRepostTargetChat(sendedMessage.Chat.Id);
                if (target == null)
                    return;
                
                await _repostService.RepostWithLikes(sendedMessage, target, caption);
            }
        }

        private async Task<Stream> DownloadPixivStreamAsync(string image)
        {
            var myRequest = (HttpWebRequest)WebRequest.Create(image);

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
            
            _logger.LogTrace($"ContentLength: {webResponse.ContentLength} | SizeLimit: {_photoSizeLimit}");
            if (webResponse.ContentLength >= _photoSizeLimit)
            {
                _logger.LogTrace($"Resize and rescale");
                var resizedStream = await Task.Run(() => Resize(webResponse.GetResponseStream(), true));
                _logger.LogTrace($"New ContentLength: {resizedStream.Length} | SizeLimit: {_photoSizeLimit}");

                return resizedStream;
            }
            else
            {
                _logger.LogTrace($"Rescale");
                var resizedStream = await Task.Run(() => Resize(webResponse.GetResponseStream()));
                _logger.LogTrace($"New ContentLength: {resizedStream.Length} | SizeLimit: {_photoSizeLimit}");

                return resizedStream;
            }
        }

        private Stream Resize(Stream stream, bool resize = false)
        {
            using (var image = Image.Load(stream))
            {
                stream.Dispose();

                if (image.Height - _photoHeightLimit > 0 || image.Width - _photoWidthLimit > 0)
                {
                    double ratioH = _photoHeightLimit / (double) image.Height;
                    double ratioW = _photoWidthLimit / (double) image.Width;

                    ratioH = ratioH >= 1 ? Double.MinValue : ratioH;
                    ratioW = ratioW >= 1 ? Double.MinValue : ratioW;

                    var minRatio = ratioW < ratioH ? ratioW : ratioH;

                    image.Mutate(x => x.Resize((int) (image.Width * minRatio), (int) (image.Height * minRatio)));
                }
                else if (resize)
                {
                    image.Mutate(x => x.Resize((int) (image.Width * 0.9), (int) (image.Height * 0.9)));
                }

                var outStream = new MemoryStream();


                image.SaveAsJpeg(outStream, new JpegEncoder {Quality = 95});
                outStream.Seek(0, SeekOrigin.Begin);

                return outStream.Length >= _photoSizeLimit
                    ? Resize(outStream, true)
                    : outStream;
            }
        }
    }
}
