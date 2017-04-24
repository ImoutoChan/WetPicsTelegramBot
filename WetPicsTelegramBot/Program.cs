using System;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace WetPicsTelegramBot
{

    class Bot : IDisposable
    {
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
            Debugger.Break();
        }

        public void Dispose()
        {
            _api.StopReceiving();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            using (var bot = new Bot("363430484:AAEeGzJTboUScF5V1lHX8s_3fk3EKRX9PqQ"))
            {
                Console.ReadLine();
            };
        }
    }
}