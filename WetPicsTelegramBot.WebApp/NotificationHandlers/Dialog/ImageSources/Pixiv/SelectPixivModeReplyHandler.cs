using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data.Models;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.Models.AwaitedReplies;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.ImageSources.Pixiv
{
    public class SelectPixivModeReplyHandler : ReplyHandler
    {
        public SelectPixivModeReplyHandler(ITgClient tgClient, 
                                           ILogger<SelectPixivModeReplyHandler> logger, 
                                           ICommandsProvider commandsProvider, 
                                           IMessagesProvider messagesProvider, 
                                           IAwaitedRepliesService awaitedRepliesService) 
            : base(tgClient, logger, commandsProvider, messagesProvider, awaitedRepliesService)
        {
        }

        protected override bool WantHandle(Message message, string command)
            => IsMessageAwaited(message);

        protected override async Task Handle(Message message, 
                                             string command, 
                                             CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(message.Text, out PixivTopType selectedMode) 
                || !selectedMode.IsDefined())
            {
                await TgClient.Reply(message, 
                                     MessagesProvider.PixivIncorrectMode, 
                                     cancellationToken);
                return;
            }

            var mes = await TgClient.Reply(message,
                                           String.Format(MessagesProvider.SelectPixivIntervalMessageF, message.Text),
                                           cancellationToken,
                                           replyMarkup: new ForceReplyMarkup { Selective = true });

            AwaitedRepliesService.AwaitedReplies.TryRemove(message.ReplyToMessage.MessageId, out _);
            AwaitedRepliesService.AwaitedReplies
               .TryAdd(mes.MessageId,
                       new PixivIntervalAwaitedReply(selectedMode));
        }
    }
}