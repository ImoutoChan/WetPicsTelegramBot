using System.Collections.Generic;
using System.Threading.Tasks;
using PixivApi.Models;

namespace PixivApi
{
    public interface IPixivApiClient
    {
        Task<IReadOnlyCollection<PixivPostHeader>> LoadTop(PixivTopType topType, int page = 1, int count = 100);

        Task<MeasuredStream> DownloadImage(string imageUrl);
    }
}
