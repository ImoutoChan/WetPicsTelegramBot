using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PixivApi;
using PixivApi.Model;
using PixivApi.Services;
using Polly;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Factories;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models.Settings;

namespace WetPicsTelegramBot.WebApp.Repositories
{
    public class PixivRepository : IPixivRepository
    {
        private const string PixivTopCacheKey = "PixivTopCacheKey";

        private readonly PixivApiProvider         _pixivApiProvider;
        private readonly AppSettings              _settings;
        private readonly ILogger<PixivRepository> _logger;
        private readonly IPolicesFactory          _policesFactory;
        private readonly IMemoryCache             _memoryCache;

        public PixivRepository(AppSettings settings, 
            ILogger<PixivRepository> logger,
            IPolicesFactory policesFactory,
            IMemoryCache memoryCache,
            PixivApiProvider pixivApiProvider)
        {
            _settings = settings;
            _logger = logger;
            _policesFactory = policesFactory;
            _memoryCache = memoryCache;
            _pixivApiProvider = pixivApiProvider;
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
            => string.Join("_", PixivTopCacheKey, type.GetEnumDescription(), page.ToString(), count.ToString());

        private async Task<Paginated<Rank>> GetPixivTopRemote(
            PixivTopType type, 
            int page, 
            int count)
        {
            var policy = Policy.WrapAsync(
                Policy
                    .Handle<NullReferenceException>()
                    .RetryAsync(1, (exception, i) => OnAuthRetry(exception, i)),
                _policesFactory.GetDefaultHttpRetryPolicy());

            return await policy.ExecuteAsync(async () =>
            {
                var api = await Authorize();
                return await api.GetRankingAllAsync(type.GetEnumDescription(), page, count);
            });
        }

        private void OnAuthRetry(Exception exception, int retryCount)
        {
            _pixivApiProvider.ForceReAuth();
            _logger.LogError(exception, "Pixiv auth error");
        }

        private Task<PixivApi.Services.PixivApi> Authorize()
            => _pixivApiProvider.GetApiAsync(
                _settings.PixivConfiguration.Login,
                _settings.PixivConfiguration.Password,
                _settings.PixivConfiguration.ClientId,
                _settings.PixivConfiguration.ClientSecret);
    }
}