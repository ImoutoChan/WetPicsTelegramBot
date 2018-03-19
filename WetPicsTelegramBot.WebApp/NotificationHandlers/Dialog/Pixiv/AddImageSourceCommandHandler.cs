using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.Pixiv
{
    public class AddImageSourceCommandHandler : MessageHandler
    {
        private readonly IAwaitedRepliesService _pendingPixivRepliesService;

        public AddImageSourceCommandHandler(ITgClient tgClient, 
                                            ILogger<PixivActivateCommandHandler> logger, 
                                            ICommandsProvider commandsProvider, 
                                            IMessagesProvider messagesProvider,
                                            IAwaitedRepliesService pendingPixivRepliesService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _pendingPixivRepliesService = pendingPixivRepliesService;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.AddImageSourceCommandText;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            
            var mes = 
                await TgClient.Reply(message, 
                                     MessagesProvider.SelectImageSource,
                                     cancellationToken,
                                     replyMarkup: GetImageSourceModesKeyboard());

            _pendingPixivRepliesService
               .AwaitedReplies
               .TryAdd(mes.MessageId, new SelectImageSourceAwaitedReply());
        }

        private ReplyKeyboardMarkup GetImageSourceModesKeyboard()
        {
            var buttons = Enum
               .GetNames(typeof(ImageSource))
               .Select(x => new KeyboardButton(x))
               .ToList();

            return new ReplyKeyboardMarkup(
                new[] {
                    buttons.Take(6).ToArray(),
                    buttons.Skip(6).ToArray(),
                },
                resizeKeyboard: true,
                oneTimeKeyboard: true);
        }
    }

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
               && awaitedMessage.AwaitedForHandler == GetType();
    }

    public class SelectPixivImageSourceReplyHandler : ReplyHandler
    {
        public SelectPixivImageSourceReplyHandler(ITgClient tgClient, 
            ILogger<SelectPixivImageSourceReplyHandler> logger, 
            ICommandsProvider commandsProvider, 
            IMessagesProvider messagesProvider, 
            IAwaitedRepliesService awaitedRepliesService) 
            : base(tgClient, logger, commandsProvider, messagesProvider, awaitedRepliesService)
        {
        }

        protected override bool WantHandle(Message message, string command)
            => IsMessageAwaited(message) 
                   && Enum.TryParse(message.Text, out ImageSource imageSource) 
                   && imageSource == ImageSource.Pixiv;

        protected override async Task Handle(Message message, 
            string command, 
            CancellationToken cancellationToken)
        {
            var mes = await TgClient.Reply(message,
                MessagesProvider.SelectPixivModeMessage,
                cancellationToken,
                replyMarkup: GetPixivModesKeyboard());

            AwaitedRepliesService.AwaitedReplies.TryRemove(message.ReplyToMessage.MessageId, out _);
            AwaitedRepliesService.AwaitedReplies.TryAdd(mes.MessageId, new PixivModeAwaitedReply());
        }

        private ReplyKeyboardMarkup GetPixivModesKeyboard()
        {
            var buttons = Enum
                .GetNames(typeof(PixivTopType))
                .Select(x => new KeyboardButton(x))
                .ToList();

            return new ReplyKeyboardMarkup(
                new[] {
                    buttons.Take(6).ToArray(),
                    buttons.Skip(6).ToArray(),
                },
                resizeKeyboard: true,
                oneTimeKeyboard: true);
        }
    }

    public class SelectPixivModeReplyHandler : ReplyHandler
    {
        public SelectPixivModeReplyHandler(ITgClient tgClient, 
            ILogger<SelectPixivModeReplyHandler> logger, 
            ICommandsProvider commandsProvider, 
            IMessagesProvider messagesProvider, 
            IAwaitedRepliesService awaitedRepliesService) 
            : base(tgClient, logger, commandsProvider, messagesProvider, awaitedRepliesService)
        {
        }

        protected override bool WantHandle(Message message, string command)
            => IsMessageAwaited(message) 
                   && Enum.TryParse(message.Text, out ImageSource imageSource) 
                   && imageSource == ImageSource.Pixiv;

        protected override async Task Handle(Message message, 
            string command, 
            CancellationToken cancellationToken)
        {
            var mes = await TgClient.Reply(message,
                MessagesProvider.SelectPixivModeMessage,
                cancellationToken,
                replyMarkup: GetPixivModesKeyboard());

            AwaitedRepliesService.AwaitedReplies.TryRemove(message.ReplyToMessage.MessageId, out _);
            AwaitedRepliesService.AwaitedReplies.TryAdd(mes.MessageId, new PixivModeAwaitedReply());
        }

        private ReplyKeyboardMarkup GetPixivModesKeyboard()
        {
            var buttons = Enum
                .GetNames(typeof(PixivTopType))
                .Select(x => new KeyboardButton(x))
                .ToList();

            return new ReplyKeyboardMarkup(
                new[] {
                    buttons.Take(6).ToArray(),
                    buttons.Skip(6).ToArray(),
                },
                resizeKeyboard: true,
                oneTimeKeyboard: true);
        }
    }
}