using System.Threading.Tasks;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Models;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    internal interface IScheduledResultsService
    {
        Task PostResults(ChatId chatId, ScheduledResultType scheduledResultType);
    }
}