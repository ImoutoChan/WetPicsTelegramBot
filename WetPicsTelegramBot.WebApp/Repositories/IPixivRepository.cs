using System.Collections.Generic;
using System.Threading.Tasks;
using PixivApi.Models;
using PixivTopType = WetPicsTelegramBot.Data.Models.PixivTopType;

namespace WetPicsTelegramBot.WebApp.Repositories
{
    public interface IPixivRepository
    {
        Task<IReadOnlyCollection<PixivPostHeader>> GetPixivTop(
            PixivTopType type,
            int page = 1,
            int count = 100);
    }
}
