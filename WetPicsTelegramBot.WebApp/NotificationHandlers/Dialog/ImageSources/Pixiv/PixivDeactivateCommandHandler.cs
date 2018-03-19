using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv
{
    public class PixivDeactivateCommandHandler : MessageHandler
    {
        private readonly IPixivSettingsService _pixivSettingsService;

        public PixivDeactivateCommandHandler(ITgClient tgClient,
                                             ILogger<PixivDeactivateCommandHandler> logger,
                                             ICommandsProvider commandsProvider,
                                             IMessagesProvider messagesProvider,
                                             IPixivSettingsService pixivSettingsService)
            : base(tgClient, logger, commandsProvider, messagesProvider)
        {
            _pixivSettingsService = pixivSettingsService;
        }

        protected override bool WantHandle(Message message, string command)
            => command == CommandsProvider.DeactivatePixivCommandText;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            await _pixivSettingsService.Remove(message.Chat.Id);

            await TgClient.Client.SendTextMessageAsync(message.Chat.Id, 
                                                       MessagesProvider.PixivWasDeactivated, 
                                                       cancellationToken: cancellationToken);
        }
    }
}