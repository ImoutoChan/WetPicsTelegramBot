using System.Linq;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract
{
    public abstract class ReplyHandler : MessageHandler
    {

        protected ReplyHandler(ITgClient tgClient, 
                               ILogger logger, 
                               ICommandsProvider commandsProvider, 
                               IMessagesProvider messagesProvider, 
                               IAwaitedRepliesService awaitedRepliesService) 
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            AwaitedRepliesService = awaitedRepliesService;
        }

        protected IAwaitedRepliesService AwaitedRepliesService { get; }


        protected virtual bool IsMessageAwaited(Message message)
            => message.ReplyToMessage != null
                && AwaitedRepliesService
                   .AwaitedReplies
                   .TryGetValue(message.ReplyToMessage.MessageId, out var awaitedMessage)
                && awaitedMessage.AwaitedForHandler.Contains(GetType());
    }
}