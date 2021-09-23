using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PixivApi;
using PixivApi.Models;
using WetPicsTelegramBot.WebApp.Factories;
using WetPicsTelegramBot.WebApp.Models.Settings;
using PixivTopType = WetPicsTelegramBot.Data.Models.PixivTopType;

namespace WetPicsTelegramBot.WebApp.Repositories
{
    public class PixivRepository : IPixivRepository
    {
        private const string PixivTopCacheKey = "PixivTopCacheKey";

        private readonly IPixivApiClient          _pixivApiClient;
        private readonly AppSettings              _settings;
        private readonly ILogger<PixivRepository> _logger;
        private readonly IPolicesFactory          _policesFactory;
        private readonly IMemoryCache             _memoryCache;

        public PixivRepository(AppSettings settings,
            ILogger<PixivRepository> logger,
            IPolicesFactory policesFactory,
            IMemoryCache memoryCache,
            IPixivApiClient pixivApiClient)
        {
            _settings = settings;
            _logger = logger;
            _policesFactory = policesFactory;
            _memoryCache = memoryCache;
            _pixivApiClient = pixivApiClient;
        }

        public async Task<IReadOnlyCollection<PixivPostHeader>> GetPixivTop(
            PixivTopType type,
            int page = 1,
            int count = 100)
        {
            return await _memoryCache.GetOrCreateAsync(
                GetCacheKey(type, page, count),
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                    return GetPixivTopRemote(type, page, count);
                });
        }

        private static string GetCacheKey(PixivTopType type, int page, int count)
            => string.Join("_", PixivTopCacheKey, type.ToString(), page.ToString(), count.ToString());

        private async Task<IReadOnlyCollection<PixivPostHeader>> GetPixivTopRemote(
            PixivTopType type,
            int page,
            int count)
        {
            return await _pixivApiClient.LoadTop((PixivApi.Models.PixivTopType)(int)type);
        }

    }
}
