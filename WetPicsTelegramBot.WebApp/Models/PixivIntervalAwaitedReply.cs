using System;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Models.Abstract;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv;

namespace WetPicsTelegramBot.WebApp.Models
{
    public class PixivIntervalAwaitedReply : IAwaitedMessage
    {
        public PixivIntervalAwaitedReply(PixivTopType selectedTopType)
        {
            SelectedTopType = selectedTopType;
        }

        public Type AwaitedForHandler => typeof(PixivActivateIntervalReplyHandler);

        public PixivTopType SelectedTopType { get; }
    }
}