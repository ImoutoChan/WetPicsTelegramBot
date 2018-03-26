using System;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv;

namespace WetPicsTelegramBot.WebApp.Models.AwaitedReplies
{
    public class PixivModeAwaitedReply : IAwaitedMessage
    {
        public Type[] AwaitedForHandler => new[] { typeof(SelectPixivModeReplyHandler) };
    }
}