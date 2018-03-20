using System;

namespace WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract
{
    public interface IAwaitedMessage
    {
        Type[] AwaitedForHandler { get; }
    }
}