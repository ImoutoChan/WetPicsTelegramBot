using System;

namespace WetPicsTelegramBot.WebApp.Models
{
    public interface IAwaitedMessage
    {
        Type AwaitedForHandler { get; }
    }
}