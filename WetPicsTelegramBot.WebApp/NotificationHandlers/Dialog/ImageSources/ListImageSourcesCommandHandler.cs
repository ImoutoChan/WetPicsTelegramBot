using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Data.Entities.ImageSources;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources
{
    public class ListImageSourcesCommandHandler : MessageHandler
    {
        private readonly IWetpicsService _wetpicsService;

        public ListImageSourcesCommandHandler(ITgClient tgClient, 
                                              ILogger<ListImageSourcesCommandHandler> logger, 
                                              ICommandsProvider commandsProvider, 
                                              IMessagesProvider messagesProvider,
                                              IWetpicsService wetpicsService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _wetpicsService = wetpicsService;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.ListImageSourcesCommandText;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var imageSources = await _wetpicsService.GetImageSources(message.Chat.Id);

            if (!imageSources.Any())
            {
                await TgClient.Reply(message, MessagesProvider.ZeroSources, cancellationToken);

                return;
            }

            var reply = BuildListMessage(imageSources);

            await TgClient.Reply(message, reply, cancellationToken);
        }

        private string BuildListMessage(List<ImageSourceSetting> imageSources)
        {
            var sb = new StringBuilder();
            sb.AppendLine("В чате настроены следующие источники:");

            int counter = 0;
            foreach (var imageSource in imageSources)
            {
                sb.AppendLine($"{counter++}. {imageSource.ImageSource} | {imageSource.Options}");
            }

            sb.AppendLine();
            sb.AppendLine($"Для удаления источников используйте комманду " 
                          + $"{CommandsProvider.RemoveImageSourceCommandText} <id>. " 
                          + $"История о запощенных изображениях не удалится.");

            return sb.ToString();
        }
    }
}