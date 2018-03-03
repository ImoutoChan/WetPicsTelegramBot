using System.Threading.Tasks;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IPixivService
    {
        Task TriggerPostNext();
    }
}