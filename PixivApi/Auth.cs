using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PixivApi.Objects;

namespace PixivApi
{
    public static class Auth
    {
        /// <summary>
        /// <para>Available parameters:</para>
        /// <para>- <c>string</c> username (required)</para>
        /// <para>- <c>string</c> password (required)</para>
        /// </summary>
        /// <returns>Tokens.</returns>
        public static async Task<Tokens> AuthorizeAsync(string username, string password, string clientId, string clientSecret)
        {
            var httpClient = BuildHttpClient();

            var param = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", username },
                { "password", password },
                { "grant_type", "password" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "device_token", "pixiv" },
                { "get_secure_url", "true" },
                { "include_policy", "true" },
            });

            var response = await httpClient.PostAsync("https://oauth.secure.pixiv.net/auth/token", param);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var authorize = JToken.Parse(json).SelectToken("response").ToObject<Authorize>();

            return new Tokens(authorize.AccessToken);
        }

        public static Tokens AuthorizeWithAccessToken(string accessToken)
        {
            return new Tokens(accessToken);
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
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PixivAndroidApp/5.0.115 (Android 6.0; PixivBot)");
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
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}