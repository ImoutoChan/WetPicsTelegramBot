using System.Threading.Tasks;

namespace WetPicsTelegramBot.Services.Abstract
{
    interface ISchedulerService
    {
        Task StartScheduler();
    }
}