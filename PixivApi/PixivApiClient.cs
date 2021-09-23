using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PixivApi.Dto;
using PixivApi.Models;

namespace PixivApi
{
    public class PixivApiClient : IPixivApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<PixivConfiguration> _configuration;
        private readonly IPixivAuthorization _pixivAuthorization;

        public PixivApiClient(
            HttpClient httpClient,
            IOptions<PixivConfiguration> configuration,
            IPixivAuthorization pixivAuthorization)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _pixivAuthorization = pixivAuthorization;
        }

        public async Task<IReadOnlyCollection<PixivPostHeader>> LoadTop(PixivTopType topType, int page = 1, int count = 100)
        {
            const string url = "https://app-api.pixiv.net/v1/illust/ranking";

            var param = new Dictionary<string, string>
            {
                {"mode", topType.GetEnumDescription()},
                {"filter", "for_ios"},
            };

            var responseContent = await GetAsync(url, param);
            var illustrations = JToken.Parse(responseContent)
                ?.SelectToken("illusts")
                ?.ToObject<IReadOnlyCollection<Illustration>>();

            if (illustrations == null)
                throw new Exception($"Unexpected pixiv response: {responseContent}");

            return illustrations
                .Select(x => new PixivPostHeader((int)x.Id, x.ImageUrls!.Large!, x.Title!, x.User!.Name!))
                .ToList();
        }

        private async Task<string> GetAsync(
            string url,
            IDictionary<string, string> param,
            bool isRetry = false)
        {
            var token = await _pixivAuthorization.GetAccessToken();

            url += "?" + string.Join("&", param.Select(x => x.Key + "=" + WebUtility.UrlEncode(x.Value)));

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            //request.Headers.Referrer = new Uri("https://app-api.pixiv.net");

            request.Headers.UserAgent.Clear();
            request.Headers.TryAddWithoutValidation("app-os", "ios");
            request.Headers.TryAddWithoutValidation("app-os-version", "14.6");
            request.Headers.TryAddWithoutValidation("User-Agent", "PixivIOSApp/7.13.3 (iOS 14.6; iPhone13,2)");

            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer " + token);

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch when(!isRetry)
            {
                _pixivAuthorization.ResetAccessToken();
                return await GetAsync(url, param, true);
            }
        }

        public async Task<MeasuredStream> DownloadImage(string imageUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);

            request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.0; zh-CN; rv:1.9.0.6) Gecko/2009011913 Firefox/3.0.6");
            request.Headers.TryAddWithoutValidation("Accept-Language", "zh-cn,zh;q=0.7,ja;q=0.3");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip,deflate");
            request.Headers.TryAddWithoutValidation("Accept-Charset", "gb18030,utf-8;q=0.7,*;q=0.7");
            request.Headers.TryAddWithoutValidation("referer", "https://app-api.pixiv.net/");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var length = response.Content.Headers.ContentLength;
            if (!length.HasValue)
            {
                throw new Exception("Unexpected length");
            }

            return new (await response.Content.ReadAsStreamAsync(), length.Value);
        }
    }
}
