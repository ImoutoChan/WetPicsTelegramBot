using Telegram.Bot.Types.Enums;

namespace WetPicsTelegramBot.WebApp.Models
{
    public class ReplyMessage
    {
        public ReplyMessage(string message, ParseMode parseMode = ParseMode.Default)
        {
            Message = message;
            ParseMode = parseMode;
        }

        public string Message { get; set; }

        public ParseMode ParseMode { get; set; }
    }
}