using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Danbooru
{
    public class SelectDanbooruModeReplyHandler : ReplyHandler
    {
        private readonly IWetpicsService _wetpicsService;

        public SelectDanbooruModeReplyHandler(ITgClient tgClient, 
            ILogger<SelectDanbooruModeReplyHandler> logger, 
            ICommandsProvider commandsProvider, 
            IMessagesProvider messagesProvider, 
            IAwaitedRepliesService awaitedRepliesService,
            IWetpicsService wetpicsService) 
            : base(tgClient, logger, commandsProvider, messagesProvider, awaitedRepliesService)
        {
            _wetpicsService = wetpicsService;
        }

        protected override bool WantHandle(Message message, string command)
            => IsMessageAwaited(message);

        protected override async Task Handle(Message message, 
                                             string command, 
                                             CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(message.Text, out DanbooruTopType selectedMode) 
                || !selectedMode.IsDefined())
            {
                await TgClient.Reply(message, 
                                     MessagesProvider.IncorrectMode, 
                                     cancellationToken);
                return;
            }

            await _wetpicsService.AddImageSource(message.Chat.Id, ImageSource.Danbooru, selectedMode.ToString());

            await TgClient.Reply(message, MessagesProvider.DanbooruSourceAddSuccess, cancellationToken);
            
            AwaitedRepliesService.AwaitedReplies.TryRemove(FoundReplyTo.Value, out _);
        }
    }
}