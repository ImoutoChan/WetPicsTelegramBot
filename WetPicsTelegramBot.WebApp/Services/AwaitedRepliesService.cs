using System.Collections.Concurrent;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class AwaitedRepliesService : IAwaitedRepliesService
    {
        public ConcurrentDictionary<int, IAwaitedMessage> AwaitedReplies 
            => new ConcurrentDictionary<int, IAwaitedMessage>();
    }
}