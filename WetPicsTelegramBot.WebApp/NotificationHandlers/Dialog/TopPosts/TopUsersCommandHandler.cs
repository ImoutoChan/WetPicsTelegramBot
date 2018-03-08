using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Models;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.Dialog.TopPosts
{
    public class TopUsersCommandHandler : MessageHandler
    {
        private readonly ITopRatingService _topRatingService;

        public TopUsersCommandHandler(ITgClient tgClient,
                                      ICommandsProvider commandsProvider,
                                      ILogger<TopUsersCommandHandler> logger,
                                      IMessagesProvider messagesProvider,
                                      ITopRatingService topRatingService)
            : base(tgClient, 
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
            _topRatingService = topRatingService;
        }

        protected override bool WantHandle(Message message, string command) 
            => command == CommandsProvider.TopUsersCommandText;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var args = new TopRequestArgs(message.Text);
            await _topRatingService.PostUsersTop(message.Chat.Id,
                                                 message.MessageId,
                                                 count: args.Count,
                                                 period: args.TopPeriod);
        }
    }
}