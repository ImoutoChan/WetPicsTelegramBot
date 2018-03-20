using System;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;

namespace WetPicsTelegramBot.WebApp.Models.AwaitedReplies
{
    public class PixivIntervalAwaitedReply : IAwaitedMessage
    {
        public PixivIntervalAwaitedReply(PixivTopType selectedTopType)
        {
            SelectedTopType = selectedTopType;
        }

        public Type[] AwaitedForHandler => new[] { typeof(PixivIntervalAwaitedReply) };

        public PixivTopType SelectedTopType { get; }
    }
}