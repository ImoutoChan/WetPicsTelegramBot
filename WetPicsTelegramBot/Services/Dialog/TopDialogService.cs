﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Database.Model;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    class TopDialogService : IDialogService<TopDialogService>
    {
        private enum TopSource
        {
            My,
            Global,
            Reply
        }

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
                {_commandsService.TopCommandText, OnNextTopCommand},
                {_commandsService.MyTopCommandText, OnNextMyTopCommand},
                {_commandsService.GlobalTopCommandText, OnNextGlobalTopCommand},
                {_commandsService.TopSCommandText, OnNextTopSCommand},
                {_commandsService.MyTopSCommandText, OnNextMyTopSCommand},
                {_commandsService.GlobalTopSCommandText, OnNextGlobalTopSCommand},
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

                await PostTop(command, TopSource.Reply);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(OnNextTopCommand)} method");
            }
        }

        private async Task OnNextMyTopCommand(Command command)
        {
            try
            {
                _logger.LogTrace($"{_commandsService.MyTopCommandText} command recieved");

                await PostTop(command, TopSource.My);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(OnNextTopCommand)} method");
            }
        }

        private async Task OnNextGlobalTopCommand(Command command)
        {
            try
            {
                _logger.LogTrace($"{_commandsService.GlobalTopCommandText} command recieved");

                await PostTop(command, TopSource.Global);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(OnNextTopCommand)} method");
            }
        }


        [Obsolete]
        private async Task OnNextTopSCommand(Command command)
        {
            try
            {
                _logger.LogTrace($"{_commandsService.TopSCommandText} command recieved");

                await PostTop(command, TopSource.Reply, true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(OnNextTopCommand)} method");
            }
        }

        [Obsolete]
        private async Task OnNextMyTopSCommand(Command command)
        {
            try
            {
                _logger.LogTrace($"{_commandsService.MyTopSCommandText} command recieved");

                await PostTop(command, TopSource.My, true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(OnNextTopCommand)} method");
            }
        }

        [Obsolete]
        private async Task OnNextGlobalTopSCommand(Command command)
        {
            try
            {
                _logger.LogTrace($"{_commandsService.GlobalTopSCommandText} command recieved");

                await PostTop(command, TopSource.Global, true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(OnNextTopCommand)} method");
            }
        }

        private async Task PostTop(Command command, TopSource topSource = TopSource.Reply, bool slow = false, int count = 5)
        {

            var message = command.Message;

            User user = null;
            var messageText = new StringBuilder();

            switch (topSource)
            {
                case TopSource.Reply:
                    if (message.ReplyToMessage == null)
                    {
                        await _baseDialogService.Reply(message, _messagesService.TopReplyToUser);
                        return;
                    }
                    user = message.ReplyToMessage.From;
                    messageText.AppendLine($"Топ пользователя {user.GetBeautyName()} за все время.");
                    break;
                case TopSource.My:
                    user = message.From;
                    messageText.AppendLine($"Топ пользователя {user.GetBeautyName()} за все время.");
                    break;
                default:
                case TopSource.Global:
                    user = null;
                    messageText.AppendLine($"Топ среди всех постов за все время.");
                    break;
            }


            List<Photo> topPhotots;
            if (slow)
            {
                var results = await _dbRepository.GetTopSlow(user?.Id, count);

                messageText.AppendLine();

                int counter = 1;
                foreach (var topEntry in results)
                {
                    messageText.AppendLine($"{counter++}. Лайков: <b>{topEntry.Likes}</b>");
                }

                await _baseDialogService.Reply(message, messageText.ToString(), ParseMode.Html);

                topPhotots = results.Select(x => x.Photo).ToList();
            }
            else
            {
                var results = await _dbRepository.GetTop(user?.Id, count);

                await _baseDialogService.Reply(message, messageText.ToString(), ParseMode.Html);

                topPhotots = results;
            }
            
            foreach (var topPhoto in topPhotots)
            {
                await _telegramApi.ForwardMessageAsync(command.Message.Chat.Id, topPhoto.ChatId, topPhoto.MessageId);
            }
        }
    }
}