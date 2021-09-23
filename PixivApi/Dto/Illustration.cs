using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PixivApi.Dto
{
    public record Illustration
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("image_urls")]
        public IllustrationImageUrls? ImageUrls { get; set; }

        [JsonProperty("caption")]
        public string? Caption { get; set; }

        [JsonProperty("restrict")]
        public long Restrict { get; set; }

        [JsonProperty("user")]
        public UserInfo? User { get; set; }

        [JsonProperty("tags")]
        public IEnumerable<Tag>? Tags { get; set; }

        [JsonProperty("tools")]
        public IEnumerable<string>? Tools { get; set; }

        [JsonProperty("create_date")]
        public DateTimeOffset CreateDate { get; set; }

        [JsonProperty("page_count")]
        public long PageCount { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("sanity_level")]
        public long SanityLevel { get; set; }

        [JsonProperty("x_restrict")]
        public long XRestrict { get; set; }

        [JsonProperty("meta_single_page")]
        public IllustrationMetaSinglePage? MetaSinglePage { get; set; }

        [JsonProperty("meta_pages")]
        public IEnumerable<MetaPage>? MetaPages { get; set; }

        [JsonProperty("total_view")]
        public int TotalView { get; set; }

        [JsonProperty("total_bookmarks")]
        public int TotalBookmarks { get; set; }

        [JsonProperty("is_bookmarked")]
        public bool IsBookmarked { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; }

        [JsonProperty("is_muted")]
        public bool IsMuted { get; set; }


        public class IllustrationMetaSinglePage
        {
            [JsonProperty("original_image_url")]
            public string? OriginalImageUrl { get; set; }
        }

        public class IllustrationImageUrls
        {
            [JsonProperty("square_medium")]
            public string? SquareMedium { get; set; }

            [JsonProperty("medium")]
            public string? Medium { get; set; }

            [JsonProperty("large")]
            public string? Large { get; set; }

            [JsonProperty("original")]
            public string? Original { get; set; }
        }

        public class UserInfo
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("name")]
            public string? Name { get; set; }

            [JsonProperty("account")]
            public string? Account { get; set; }

            [JsonProperty("profile_image_urls")]
            public ProfileImageUrls? ProfileImageUrls { get; set; }

            [JsonProperty("is_followed")]
            public bool IsFollowed { get; set; }
        }

        public class MetaPage
        {
            [JsonProperty("image_urls")]
            public IllustrationImageUrls? ImageUrls { get; set; }
        }

        public record Tag
        {
            [JsonProperty("name")]
            public string? Name { get; init; }

            [JsonProperty("translated_name")]
            public string? TranslatedName { get; init; }
        }

        public record ProfileImageUrls
        {
            [JsonProperty("medium")]
            public string? Medium { get; set; }

            [JsonProperty("large")]
            public string? Large { get; set; }
        }
    }
}
