using System.IO;
using System.Threading.Tasks;

namespace WetPicsTelegramBot.Services.Abstract
{
    interface IIqdbService
    {
        Task<string> SearchImage(Stream stream);
    }
}