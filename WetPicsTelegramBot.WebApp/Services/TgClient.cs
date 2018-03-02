using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    class TgClient : ITgClient
    {
        private User _me;

        public TgClient(ITelegramBotClient telegramBotClient)
        {
            Client = telegramBotClient;
        }

        public async Task<User> GetMe()
        {
            return _me ?? (_me = await Client.GetMeAsync());
        }

        public ITelegramBotClient Client { get; }
    }
}