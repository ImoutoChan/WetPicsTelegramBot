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

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Yandere
{
    public class SelectYandereModeReplyHandler : ReplyHandler
    {
        private readonly IWetpicsService _wetpicsService;

        public SelectYandereModeReplyHandler(ITgClient tgClient, 
            ILogger<SelectYandereModeReplyHandler> logger, 
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
            if (!Enum.TryParse(message.Text, out YandereTopType selectedMode) 
                || !selectedMode.IsDefined())
            {
                await TgClient.Reply(message, 
                                     MessagesProvider.IncorrectMode, 
                                     cancellationToken);
                return;
            }

            await _wetpicsService.AddImageSource(message.Chat.Id, ImageSource.Yandere, selectedMode.ToString());

            await TgClient.Reply(message, MessagesProvider.YandereSourceAddSuccess, cancellationToken);
            
            AwaitedRepliesService.AwaitedReplies.TryRemove(FoundReplyTo.Value, out _);
        }
    }
}