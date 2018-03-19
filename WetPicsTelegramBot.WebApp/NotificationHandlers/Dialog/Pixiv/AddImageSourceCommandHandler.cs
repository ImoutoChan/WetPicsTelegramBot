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

    public class SelectImageSourceReplyHandler : MessageHandler
    {
        private readonly IAwaitedRepliesService _pendingPixivRepliesService;

        public SelectImageSourceReplyHandler(ITgClient tgClient, 
                                             ILogger<PixivActivateCommandHandler> logger, 
                                             ICommandsProvider commandsProvider, 
                                             IMessagesProvider messagesProvider,
                                             IAwaitedRepliesService pendingPixivRepliesService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _pendingPixivRepliesService = pendingPixivRepliesService;
        }

        protected override bool WantHandle(Message message, string command)
        {
            return message.ReplyToMessage != null
                && _pendingPixivRepliesService
                   .AwaitedReplies
                   .TryGetValue(message.ReplyToMessage.MessageId, out var awaitedMessage)
                && awaitedMessage.AwaitedForHandler == GetType();
        }

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {

            if (!Enum.TryParse(message.Text, out ImageSource imageSource) || !imageSource.IsDefined())
            {
                await TgClient.Reply(message, MessagesProvider.IncorrectImageSource, cancellationToken);
                return;
            }

            var mes = await TgClient.Reply(message,
                                           MessagesProvider,
                                           cancellationToken,
                                           replyMarkup: new ForceReplyMarkup { Selective = true });

            _pendingPixivRepliesService.AwaitIntervalReply.TryAdd(mes.MessageId, selectedMode);
            _pendingPixivRepliesService.AwaitModeReply.TryRemove(message.ReplyToMessage.MessageId, out _);


            var mes = 
                await TgClient.Reply(message, 
                                     MessagesProvider.SelectImageSource,
                                     cancellationToken,
                                     replyMarkup: GetImageSourceModesKeyboard());

            _pendingPixivRepliesService.AwaitedReplies.TryAdd(mes.MessageId, GetType());
        }

        private ReplyKeyboardMarkup GetImageSourceModesKeyboard()
        {
            var buttons = Enum
               .GetNames(typeof(ImageSource))
               .Select(x => new KeyboardButton(x))
               .ToList();

            return new ReplyKeyboardMarkup(new[] {
                                               buttons.Take(6).ToArray(),
                                               buttons.Skip(6).ToArray(),
                                           },
                                           resizeKeyboard: true,
                                           oneTimeKeyboard: true);
        }
    }
}