using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PixivApi.Model;

namespace PixivApi.Services
{
    public class PixivApiProvider
    {
        private readonly IMemoryCache _memoryCache;

        private Authorize _currentTokenInfo;
        private DateTimeOffset _lastUpdated = DateTimeOffset.Now;

        public PixivApiProvider(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<PixivApi> GetApiAsync(
            string accessToken, 
            string refreshToken, 
            string clientId, 
            string clientSecret)
        {
            if (_currentTokenInfo == null)
            {
                _currentTokenInfo = await PixivAuthorization.AuthorizeAsync(_memoryCache, accessToken, refreshToken);
                _lastUpdated = DateTimeOffset.Now;

                return new PixivApi(_currentTokenInfo.AccessToken);
            }

            if (_currentTokenInfo?.ExpiresIn != null
                && _lastUpdated.AddSeconds(_currentTokenInfo.ExpiresIn.Value - 60) > DateTimeOffset.Now)
            {
                return new PixivApi(_currentTokenInfo.AccessToken);
            }

            try
            {
                _currentTokenInfo = await PixivAuthorization.RefreshTokenAsync(
                    _currentTokenInfo.RefreshToken,
                    clientId,
                    clientSecret);
                
                _memoryCache.Set(PixivApiCredentialsCacheKeys.AccessToken, _currentTokenInfo.AccessToken);
                _memoryCache.Set(PixivApiCredentialsCacheKeys.RefreshToken, _currentTokenInfo.RefreshToken);
                _memoryCache.Set(PixivApiCredentialsCacheKeys.ExpireToken, _currentTokenInfo.ExpiresIn);
                
                return new PixivApi(_currentTokenInfo.AccessToken);
            }
            catch
            {
                _currentTokenInfo = null;
                throw;
            }
        }

        public void ForceReAuth()
        {
            _currentTokenInfo = null;
        }

        public void ForceRefresh()
        {
            _lastUpdated = DateTimeOffset.Now.AddDays(-1);
        }
    }
}
