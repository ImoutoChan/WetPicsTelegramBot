using System.Threading.Tasks;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IIqdbService
    {
        Task<string> SearchImage(string fileId);
        Task<string> SearchTags(string fileId);
    }
}