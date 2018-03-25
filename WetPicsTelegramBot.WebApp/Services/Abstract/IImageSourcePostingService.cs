using System.Threading.Tasks;

namespace WetPicsTelegramBot.WebApp.Services
{
    public interface IImageSourcePostingService
    {
        Task TriggerPostNext();
    }
}