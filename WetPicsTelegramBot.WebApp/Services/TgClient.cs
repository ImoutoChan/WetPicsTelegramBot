using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
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

        public async Task<string> GetCommand(Message message)
        {
            var me = await GetMe();
            var botUsername = me.Username;

            var text = message?.Text;

            if (String.IsNullOrWhiteSpace(text) || !text.StartsWith("/"))
            {
                return null;
            }

            var firstWord = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
            var isCommandWithId = firstWord.Contains("@") && firstWord.EndsWith(botUsername);
            var command = isCommandWithId ? firstWord.Split('@').First() : firstWord;

            return command;
        }

        public async Task<bool> CheckOnAdmin(string targetChatId, int userId)
        {
            try
            {
                var admins = await Client.GetChatAdministratorsAsync(targetChatId);

                var isAdmin = admins.FirstOrDefault(x => x.User.Id == userId);

                if (isAdmin == null || isAdmin.CanPostMessages == false)
                {
                    return false;
                }
                
                return true;
            }
            catch (ApiRequestException ex) 
                when (ex.Message == "Bad Request: there is no administrators in the private chat")
            {
                // target chat is private chat
                return true;
            }
        }
    }
}