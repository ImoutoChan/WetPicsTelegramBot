using System;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Yandere;

namespace WetPicsTelegramBot.WebApp.Models.AwaitedReplies
{
    public class YandereModeAwaitedReply : IAwaitedMessage
    {
        public Type[] AwaitedForHandler => new[] { typeof(SelectYandereModeReplyHandler) };
    }
}