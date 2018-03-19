using System;
using WetPicsTelegramBot.WebApp.Models.Abstract;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv;

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