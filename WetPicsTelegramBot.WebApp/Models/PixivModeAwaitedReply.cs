using System;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.Pixiv;

namespace WetPicsTelegramBot.WebApp.Models
{
    public class PixivModeAwaitedReply : IAwaitedMessage
    {
        public Type AwaitedForHandler => typeof(PixivActivateModelReplyHandler);
    }

    public class SelectImageSourceAwaitedReply : IAwaitedMessage
    {
        public Type AwaitedForHandler => typeof(SelectImageSourceReplyHandler);
    }
}