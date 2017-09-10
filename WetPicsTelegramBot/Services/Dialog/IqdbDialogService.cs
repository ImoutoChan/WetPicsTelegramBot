using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    class IqdbDialogService : IDialogService<IqdbDialogService>
    {
        private readonly IDialogObserverService _baseDialogService;
        private readonly ILogger<IqdbDialogService> _logger;
        private readonly IMessagesService _messagesService;
        private readonly ICommandsService _commandsService;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IIqdbService _iqdbService;

        private Dictionary<string, Func<Command, Task>> _commandHandlers;

        public IqdbDialogService(IDialogObserverService baseDialogService,
                                 ILogger<IqdbDialogService> logger,
                                 IMessagesService messagesService,
                                 ICommandsService commandsService,
                                 ITelegramBotClient telegramBotClient,
                                 IIqdbService iqdbService)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
            _commandsService = commandsService;
            _telegramBotClient = telegramBotClient;
            _iqdbService = iqdbService;

            SetupCommandHandlers();
        }

        private void SetupCommandHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<Command, Task>>
            {
                {_commandsService.SearchIqdbCommandText, OnNextSearchIqdbCommand},
                {_commandsService.GetTagsCommandText, OnNextGetTagsCommand}
            };
        }

        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable.GroupBy(x => x.CommandName)
                .Where(group => _commandHandlers.ContainsKey(group.Key))
                .Subscribe(group => group.HandleAsync(_commandHandlers[group.Key]).Subscribe());
        }

        private async Task OnNextSearchIqdbCommand(Command command)
        {
            _logger.LogTrace($"{_commandsService.SearchIqdbCommandText} command recieved");
            var message = command.Message;

            if (message.ReplyToMessage == null || !message.ReplyToMessage.Photo.Any())
            {
                await _baseDialogService.Reply(message, _messagesService.ReplyToImage);
                return;
            }

            string searchResults;

            using (MemoryStream ms = new MemoryStream())
            {
                await _telegramBotClient.GetFileAsync(message.ReplyToMessage.Photo.Last().FileId, ms);

                searchResults = await _iqdbService.SearchImage(ms);
            }

            await _baseDialogService.Reply(message, searchResults, parseMode: ParseMode.Html);
        }

        private Task OnNextGetTagsCommand(Command arg)
        {
            throw new NotImplementedException();
        }
    }
}