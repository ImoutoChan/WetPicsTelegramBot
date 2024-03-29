﻿using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace WetPicsTelegramBot.Tests
{
    public partial class PixivApiTests
    {
        [Fact]
        public async Task TestAuth()
        {
            var cache = new MemoryCache(new MemoryDistributedCacheOptions {SizeLimit = null});

            // var tokenProvider = new PixivApiProvider(cache);
            // var api = await tokenProvider.GetApiAsync(
            //     AccessToken,
            //     RefreshToken,
            //     ClientId,
            //     ClientSecret);
            // tokenProvider.ForceReAuth();
            // api = await tokenProvider.GetApiAsync(
            //     AccessToken,
            //     RefreshToken,
            //     ClientId,
            //     ClientSecret);
            // var latest = await api.GetLatestWorksAsync();
            //
            // Assert.NotNull(latest);
        }

        [Fact]
        public async Task TestRefresh()
        {
            var cache = new MemoryCache(new MemoryDistributedCacheOptions());
            //
            // var tokenProvider = new PixivApiProvider(cache);
            // var api = await tokenProvider.GetApiAsync(
            //     AccessToken,
            //     RefreshToken,
            //     ClientId,
            //     ClientSecret);
            // tokenProvider.ForceRefresh();
            // api = await tokenProvider.GetApiAsync(
            //     AccessToken,
            //     RefreshToken,
            //     ClientId,
            //     ClientSecret);
            // var latest = await api.GetLatestWorksAsync();
            //
            // Assert.NotNull(latest);
        }
    }
}
