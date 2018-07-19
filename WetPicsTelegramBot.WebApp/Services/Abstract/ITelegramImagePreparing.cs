using System.IO;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface ITelegramImagePreparing
    {
        Stream Prepare(Stream input, long inputLength);
    }
}