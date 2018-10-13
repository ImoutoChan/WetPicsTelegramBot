using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Factories
{
    public interface IPostingServicesFactory
    {
        IPostingService GetPostingService(ImageSource source);
    }
}