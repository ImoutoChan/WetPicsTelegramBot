using System;
using System.Threading.Tasks;
using PixivApi.Model;

namespace PixivApi.Services
{
    public class PixivApiProvider
    {
        private Authorize _currentTokenInfo;
        private DateTimeOffset _lastUpdated = DateTimeOffset.Now;

        public async Task<PixivApi> GetApiAsync(string username, string password, string clientId, string clientSecret)
        {
            if (_currentTokenInfo == null)
            {
                _currentTokenInfo = await PixivAuthorization.AuthorizeAsync(username, password, clientId, clientSecret);
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
    }
}
