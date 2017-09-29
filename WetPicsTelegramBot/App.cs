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

        private readonly IImageRepostService _imageRepostService;
        private readonly IForwardService _forwardService;
        private readonly IUserTrackingService _userTrackingService;
        private readonly PixivService _pixivService;
        private readonly IDialogServiceInitializer _dialogServiceInitializer;

        public App(ITelegramBotClient telegramBotClient, 
                   ILogger<App> logger,
                   IImageRepostService imageRepostService,
                   PixivService pixivService,
                   IDialogServiceInitializer dialogServiceInitializer,
                   IForwardService forwardService,
                   IUserTrackingService userTrackingService)
        {
            _telegramBotClient = telegramBotClient;
            _telegramBotClient.OnReceiveError += BotOnReceiveError;
            _logger = logger;
            
            _pixivService = pixivService;
            _imageRepostService = imageRepostService;

            _dialogServiceInitializer = dialogServiceInitializer;
            _dialogServiceInitializer.Subscribe();

            _userTrackingService = userTrackingService;
            _userTrackingService.Subscribe();

            _forwardService = forwardService;
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