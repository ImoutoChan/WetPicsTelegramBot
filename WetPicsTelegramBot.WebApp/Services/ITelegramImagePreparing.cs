using System.IO;

namespace WetPicsTelegramBot.WebApp.Services
{
    public interface ITelegramImagePreparing
    {
        Stream Prepare(Stream input, long inputLength);
    }
}