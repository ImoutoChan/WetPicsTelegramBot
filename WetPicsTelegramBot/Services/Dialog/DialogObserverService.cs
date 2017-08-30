﻿using System;
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
    class DialogObserverService : IDialogObserverService
    {
        private readonly ITelegramBotClient _api;
        private readonly ILogger<DialogObserverService> _logger;
        private readonly User _me;

        public DialogObserverService(ITelegramBotClient api,
                                     ILogger<DialogObserverService> logger)
        {
            _api = api;
            _logger = logger;
            _me = _api.GetMeAsync().Result;

            SetupBaseTextObservable();
            SetupMessageObservable();
            SetupRepliesObservable();
        }

        private IObservable<Message> BaseTextObservable { get; set; }

        public IObservable<Command> MessageObservable { get; private set; }

        public IObservable<Message> RepliesObservable { get; private set; }
        
        private void SetupBaseTextObservable()
        {
            BaseTextObservable = Observable
                .FromEventPattern<MessageEventArgs>(addHandler => _api.OnMessage += addHandler,
                                                    removeHandler => _api.OnMessage -= removeHandler)
                .Select(x => x.EventArgs.Message)
                .Where(message => !String.IsNullOrWhiteSpace(message?.Text));
        }

        private void SetupMessageObservable()
        {
            MessageObservable = BaseTextObservable
                .Select( message => new Command(GetCommandText(message, _me.Username), message))
                .Where(command => !String.IsNullOrWhiteSpace(command.CommandName))
                .ObserveOn(Scheduler.Default);
        }

        private void SetupRepliesObservable()
        {
            RepliesObservable = BaseTextObservable
                .Where(message => message.ReplyToMessage?.From != null
                                  && message.ReplyToMessage.From.Id.ToString() == _me.Id.ToString())
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
        
        public async Task<Message> Reply(Message message,
                                            string text,
                                            ParseMode parseMode = ParseMode.Default,
                                            IReplyMarkup replyMarkup = null)
        {
            return await _api.SendTextMessageAsync(message.Chat.Id, 
                                                    text, 
                                                    parseMode, 
                                                    replyToMessageId: message.MessageId, 
                                                    replyMarkup: replyMarkup);
        }
    }
}
