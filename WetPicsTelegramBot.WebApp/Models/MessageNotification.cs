using System;
using System.Linq;
using MediatR;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.WebApp.Models
{
    public class MessageNotification : INotification
    {
        public MessageNotification(Message message, )
        {
            Message = message;

            var first Message.Text.Split(' ').FirstOrDefault();

            Command = 
        }

        public Message Message { get; }

        public string Command { get; }

        private string GetCommandText(Message message, string botUsername)
        {
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
    }
}