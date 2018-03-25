using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface ITgClient
    {
        Task<User> GetMe();

        ITelegramBotClient Client { get; }

        Task<string> GetCommand(Message message);

        Task<bool> CheckOnAdmin(string targetChatId, int userId);

        InlineKeyboardMarkup GetPhotoKeyboard(int likesCount);

        ReplyKeyboardMarkup GetReplyKeyboardFromEnum<T>(int splitBy = 6)
            where T : struct, IConvertible;
    }
}