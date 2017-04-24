using System;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace WetPicsTelegramBot
{
    internal class Bot : IDisposable
    {
        ILogger Logger { get; } = ApplicationLogging.CreateLogger<Bot>();

        private readonly TelegramBotClient _api;
        private readonly PhotoPublisherService _photoPublisherService;
        private readonly DialogServive _dialogService;

        public Bot(string token)
        {
            _api = new TelegramBotClient(token);
            _api.OnReceiveError += BotOnReceiveError;

            _photoPublisherService = new PhotoPublisherService(_api);
            _photoPublisherService.Init();

            _dialogService = new DialogServive(_api);

            _api.StartReceiving();
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Logger.LogError(receiveErrorEventArgs.ApiRequestException.Message, "bot exception");
        }

        public void Dispose()
        {
            _api.StopReceiving();
        }
    }
}