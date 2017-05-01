using System.Collections.Generic;
using Newtonsoft.Json;

namespace PixivApi.Objects
{
    public class UsersFavoriteWork
    {

        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("tags")]
        public IList<string> Tags { get; set; }

        [JsonProperty("publicity")]
        public string Publicity { get; set; }

        [JsonProperty("work")]
        public Work Work { get; set; }
    }

}
