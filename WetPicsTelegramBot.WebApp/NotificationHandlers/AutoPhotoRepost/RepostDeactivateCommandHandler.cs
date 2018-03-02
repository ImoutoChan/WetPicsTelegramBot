using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.AutoPhotoRepost
{
    public class RepostDeactivateCommandHandler : MessageHandler
    {
        private readonly IRepostSettingsService _repostSettingsService;

        public RepostDeactivateCommandHandler(ITgClient tgClient,
                                              ILogger<RepostDeactivateCommandHandler> logger,      
                                              ICommandsProvider commandsProvider,
                                              IMessagesProvider messagesProvider,
                                              IRepostSettingsService repostSettingsService)
            : base(tgClient,
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
            _repostSettingsService = repostSettingsService;
        }

        protected override bool WantHandle(Message message, string command)
            => command == CommandsProvider.DeactivatePhotoRepostCommandText;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var settings = await _repostSettingsService.GetSettings();
            var targetId = settings.FirstOrDefault(x => x.ChatId == message.Chat.Id)?.TargetId;

            if (targetId == null)
            {
                return;
            }

            var hasRights = await TgClient.CheckOnAdmin(targetId, message.From.Id);

            if (!hasRights)
            {
                Logger.LogError($"Remove repost was aborted. " +
                                $"User {message.From.GetBeautyName()} must be admin in target chat.");

                await TgClient.Reply(message, 
                                     MessagesProvider.RepostActivateTargetRestrict, 
                                     cancellationToken);
                return;
            }

            await _repostSettingsService.Remove(message.Chat.Id);
            
            await TgClient.Reply(message, 
                                 MessagesProvider.DeactivatePhotoRepostMessage, 
                                 cancellationToken);
        }
    }
}