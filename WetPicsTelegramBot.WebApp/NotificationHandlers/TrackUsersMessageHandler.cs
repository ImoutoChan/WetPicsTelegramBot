using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers
{
    public class TrackUsersMessageHandler : MessageHandler
    {
        private readonly IDbRepository _dbRepository;

        public TrackUsersMessageHandler(ITgClient tgClient, 
                                        ICommandsProvider commandsProvider, 
                                        ILogger<TrackUsersMessageHandler> logger,
                                        IMessagesProvider messagesProvider,
                                        IDbRepository dbRepository)
            : base(tgClient,
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
            _dbRepository = dbRepository;
        }


        protected override bool WantHandle(Message message, string command) 
            => true;

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var user = message.From;
            await _dbRepository.SaveOrUpdateUser(user.Id, 
                                                 user.FirstName, 
                                                 user.LastName, 
                                                 user.Username);
        }
    }
}