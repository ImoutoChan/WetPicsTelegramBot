using System;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Danbooru;

namespace WetPicsTelegramBot.WebApp.Models.AwaitedReplies
{
    public class DanbooruModeAwaitedReply : IAwaitedMessage
    {
        public Type[] AwaitedForHandler => new[] { typeof(SelectDanbooruModeReplyHandler) };
    }
}