using System.Threading.Tasks;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IImageSourcePostingService
    {
        Task TriggerPostNext();
    }
}