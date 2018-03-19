using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.Pixiv
{
    public class PixivActivateCommandHandler : MessageHandler
    {
        private readonly IAwaitedRepliesService _pendingPixivRepliesService;

        public PixivActivateCommandHandler(ITgClient tgClient, 
                                           ILogger<PixivActivateCommandHandler> logger, 
                                           ICommandsProvider commandsProvider, 
                                           IMessagesProvider messagesProvider,
                                           IAwaitedRepliesService pendingPixivRepliesService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _pendingPixivRepliesService = pendingPixivRepliesService;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.ActivatePixivCommandText;

        protected override async Task Handle(Message message,
                                       string command,
                                       CancellationToken cancellationToken)
        {
            
            var mes = 
                await TgClient.Reply(message, 
                                     MessagesProvider.SelectPixivModeMessage,
                                     cancellationToken,
                                     replyMarkup: GetPixivModesKeyboard());

            _pendingPixivRepliesService.AwaitModeReply.TryAdd(mes.MessageId, null);
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