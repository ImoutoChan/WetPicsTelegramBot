using System.Collections.Concurrent;
using WetPicsTelegramBot.Data.Models;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IPendingPixivRepliesService
    {
        ConcurrentDictionary<int, PixivTopType> AwaitIntervalReply { get; }
        ConcurrentDictionary<int, object> AwaitModeReply { get; }
    }
}