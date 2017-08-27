using System;
using Telegram.Bot.Types;

namespace WetPicsTelegramBot.Services.Dialog
{
    internal class CommandEventArgs : EventArgs
    {
        public string Command { get; }

        public Message Message { get; }

        public CommandEventArgs(string command, Message message)
        {
            Command = command;
            Message = message;
        }
    }
}