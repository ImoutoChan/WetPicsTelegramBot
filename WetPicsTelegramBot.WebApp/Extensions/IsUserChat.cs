using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Extensions
{
    public static class ChatExtensions
    {
        public static bool IsUserChat(this Chat chat)
            => chat.Id > 0;
    }
}