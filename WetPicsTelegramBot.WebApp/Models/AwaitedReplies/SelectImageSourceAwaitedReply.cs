using System;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Danbooru;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Yandere;

namespace WetPicsTelegramBot.WebApp.Models.AwaitedReplies
{
    public class SelectImageSourceAwaitedReply : IAwaitedMessage
    {
        public Type[] AwaitedForHandler => new []
        {
            typeof(SelectPixivImageSourceReplyHandler),
            typeof(SelectDanbooruImageSourceReplyHandler),
            typeof(SelectYandereImageSourceReplyHandler)
        };
    }
}