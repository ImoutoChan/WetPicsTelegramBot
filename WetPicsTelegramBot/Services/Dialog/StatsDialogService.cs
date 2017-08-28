using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.Database;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    class StatsDialogService : IDialogService<StatsDialogService>
    {
        private readonly IBaseDialogService _baseDialogService;
        private readonly ILogger<StatsDialogService> _logger;
        private readonly IMessagesService _messagesService;
        private readonly ICommandsService _commandsService;
        private readonly IDbRepository _dbRepository;

        private Dictionary<string, Func<Command, Task>> _commandHandlers;

        public StatsDialogService(IBaseDialogService baseDialogService,
            ILogger<StatsDialogService> logger,
            IMessagesService messagesService,
            ICommandsService commandsService,
            IDbRepository dbRepository)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
            _commandsService = commandsService;
            _dbRepository = dbRepository;

            SetupCommandHandlers();
        }

        private void SetupCommandHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<Command, Task>>
            {
                {_commandsService.MyStatsCommandText, OnNextMyStatsCommand},
                {_commandsService.StatsCommandText, OnNextStatsCommand}
            };
        }

        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable.GroupBy(x => x.CommandName)
                .Where(group => _commandHandlers.ContainsKey(group.Key))
                .Subscribe(group => group.Subscribe(command => _commandHandlers[group.Key](command).Wait()));
        }

        private async Task OnNextStatsCommand(Command command)
        {
            _logger.LogTrace($"{_commandsService.StatsCommandText} command recieved");
            var message = command.Message;

            if (message.ReplyToMessage == null)
            {
                await _baseDialogService.Reply(message, _messagesService.StatsReplyToUser);
                return;
            }

            var user = message.ReplyToMessage.From;

            var result = await _dbRepository.GetStats(user.Id);

            await _baseDialogService.Reply(message, BuildGetStatMessage(user, result), parseMode: ParseMode.Html);
        }

        private async Task OnNextMyStatsCommand(Command command)
        {
            _logger.LogTrace($"{_commandsService.MyStatsCommandText} command recieved");
            var message = command.Message;

            var user = message.From;
            var result = await _dbRepository.GetStats(user.Id);

            await _baseDialogService.Reply(message, BuildGetStatMessage(user, result), parseMode: ParseMode.Html);
        }

        private string BuildGetStatMessage(User user, Stats result)
        {
            return String.Format((string) _messagesService.StatsResultF, 
                user.GetBeautyName(), 
                result.PicCount, 
                result.GetLikeCount, 
                result.SetLikeCount, 
                result.SetSelfLikeCount);
        }
    }
}