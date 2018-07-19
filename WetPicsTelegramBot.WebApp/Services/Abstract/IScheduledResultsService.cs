using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    internal interface IScheduledResultsService
    {
        Task PostDailyResults(ChatId chatId);
        Task PostMonthlyResults(ChatId chatId);
        Task PostWeeklyResults(ChatId chatId);
    }
}