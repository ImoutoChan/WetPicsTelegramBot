using System;

namespace WetPicsTelegramBot.WebApp.Models.Abstract
{
    public interface IAwaitedMessage
    {
        Type AwaitedForHandler { get; }
    }
}