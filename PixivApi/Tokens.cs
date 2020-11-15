﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PixivApi.Model;

namespace PixivApi.Services
{
    public class Tokens
    {
        internal Tokens(string accessToken)
        {
            AccessToken = accessToken;
        }

        private string AccessToken { get; }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>MethodType</c> type (required) [ GET, POST ]</para>
        ///     <para>- <c>string</c> url (required)</para>
        ///     <para>- <c>IDictionary</c> param (required)</para>
        ///     <para>- <c>IDictionary</c> header (optional)</para>
        /// </summary>
        /// <returns>AsyncResponse.</returns>
        private async Task<AsyncResponse> SendRequestAsync(
            MethodType type,
            string url,
            IDictionary<string, string> param,
            IDictionary<string, string> headers = null)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Referer", "http://spapi.pixiv.net/");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PixivIOSApp/5.8.0");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);

            if (headers != null)
                foreach (var header in headers)
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);

            AsyncResponse asyncResponse;

            if (type == MethodType.Post)
            {
                var reqParam = new FormUrlEncodedContent(param);
                var response = await httpClient.PostAsync(url, reqParam);
                asyncResponse = new AsyncResponse(response);
            }
            else if (type == MethodType.Delete)
            {
                var uri = url;

                if (param != null)
                {
                    var queryString = "";
                    foreach (var kvp in param)
                    {
                        if (queryString == "")
                            queryString += "?";
                        else
                            queryString += "&";

                        queryString += kvp.Key + "=" + WebUtility.UrlEncode(kvp.Value);
                    }

                    uri += queryString;
                }

                var response = await httpClient.DeleteAsync(uri);
                asyncResponse = new AsyncResponse(response);
            }
            else
            {
                var uri = url;

                if (param != null)
                {
                    var queryString = "";
                    foreach (var kvp in param)
                    {
                        if (queryString == "")
                            queryString += "?";
                        else
                            queryString += "&";

                        queryString += kvp.Key + "=" + WebUtility.UrlEncode(kvp.Value);
                    }

                    uri += queryString;
                }

                var response = await httpClient.GetAsync(uri);
                asyncResponse = new AsyncResponse(response);
            }

