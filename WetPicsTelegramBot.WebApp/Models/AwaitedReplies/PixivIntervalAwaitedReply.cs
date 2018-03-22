using System;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources;

namespace WetPicsTelegramBot.WebApp.Models.AwaitedReplies
{
    public class WetpicsIntervalAwaitedReply : IAwaitedMessage
    {
        public Type[] AwaitedForHandler => new[] { typeof(WetpicsIntervalReplyHandler) };
    }
}