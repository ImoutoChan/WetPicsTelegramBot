using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using WetPicsTelegramBot.Services;

namespace WetPicsTelegramBot
{
    public class App : IDisposable
    {
        private readonly ILogger<App> _logger;
        private readonly ITelegramBotClient _telegramBotClient;

        private readonly PhotoPublisherService _photoPublisherService;
        private readonly DialogService _dialogService;
        private readonly PixivService _pixivService;

        public App(ITelegramBotClient telegramBotClient, ILogger<App> logger, IServiceProvider serviceProvider)
        {
            _telegramBotClient = telegramBotClient;
            _telegramBotClient.OnReceiveError += BotOnReceiveError;
            _logger = logger;

            _photoPublisherService = serviceProvider.GetService<PhotoPublisherService>();
            _dialogService = serviceProvider.GetService<DialogService>();

            _pixivService = serviceProvider.GetService<PixivService>();
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