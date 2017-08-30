using System.Threading.Tasks;

namespace WetPicsTelegramBot.Services
{
    internal interface IImageRepostService
    {
        Task PostToTargetIfExists(long chatId, string caption, string fileId, int fromUserId);
    }
}