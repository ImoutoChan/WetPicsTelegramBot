using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.AutoRepostPhoto
{
    public class RepostActivateCommandHandler : MessageHandler
    {
        public RepostActivateCommandHandler(ITgClient tgClient,
                                            ILogger<RepostActivateCommandHandler> logger,
                                            ICommandsProvider commandsProvider,
                                            IMessagesProvider messagesProvider)
            : base(tgClient,
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.ActivatePhotoRepostCommandText;

        protected override Task Handle(Message message,
                                       string command,
                                       CancellationToken cancellationToken)
            => TgClient.Reply(message, 
                              MessagesProvider.ActivateRepostMessage, 
                              cancellationToken, 
                              new ForceReplyMarkup { Selective = true });
    }
}