using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PixivApi;
using PixivApi.Objects;
using Polly;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Factories;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models.Settings;

namespace WetPicsTelegramBot.WebApp.Repositories
{
    public class PixivRepository : IPixivRepository
    {
        private const string _pixivTopCacheKey = "PixivTopCacheKey";

        private          Tokens                   _pixivApi;
        private readonly AppSettings              _settings;
        private readonly ILogger<PixivRepository> _logger;
        private readonly IPolicesFactory          _policesFactory;
        private readonly IMemoryCache             _memoryCache;

        public PixivRepository(AppSettings settings, 
            ILogger<PixivRepository> logger,
            IPolicesFactory policesFactory,
            IMemoryCache memoryCache)
        {
            _settings = settings;
            _logger = logger;
            _policesFactory = policesFactory;
            _memoryCache = memoryCache;
        }

        public async Task<Paginated<Rank>> GetPixivTop(
            PixivTopType type,
            int page = 1,
            int count = 100)
        {
            return await _memoryCache.GetOrCreateAsync(
                GetCacheKey(type, page, count),
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return GetPixivTopRemote(type, page, count);
                });
        }

        private static string GetCacheKey(PixivTopType type, int page, int count) 
            => string.Join("_", _pixivTopCacheKey, type.GetEnumDescription(), page.ToString(), count.ToString());

        private async Task<Paginated<Rank>> GetPixivTopRemote(
            PixivTopType type, 
            int page, 
            int count)
        {
            await TouchPixivApi();

            var policy =
                Policy
                   .Handle<NullReferenceException>()
                   .RetryAsync(1, OnAuthRetry)
                   .WrapAsync(_policesFactory.GetDefaultHttpRetryPolicy());

            return await policy.ExecuteAsync(()
                => _pixivApi.GetRankingAllAsync(type.GetEnumDescription(), page, count));
        }

        private async Task TouchPixivApi()
        {
            if (_pixivApi == null)
                _pixivApi = await Authorize();
        }

        private async Task OnAuthRetry(Exception exception, int retryCount)
        {
            _pixivApi = await Authorize();
            _logger.LogError(exception, "Pixiv auth error");
        }

        private Task<Tokens> Authorize()
            => Auth.AuthorizeAsync(
                _settings.PixivConfiguration.Login,
                _settings.PixivConfiguration.Password,
                _settings.PixivConfiguration.ClientId,
                _settings.PixivConfiguration.ClientSecret);
    }
}