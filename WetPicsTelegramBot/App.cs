using System;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using WetPicsTelegramBot.Services;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot
{
    class App : IDisposable
    {
        private readonly ILogger<App> _logger;
        private readonly ITelegramBotClient _telegramBotClient;

        private readonly PhotoPublisherService _photoPublisherService;
        private readonly PixivService _pixivService;
        private readonly IDialogServiceInitializer _dialogServiceInitializer;

        public App(ITelegramBotClient telegramBotClient, 
                    ILogger<App> logger,
                    PhotoPublisherService photoPublisherService,
                    PixivService pixivService,
                    IDialogServiceInitializer dialogServiceInitializer)
        {
            _telegramBotClient = telegramBotClient;
            _telegramBotClient.OnReceiveError += BotOnReceiveError;
            _logger = logger;
            
            _pixivService = pixivService;
            _photoPublisherService = photoPublisherService;

            _dialogServiceInitializer = dialogServiceInitializer;
            _dialogServiceInitializer.Subscribe();
        }

        public void Run()
        {
            _telegramBotClient.StartReceiving();
            _logger.LogInformation("App started.");

            while (true)
            {
                Console.ReadLine();
            }
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            _logger.LogError("Received bot exception: " + receiveErrorEventArgs.ApiRequestException.Message);
        }

        public void Dispose()
        {
            _telegramBotClient.StopReceiving();
        }
    }
}