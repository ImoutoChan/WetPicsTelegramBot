using System;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv;

namespace WetPicsTelegramBot.WebApp.Models.AwaitedReplies
{
    public class PixivIntervalAwaitedReply : IAwaitedMessage
    {
        public PixivIntervalAwaitedReply(PixivTopType selectedTopType)
        {
            SelectedTopType = selectedTopType;
        }

        public Type[] AwaitedForHandler => new[] { typeof(SelectPixivIntervalReplyHandler) };

        public PixivTopType SelectedTopType { get; }
    }
}