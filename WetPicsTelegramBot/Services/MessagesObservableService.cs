using System;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    class MessagesObservableService : IMessagesObservableService
    {
        private readonly ITelegramBotClient _api;
        private readonly ILogger<MessagesObservableService> _logger;

        public MessagesObservableService(ITelegramBotClient api,
                                         ILogger<MessagesObservableService> logger)
        {
            _api = api;
            _logger = logger;

            SetupBaseObservable();
            SetupBaseCallbackObservablee();
        }

        public IObservable<Message> BaseObservable { get; set; }

        public IObservable<CallbackQuery> BaseCallbackObservable { get; set; }

        private void SetupBaseObservable()
        {
            BaseObservable = Observable
                .FromEventPattern<MessageEventArgs>(addHandler => _api.OnMessage += addHandler,
                                                    removeHandler => _api.OnMessage -= removeHandler)
                .Select(x => x.EventArgs.Message);
        }

        private void SetupBaseCallbackObservablee()
        {
            BaseCallbackObservable = Observable
                .FromEventPattern<CallbackQueryEventArgs>(addHandler => _api.OnCallbackQuery += addHandler,
                                                    removeHandler => _api.OnCallbackQuery -= removeHandler)
                .Select(x => x.EventArgs.CallbackQuery);
        }
    }
}