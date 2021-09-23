using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixivApi.Models;

namespace PixivApi
{
    /// <remarks>
    /// Designed as singleton
    /// </remarks>>
    public class PixivAuthorization : IPixivAuthorization
    {
        private readonly PixivConfiguration _pixivConfiguration;
        private readonly IMemoryCache _memoryCache;

        public PixivAuthorization(IOptions<PixivConfiguration> pixivConfiguration, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _pixivConfiguration = pixivConfiguration.Value;
        }

        public async Task<string> GetAccessToken()
        {
            var validAccessToken = _memoryCache.Get<string>(PixivApiCredentialsCacheKeys.AccessToken);

            if (validAccessToken != null)
            {
                return validAccessToken;
            }

            var validRefreshToken = _memoryCache.Get<string>(PixivApiCredentialsCacheKeys.RefreshToken);

            validRefreshToken ??= _pixivConfiguration.RefreshToken;

            var when = DateTimeOffset.Now;
            var (accessToken, refreshToken, expiresIn) = await RefreshTokenAsync(
                validRefreshToken,
                _pixivConfiguration.ClientId,
                _pixivConfiguration.ClientSecret);

            await SaveTokens(accessToken, when, expiresIn, refreshToken);

            return accessToken;
        }

        public void ResetAccessToken() => _memoryCache.Remove(PixivApiCredentialsCacheKeys.AccessToken);

        private async Task SaveTokens(string? accessToken, DateTimeOffset when, int expiresIn, string? refreshToken)
        {
            _memoryCache.Set(
                PixivApiCredentialsCacheKeys.AccessToken,
                accessToken,
                when.AddSeconds(expiresIn));
            _memoryCache.Set(PixivApiCredentialsCacheKeys.RefreshToken, refreshToken);

            var configuration = new
            {
                Configuration = new
                {
                    PixivConfiguration = new
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    }
                }
            };

            const string cacheFileName = "appsettings.Cache.json";
            const string backupCacheFileName = cacheFileName + ".backup";

            if (File.Exists(backupCacheFileName))
                File.Delete(backupCacheFileName);

            if (File.Exists(cacheFileName))
                File.Move(cacheFileName, backupCacheFileName);

            await File.WriteAllTextAsync("appsettings.Cache.json", JsonConvert.SerializeObject(configuration));

            if (File.Exists(backupCacheFileName))
                File.Delete(backupCacheFileName);
        }

        private static async Task<PixivApiAuthInfo> RefreshTokenAsync(
            string refreshToken,
            string clientId,
            string clientSecret)
        {
            var httpClient = BuildHttpClient();

            var param = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    {"refresh_token", refreshToken},
                    {"grant_type", "refresh_token"},
                    {"client_id", clientId},
                    {"client_secret", clientSecret},
                    {"device_token", "pixiv"},
                    {"get_secure_url", "true"},
                    {"include_policy", "true"}
                }!);

            var response = await httpClient.PostAsync("https://oauth.secure.pixiv.net/auth/token", param);
            await EnsureSuccessStatusCode(response);

            var json = await response.Content.ReadAsStringAsync();

            var responsePart = JToken.Parse(json).SelectToken("response");

            return new PixivApiAuthInfo(
                responsePart.SelectToken("access_token").Value<string>(),
                responsePart.SelectToken("refresh_token").Value<string>(),
                responsePart.SelectToken("expires_in").Value<int>());
        }

        private static async Task EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            if ((int)response.StatusCode < 300)
            {
                return;
            }

            try
            {
                var content = await response.Content.ReadAsStringAsync();

                var errorCode = JToken.Parse(content).SelectToken("errors").SelectToken("system").SelectToken("code").Value<int>();

                const int invalidRefreshTokenErrorCode = 1508;

                if (errorCode == invalidRefreshTokenErrorCode)
                {
                    throw new InvalidRefreshTokenException();
                }
            }
            catch (InvalidRefreshTokenException)
            {
                throw;
            }
            catch
            {
                response.EnsureSuccessStatusCode();
            }
        }

        private static HttpClient BuildHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var currentDateTime = DateTimeOffset.Now;
            var usDateTime = currentDateTime.ToOffset(TimeSpan.FromHours(2));
            var dateTimeString = usDateTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
            const string salt = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";

            var hash = CalculateMd5(dateTimeString + salt);

            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("Referer", "http://www.pixiv.net/");
            httpClient.DefaultRequestHeaders.Add("User-Agent", $"PixivAndroidApp/5.0.{115} (Android 6.0; PixivBot)");
            httpClient.DefaultRequestHeaders.Add("accept-language", "en_US");
            httpClient.DefaultRequestHeaders.Add("app-os", "android");
            httpClient.DefaultRequestHeaders.Add("app-os-version", "5.0.156");
            httpClient.DefaultRequestHeaders.Add("x-client-time", dateTimeString);
            httpClient.DefaultRequestHeaders.Add("x-client-hash", hash);
            httpClient.DefaultRequestHeaders.Add("accept-encoding", "gzip");
            return httpClient;
        }

        private static string CalculateMd5(string input)
        {
            using var md5 = MD5.Create();

            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            return string.Join("", hashBytes.Select(x => x.ToString("X2"))).ToLower();
        }
    }

    internal class InvalidRefreshTokenException : Exception
    {
    }
}
