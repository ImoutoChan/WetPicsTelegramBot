using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv
{
    public class SelectPixivIntervalReplyHandler : ReplyHandler
    {
        private readonly IPixivSettingsService _pixivSettingsService;

        public SelectPixivIntervalReplyHandler(ITgClient tgClient, 
                                               ILogger<SelectPixivIntervalReplyHandler> logger, 
                                               ICommandsProvider commandsProvider, 
                                               IMessagesProvider messagesProvider, 
                                               IAwaitedRepliesService awaitedRepliesService,
                                               IPixivSettingsService pixivSettingsService) 
            : base(tgClient, logger, commandsProvider, messagesProvider, awaitedRepliesService)
        {
            _pixivSettingsService = pixivSettingsService;
        }

        protected override bool WantHandle(Message message, string command)
            => IsMessageAwaited(message);

        protected override async Task Handle(Message message, 
                                             string command, 
                                             CancellationToken cancellationToken)
        {
            var repliedTo = message.ReplyToMessage.MessageId;

            if (!AwaitedRepliesService.AwaitedReplies.TryGetValue(repliedTo, out var awaitedMessage)
                || !(awaitedMessage is PixivIntervalAwaitedReply intervalAwaitedReply)
                || !int.TryParse(message.Text, out var interval))
            {
                await TgClient.Reply(message, 
                                     MessagesProvider.PixivIncorrectInterval, 
                                     cancellationToken);
                return;
            }

            await _pixivSettingsService.Add(message.Chat.Id, 
                                            intervalAwaitedReply.SelectedTopType, 
                                            interval);
            
            await TgClient.Reply(message, 
                                 MessagesProvider.PixivWasActivated, 
                                 cancellationToken);

            AwaitedRepliesService.AwaitedReplies.TryRemove(message.ReplyToMessage.MessageId, out _);
        }
    }
}