            return asyncResponse;
        }

        private async Task<T> AccessApiAsync<T>(
            MethodType type,
            string url,
            IDictionary<string, string> param,
            IDictionary<string, string> headers = null) where T : class
        {
            using (var response = await SendRequestAsync(type, url, param, headers))
            {
                var json = await response.GetResponseStringAsync();
                var obj = JToken.Parse(json).SelectToken("response").ToObject<T>();

                if (obj is IPagenated)
                    ((IPagenated) obj).Pagination = JToken.Parse(json).SelectToken("pagination").ToObject<Pagination>();

                return obj;
            }
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>long</c> illustId (required)</para>
        /// </summary>
        /// <returns>Works.</returns>
        public async Task<List<Work>> GetWorksAsync(long illustId)
        {
            var url = "https://public-api.secure.pixiv.net/v1/works/" + illustId + ".json";

            var param = new Dictionary<string, string>
            {
                {"profile_image_sizes", "px_170x170,px_50x50"},
                {"image_sizes", "px_128x128,small,medium,large,px_480mw"},
                {"include_stats", "true"}
            };

            return await AccessApiAsync<List<Work>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>long</c> authorId (required)</para>
        /// </summary>
        /// <returns>Users.</returns>
        public async Task<List<User>> GetUsersAsync(long authorId)
        {
            var url = "https://public-api.secure.pixiv.net/v1/users/" + authorId + ".json";

            var param = new Dictionary<string, string>
            {
                {"profile_image_sizes", "px_170x170,px_50x50"},
                {"image_sizes", "px_128x128,small,medium,large,px_480mw"},
                {"include_stats", "1"},
                {"include_profile", "1"},
                {"include_workspace", "1"},
                {"include_contacts", "1"}
            };

            return await AccessApiAsync<List<User>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>long</c> maxId (optional)</para>
        ///     <para>- <c>bool</c> showR18 (optional)</para>
        /// </summary>
        /// <returns>Feeds.</returns>
        public async Task<List<Feed>> GetMyFeedsAsync(long maxId = 0, bool showR18 = true)
        {
            var url = "https://public-api.secure.pixiv.net/v1/me/feeds.json";

            var param = new Dictionary<string, string>
            {
                {"relation", "all"},
                {"type", "touch_nottext"},
                {"show_r18", Convert.ToInt32(showR18).ToString()}
            };

            if (maxId != 0)
                param.Add("max_id", maxId.ToString());

            return await AccessApiAsync<List<Feed>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>int</c> page (optional)</para>
        ///     <para>- <c>int</c> perPage (optional)</para>
        ///     <para>- <c>string</c> publicity (optional) [ public, private ]</para>
        ///     <para>- <c>bool</c> includeSanityLevel (optional)</para>
        /// </summary>
        /// <returns>UsersFavoriteWorks. (Pagenated)</returns>
        public async Task<Paginated<UsersFavoriteWork>> GetMyFavoriteWorksAsync(
            int page = 1,
            int perPage = 30,
            string publicity = "public",
            bool includeSanityLevel = true)
        {
            var url = "https://public-api.secure.pixiv.net/v1/me/favorite_works.json";

            var param = new Dictionary<string, string>
            {
                {"page", page.ToString()},
                {"per_page", perPage.ToString()},
                {"publicity", publicity},
                {"include_stats", "1"},
                {"include_sanity_level", Convert.ToInt32(includeSanityLevel).ToString()},
                {"image_sizes", "px_128x128,small,medium,large,px_480mw"},
                {"profile_image_sizes", "px_170x170,px_50x50"}
            };

            return await AccessApiAsync<Paginated<UsersFavoriteWork>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>long</c> workID (required)</para>
        ///     <para>- <c>string</c> publicity (optional) [ public, private ]</para>
        /// </summary>
        /// <returns>UsersWorks. (Pagenated)</returns>
        public async Task<List<UsersFavoriteWork>> AddMyFavoriteWorksAsync(
            long workId,
            string comment = "",
            IEnumerable<string> tags = null,
            string publicity = "public")
        {
            var url = "https://public-api.secure.pixiv.net/v1/me/favorite_works.json";

            var param = new Dictionary<string, string>
            {
                {"work_id", workId.ToString()},
                {"publicity", publicity},
                {"comment", comment}
            };

            if (tags != null)
                param.Add("tags", string.Join(",", tags));

            return await AccessApiAsync<List<UsersFavoriteWork>>(MethodType.Post, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>IEnumerable</c> workIds (required)</para>
        ///     <para>- <c>string</c> publicity (optional) [ public, private ]</para>
        /// </summary>
        /// <returns>UsersWorks. (Pagenated)</returns>
        public async Task<List<UsersFavoriteWork>> DeleteMyFavoriteWorksAsync(
            IEnumerable<long> workIds,
            string publicity = "public")
        {
            var url = "https://public-api.secure.pixiv.net/v1/me/favorite_works.json";

            var param = new Dictionary<string, string>
            {
                {"work_id", string.Join(",", workIds.Select(x => x.ToString()))},
                {"publicity", publicity}
            };

            return await AccessApiAsync<List<UsersFavoriteWork>>(MethodType.Delete, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>long</c> workId (required)</para>
        ///     <para>- <c>string</c> publicity (optional) [ public, private ]</para>
        /// </summary>
        /// <returns>UsersWorks. (Pagenated)</returns>
        public async Task<Paginated<UsersFavoriteWork>> DeleteMyFavoriteWorksAsync(
            long workId,
            string publicity = "public")
        {
            var url = "https://public-api.secure.pixiv.net/v1/me/favorite_works.json";

            var param = new Dictionary<string, string>
            {
                {"work_id", workId.ToString()},
                {"publicity", publicity}
            };

            return await AccessApiAsync<Paginated<UsersFavoriteWork>>(MethodType.Delete, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>long</c> authorId (required)</para>
        ///     <para>- <c>int</c> page (optional)</para>
        ///     <para>- <c>int</c> perPage (optional)</para>
        ///     <para>- <c>string</c> publicity (optional) [ public, private ]</para>
        ///     <para>- <c>bool</c> includeSanityLevel (optional)</para>
        /// </summary>
        /// <returns>UsersWorks. (Pagenated)</returns>
        public async Task<Paginated<UsersWork>> GetMyFollowingWorksAsync(
            int page = 1,
            int perPage = 30,
            string publicity = "public",
            bool includeSanityLevel = true)
        {
            var url = "https://public-api.secure.pixiv.net/v1/me/following/works.json";

            var param = new Dictionary<string, string>
            {
                {"page", page.ToString()},
                {"per_page", perPage.ToString()},
                {"publicity", publicity},
                {"include_stats", "1"},
                {"include_sanity_level", Convert.ToInt32(includeSanityLevel).ToString()},
                {"image_sizes", "px_128x128,small,medium,large,px_480mw"},
                {"profile_image_sizes", "px_170x170,px_50x50"}
            };

            return await AccessApiAsync<Paginated<UsersWork>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>long</c> authorId (required)</para>
        ///     <para>- <c>int</c> page (optional)</para>
        ///     <para>- <c>int</c> perPage (optional)</para>
        ///     <para>- <c>string</c> publicity (optional) [ public, private ]</para>
        ///     <para>- <c>bool</c> includeSanityLevel (optional)</para>
        /// </summary>
        /// <returns>UsersWorks. (Pagenated)</returns>
        public async Task<Paginated<UsersWork>> GetUsersWorksAsync(
            long authorId,
            int page = 1,
            int perPage = 30,
            string publicity = "public",
            bool includeSanityLevel = true)
        {
            var url = "https://public-api.secure.pixiv.net/v1/users/" + authorId + "/works.json";

            var param = new Dictionary<string, string>
            {
                {"page", page.ToString()},
                {"per_page", perPage.ToString()},
                {"publicity", publicity},
                {"include_stats", "1"},
                {"include_sanity_level", Convert.ToInt32(includeSanityLevel).ToString()},
                {"image_sizes", "px_128x128,small,medium,large,px_480mw"},
                {"profile_image_sizes", "px_170x170,px_50x50"}
            };

            return await AccessApiAsync<Paginated<UsersWork>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>long</c> authorId (required)</para>
        ///     <para>- <c>int</c> page (optional)</para>
        ///     <para>- <c>int</c> perPage (optional)</para>
        ///     <para>- <c>string</c> publicity (optional) [ public, private ]</para>
        ///     <para>- <c>bool</c> includeSanityLevel (optional)</para>
        /// </summary>
        /// <returns>UsersFavoriteWorks. (Pagenated)</returns>
        public async Task<Paginated<UsersFavoriteWork>> GetUsersFavoriteWorksAsync(
            long authorId,
            int page = 1,
            int perPage = 30,
            string publicity = "public",
            bool includeSanityLevel = true)
        {
            var url = "https://public-api.secure.pixiv.net/v1/users/" + authorId + "/favorite_works.json";

            var param = new Dictionary<string, string>
            {
                {"page", page.ToString()},
                {"per_page", perPage.ToString()},
                {"publicity", publicity},
                {"include_stats", "1"},
                {"include_sanity_level", Convert.ToInt32(includeSanityLevel).ToString()},
                {"image_sizes", "px_128x128,small,medium,large,px_480mw"},
                {"profile_image_sizes", "px_170x170,px_50x50"}
            };

            return await AccessApiAsync<Paginated<UsersFavoriteWork>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>long</c> maxId (optional)</para>
        ///     <para>- <c>bool</c> showR18 (optional)</para>
        /// </summary>
        /// <returns>Feed.</returns>
        public async Task<List<Feed>> GetUsersFeedsAsync(long authorId, long maxId = 0, bool showR18 = true)
        {
            var url = "https://public-api.secure.pixiv.net/v1/users/" + authorId + "/feeds.json";

            var param = new Dictionary<string, string>
            {
                {"relation", "all"},
                {"type", "touch_nottext"},
                {"show_r18", Convert.ToInt32(showR18).ToString()}
            };

            if (maxId != 0)
                param.Add("max_id", maxId.ToString());

            return await AccessApiAsync<List<Feed>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>
        ///         - <c>string</c> mode (optional) [ daily, weekly, monthly, male, female, rookie, daily_r18, weekly_r18,
        ///         male_r18, female_r18, r18g ]
        ///     </para>
        ///     <para>- <c>int</c> page (optional)</para>
        ///     <para>- <c>int</c> perPage (optional)</para>
        ///     <para>- <c>string</c> date (optional) [ 2015-04-01 ]</para>
        ///     <para>- <c>bool</c> includeSanityLevel (optional)</para>
        /// </summary>
        /// <returns>RankingAll. (Pagenated)</returns>
        public async Task<Paginated<Rank>> GetRankingAllAsync(
            string mode = "daily",
            int page = 1,
            int perPage = 30,
            string date = "",
            bool includeSanityLevel = true)
        {
            var url = "https://public-api.secure.pixiv.net/v1/ranking/all";

            var param = new Dictionary<string, string>
            {
                {"mode", mode},
                {"page", page.ToString()},
                {"per_page", perPage.ToString()},
                {"include_stats", "1"},
                {"include_sanity_level", Convert.ToInt32(includeSanityLevel).ToString()},
                {"image_sizes", "px_128x128,small,medium,large,px_480mw"},
                {"profile_image_sizes", "px_170x170,px_50x50"}
            };

            if (!string.IsNullOrWhiteSpace(date))
                param.Add("date", date);

            return await AccessApiAsync<Paginated<Rank>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>string</c> q (required)</para>
        ///     <para>- <c>int</c> page (optional)</para>
        ///     <para>- <c>int</c> perPage (optional)</para>
        ///     <para>- <c>string</c> mode (optional) [ text, tag, exact_tag, caption ]</para>
        ///     <para>- <c>string</c> period (optional) [ all, day, week, month ]</para>
        ///     <para>- <c>string</c> order (optional) [ desc, asc ]</para>
        ///     <para>- <c>string</c> sort (optional) [ date ]</para>
        ///     <para>- <c>bool</c> includeSanityLevel (optional)</para>
        /// </summary>
        /// <returns>Works. (Pagenated)</returns>
        public async Task<Paginated<Work>> SearchWorksAsync(
            string query,
            int page = 1,
            int perPage = 30,
            string mode = "text",
            string period = "all",
            string order = "desc",
            string sort = "date",
            bool includeSanityLevel = true)
        {
            var url = "https://public-api.secure.pixiv.net/v1/search/works.json";

            var param = new Dictionary<string, string>
            {
                {"q", query},
                {"page", page.ToString()},
                {"per_page", perPage.ToString()},
                {"period", period},
                {"order", order},
                {"sort", sort},
                {"mode", mode},

                {"include_stats", "1"},
                {"include_sanity_level", Convert.ToInt32(includeSanityLevel).ToString()},
                {"image_sizes", "px_128x128,small,medium,large,px_480mw"},
                {"profile_image_sizes", "px_170x170,px_50x50"}
            };

            return await AccessApiAsync<Paginated<Work>>(MethodType.Get, url, param);
        }

        /// <summary>
        ///     <para>Available parameters:</para>
        ///     <para>- <c>int</c> page (optional)</para>
        ///     <para>- <c>int</c> perPage (optional)</para>
        ///     <para>- <c>bool</c> includeSanityLevel (optional)</para>
        /// </summary>
        /// <returns>Works. (Pagenated)</returns>
        public async Task<Paginated<Work>> GetLatestWorksAsync(
            int page = 1,
            int perPage = 30,
            bool includeSanityLevel = true)
        {
            var url = "https://public-api.secure.pixiv.net/v1/works.json";

            var param = new Dictionary<string, string>
            {
                {"page", page.ToString()},
                {"per_page", perPage.ToString()},

                {"include_stats", "1"},
                {"include_sanity_level", Convert.ToInt32(includeSanityLevel).ToString()},
                {"image_sizes", "px_128x128,small,medium,large,px_480mw"},
                {"profile_image_sizes", "px_170x170,px_50x50"}
            };

            return await AccessApiAsync<Paginated<Work>>(MethodType.Get, url, param);
        }
    }
}
