using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources
{
    public class RemoveImageSourceCommandHandler : MessageHandler
    {
        private readonly IWetpicsService _wetpicsService;

        public RemoveImageSourceCommandHandler(ITgClient tgClient, 
                                               ILogger<RemoveImageSourceCommandHandler> logger, 
                                               ICommandsProvider commandsProvider, 
                                               IMessagesProvider messagesProvider,
                                               IWetpicsService wetpicsService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _wetpicsService = wetpicsService;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.RemoveImageSourceCommandText;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var match = Regex.Match(message.Text, @"\d+");
            if (match.Success)
            {
                var id = Int32.Parse(match.Value);

                var imageSources = await _wetpicsService.GetImageSources(message.Chat.Id);

                if (id < imageSources.Count)
                {
                    await _wetpicsService.RemoveImageSource(imageSources[id].Id);


                    await TgClient.Reply(message, MessagesProvider.RemoveImageSourceSuccess, cancellationToken);

                    return;
                }
            }

            await TgClient.Reply(message, MessagesProvider.RemoveImageSourceFail, cancellationToken);
        }
    }
}