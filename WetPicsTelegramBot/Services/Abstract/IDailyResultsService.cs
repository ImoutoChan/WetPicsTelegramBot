using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.Services.Abstract
{
    internal interface IScheduledResultsService
    {
        Task PostDailyResults(ChatId chatId);
        Task PostMonthlyResults(ChatId chatId);
    }
}