using Telegram.Bot.Types;

namespace WetPicsTelegramBot.Models
{
    class Command
    {
        public Command(string commandName, Message message)
        {
            CommandName = commandName;
            Message = message;
        }

        public string CommandName { get; set; }

        public Message Message { get; set; }
    }
}