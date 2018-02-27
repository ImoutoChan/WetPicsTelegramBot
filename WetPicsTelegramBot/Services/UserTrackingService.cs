using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    class UserTrackingService : IUserTrackingService
    {
        private readonly IMessagesObservableService _messagesObservableService;
        private readonly IDbRepository _dbRepository;
        private readonly ILogger<UserTrackingService> _logger;

        public UserTrackingService(IMessagesObservableService messagesObservableService, 
                                   IDbRepository dbRepository,
                                   ILogger<UserTrackingService> logger)
        {
            _messagesObservableService = messagesObservableService;
            _dbRepository = dbRepository;
            _logger = logger;
        }

        public void Subscribe()
        {
            _messagesObservableService
                .BaseObservable
                .HandleAsyncWithLogging(TrackUser, _logger)
                .Subscribe();
        }

        private async Task TrackUser(Message message)
        {
            var user = message.From;
            await _dbRepository.SaveOrUpdateUser(user.Id, user.FirstName, user.LastName, user.Username);
        }
    }
}
