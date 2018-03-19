using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv
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
    }
}