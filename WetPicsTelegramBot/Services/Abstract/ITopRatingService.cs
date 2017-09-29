using System.Threading.Tasks;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Dialog;

namespace WetPicsTelegramBot.Services.Abstract
{
    interface ITopRatingService
    {
        Task PostTop(long chatId,
                     int messageId, 
                     TopSource topSource = TopSource.Reply, 
                     int count = 5, 
                     TopPeriod period = TopPeriod.AllTime, 
                     User user = null);
    }
}