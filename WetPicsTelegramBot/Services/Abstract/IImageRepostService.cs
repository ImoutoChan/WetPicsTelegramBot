using System.Threading.Tasks;

namespace WetPicsTelegramBot.Services.Abstract
{
    internal interface IImageRepostService
    {
        Task PostToTargetIfExists(long chatId, string caption, string fileId, int fromUserId);
    }
}