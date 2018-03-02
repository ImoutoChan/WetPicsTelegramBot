using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface INotificationService
    {
        Task NotifyAsync(Update update);
    }
}