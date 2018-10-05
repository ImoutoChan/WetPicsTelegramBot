using System.Linq;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Extensions;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract
{
    public abstract class ReplyHandler : MessageHandler
    {
        protected int? FoundReplyTo;

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
        {
            var messageAwaited = false;
            if (IsMessageAwaitedWithReplay(message, out var awaited))
            {
                FoundReplyTo = message.ReplyToMessage.MessageId;
                messageAwaited = true;
            }
            else if (IsMessageAwaitedInUserChat(message, out awaited))
            {
                FoundReplyTo = message.MessageId - 1;
                messageAwaited = true;
            }
            return messageAwaited 
                && awaited.AwaitedForHandler.Contains(GetType());
        }

        private bool IsMessageAwaitedInUserChat(Message message, out IAwaitedMessage awaited)
        {
            awaited = null;

            return message.Chat.IsUserChat()
                && message.ReplyToMessage == null
                && AwaitedRepliesService
                   .AwaitedReplies
                   .TryGetValue(message.MessageId - 1, out awaited);
        }

        private bool IsMessageAwaitedWithReplay(Message message, out IAwaitedMessage awaited)
        {
            awaited = null;

            return message.ReplyToMessage != null
                && AwaitedRepliesService
                   .AwaitedReplies.
                    TryGetValue(message.ReplyToMessage.MessageId, out awaited);
        }
    }
}