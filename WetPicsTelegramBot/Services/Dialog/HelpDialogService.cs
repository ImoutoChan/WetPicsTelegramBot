using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    internal class HelpDialogService : IDialogService<HelpDialogService>
    {
        private readonly IDialogObserverService _baseDialogService;
        private readonly ILogger<HelpDialogService> _logger;
        private readonly IMessagesService _messagesService;
        private readonly ICommandsService _commandsService;

        public HelpDialogService(IDialogObserverService baseDialogService,
            ILogger<HelpDialogService> logger,
            IMessagesService messagesService,
            ICommandsService commandsService)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
            _commandsService = commandsService;
        }
        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable
                .Where(x => x.CommandName == _commandsService.HelpCommandText
                            || x.CommandName == _commandsService.StartCommandText)
                .HandleAsync(OnNextCommand)
                .Subscribe();
        }

        private async Task OnNextCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");

            var text = _messagesService.HelpMessage;

            await _baseDialogService.Reply(command.Message, text, ParseMode.Markdown);
        }
    }
}