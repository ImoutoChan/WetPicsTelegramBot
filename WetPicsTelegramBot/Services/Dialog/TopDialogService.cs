using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ITopRatingService _topRatingService;

        private Dictionary<string, Func<Command, Task>> _commandHandlers;

        public TopDialogService(IDialogObserverService baseDialogService,
                                ILogger<TopDialogService> logger,
                                IMessagesService messagesService,
                                ICommandsService commandsService,
                                ITopRatingService topRatingService)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
            _commandsService = commandsService;
            _topRatingService = topRatingService;

            SetupCommandHandlers();
        }

        private void SetupCommandHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<Command, Task>>
            {
                {_commandsService.TopCommandText, OnNextTopCommand},
                {_commandsService.MyTopCommandText, OnNextMyTopCommand},
                {_commandsService.GlobalTopCommandText, OnNextGlobalTopCommand},
            };
        }

        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable
                .GroupBy(x => x.CommandName)
                .Where(group => _commandHandlers.ContainsKey(group.Key))
                .Subscribe(group => group.HandleAsync(_commandHandlers[group.Key]).Subscribe());
        }

        private async Task OnNextTopCommand(Command command)
        {
            try
            {
                _logger.TraceCommandReceived(_commandsService.TopCommandText);

                if (command.Message.ReplyToMessage == null)
                {
                    await _baseDialogService.Reply(command.Message, _messagesService.TopReplyToUser);
                    return;
                }

                var args = new TopRequestArgs(command.Message.Text);
                await _topRatingService.PostTop(command,
                                                TopSource.Reply, 
                                                user: command.Message.ReplyToMessage.From, 
                                                count: args.Count, 
                                                period: args.TopPeriod);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e, nameof(OnNextTopCommand));
            }
        }

        private async Task OnNextMyTopCommand(Command command)
        {
            try
            {
                _logger.TraceCommandReceived(_commandsService.MyTopCommandText);

                var args = new TopRequestArgs(command.Message.Text);
                await _topRatingService.PostTop(command,
                                                TopSource.My, 
                                                user: command.Message.From, 
                                                count: args.Count, 
                                                period: args.TopPeriod);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e, nameof(OnNextMyTopCommand));
            }
        }

        private async Task OnNextGlobalTopCommand(Command command)
        {
            try
            {
                _logger.TraceCommandReceived(_commandsService.GlobalTopCommandText);

                var args = new TopRequestArgs(command.Message.Text);
                await _topRatingService.PostTop(command,
                                                TopSource.Global, 
                                                count: args.Count, 
                                                period: args.TopPeriod);
            }
            catch (Exception e)
            {
                _logger.LogMethodError(e, nameof(OnNextGlobalTopCommand));
            }
        }
    }
}