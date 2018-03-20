using System.Collections.Concurrent;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;

namespace WetPicsTelegramBot.WebApp.Services.Abstract
{
    public interface IAwaitedRepliesService
    {
        ConcurrentDictionary<int, IAwaitedMessage> AwaitedReplies { get; }
    }
}