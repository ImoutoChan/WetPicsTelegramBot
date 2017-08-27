using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    class BaseDialogService : IBaseDialogService
    {
        private readonly ITelegramBotClient _api;
        private readonly ILogger<BaseDialogService> _logger;
        private readonly User _me;

        public BaseDialogService(ITelegramBotClient api,
                                 ILogger<BaseDialogService> logger)
        {
            _api = api;
            _logger = logger;
            _me = _api.GetMeAsync().Result;

            SetupMessageObservable();
        }
        public IObservable<Command> MessageObservable { get; private set; }

        private void SetupMessageObservable()
        {
            MessageObservable = Observable
                .FromEventPattern<MessageEventArgs>(addHandler => _api.OnMessage += addHandler,
                                                    removeHandler => _api.OnMessage -= removeHandler)
                .Select(x => x.EventArgs.Message)
                .Where(x => !String.IsNullOrWhiteSpace(x?.Text))
                .Select( message => new Command(GetCommandText(message, _me.Username), message))
                .ObserveOn(Scheduler.Default);
        }

        private string GetCommandText(Message message, string botUsername)
        {
            var text = message?.Text;

            if (String.IsNullOrWhiteSpace(text) || !text.StartsWith("/"))
            {
                return null;
            }

            var firstWord = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
            var isCommandWithId = firstWord.Contains("@") && firstWord.EndsWith(botUsername);
            var command = isCommandWithId ? firstWord.Split('@').First() : firstWord;

            return command;
        }
        
        public async Task Reply(Message message,
                                string text,
                                ParseMode parseMode = ParseMode.Default,
                                IReplyMarkup replyMarkup = null)
        {
            await _api.SendTextMessageAsync(message.Chat.Id, 
                                            text, 
                                            parseMode, 
                                            replyToMessageId: message.MessageId, 
                                            replyMarkup: replyMarkup);
        }
    }
}
