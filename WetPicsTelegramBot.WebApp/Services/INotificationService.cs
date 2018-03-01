using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(Update update);
    }
}