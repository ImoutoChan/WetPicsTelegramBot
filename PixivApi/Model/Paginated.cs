using System.Collections.Generic;
using Newtonsoft.Json;

namespace PixivApi.Model
{
    interface IPagenated
    {
        Pagination Pagination { get; set; }
    }

    public class Paginated<T> : List<T> , IPagenated
        where T : class, new()
    {
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {

        [JsonProperty("previous")]
        public int? Previous { get; set; }

        [JsonProperty("next")]
        public int? Next { get; set; }

        [JsonProperty("current")]
        public int? Current { get; set; }

        [JsonProperty("per_page")]
        public int? PerPage { get; set; }

        [JsonProperty("total")]
        public int? Total { get; set; }

        [JsonProperty("pages")]
        public int? Pages { get; set; }
    }

}
