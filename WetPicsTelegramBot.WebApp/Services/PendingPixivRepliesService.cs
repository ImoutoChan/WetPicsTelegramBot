using System.Collections.Concurrent;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class PendingPixivRepliesService : IPendingPixivRepliesService
    {
        // MessageId
        public ConcurrentDictionary<int, object> AwaitModeReply { get; } 
            = new ConcurrentDictionary<int, object>();

        // MessageId, PixivMode
        public ConcurrentDictionary<int, PixivTopType> AwaitIntervalReply { get; } 
            = new ConcurrentDictionary<int, PixivTopType>();
    }
}