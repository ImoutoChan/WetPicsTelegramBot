using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    class TopDialogService : IDialogService<TopDialogService>
    {
        private readonly IDialogObserverService _baseDialogService;
        private readonly ILogger<TopDialogService> _logger;
        private readonly IMessagesService _messagesService;
        private readonly ICommandsService _commandsService;
        private readonly IDbRepository _dbRepository;
        private readonly ITelegramBotClient _telegramApi;

        private Dictionary<string, Func<Command, Task>> _commandHandlers;

        public TopDialogService(IDialogObserverService baseDialogService,
                                ILogger<TopDialogService> logger,
                                IMessagesService messagesService,
                                ICommandsService commandsService,
                                IDbRepository dbRepository,
                                ITelegramBotClient telegramApi)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
            _commandsService = commandsService;
            _dbRepository = dbRepository;
            _telegramApi = telegramApi;

            SetupCommandHandlers();
        }

        private void SetupCommandHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<Command, Task>>
            {
                {_commandsService.TopCommandText, OnNextTopCommand}
            };
        }

        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable.GroupBy(x => x.CommandName)
                .Where(group => _commandHandlers.ContainsKey(group.Key))
                .Subscribe(group => group.HandleAsync(_commandHandlers[group.Key]).Subscribe());
        }

        private async Task OnNextTopCommand(Command command)
        {
            try
            { 
                _logger.LogTrace($"{_commandsService.TopCommandText} command recieved");

                var message = command.Message;

                if (message.ReplyToMessage == null)
                {
                    await _baseDialogService.Reply(message, _messagesService.TopReplyToUser);
                    return;
                }

                var user = message.ReplyToMessage.From;

                var result = await _dbRepository.GetTop(user.Id, 3);

                await _baseDialogService.Reply(message, $"Топ пользователя {user.GetBeautyName()} за все время.");
                foreach (var photo in result)
                {
                    await _telegramApi.ForwardMessageAsync(command.Message.Chat.Id, photo.ChatId, photo.MessageId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(OnNextTopCommand)} method");
            }
        }
    }
}