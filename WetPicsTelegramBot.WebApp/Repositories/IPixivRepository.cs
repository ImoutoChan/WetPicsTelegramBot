using System.Threading.Tasks;
using PixivApi.Model;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.WebApp.Repositories
{
    public interface IPixivRepository
    {
        Task<Paginated<Rank>> GetPixivTop(
            PixivTopType type,
            int page = 1,
            int count = 100);
    }
}