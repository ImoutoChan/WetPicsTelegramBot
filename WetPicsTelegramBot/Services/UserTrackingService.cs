using System;
using System.Threading.Tasks;
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

        public UserTrackingService(IMessagesObservableService messagesObservableService, 
                                   IDbRepository dbRepository)
        {
            _messagesObservableService = messagesObservableService;
            _dbRepository = dbRepository;
        }

        public void Subscribe()
        {
            _messagesObservableService
                .BaseObservable
                .HandleAsync(TrackUser)
                .Subscribe();
        }

        private async Task TrackUser(Message message)
        {
            var user = message.From;
            await _dbRepository.SaveOrUpdateUser(user.Id, user.FirstName, user.LastName, user.Username);
        }
    }
}
