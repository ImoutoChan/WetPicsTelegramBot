using System.IO;

namespace PixivApi.Models
{
    public record Post(PostHeader PostHeader, string Url, Stream File, long FileSize);
}
