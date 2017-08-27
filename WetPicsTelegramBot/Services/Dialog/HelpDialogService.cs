using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    internal class HelpDialogService
    {
        private readonly IBaseDialogService _baseDialogService;
        private readonly ILogger<HelpDialogService> _logger;
        private readonly IMessagesService _messagesService;
        
        public HelpDialogService(IBaseDialogService baseDialogService,
            ILogger<HelpDialogService> logger,
            IMessagesService messagesService)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
        }

        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable
                .Where(x => x.CommandName == _messagesService.HelpCommandText
                            || x.CommandName == _messagesService.StartCommandText)
                .Subscribe(command => OnNextCommand(command).Start());
        }

        private async Task OnNextCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");

            var text = _messagesService.HelpMessage;

            await _baseDialogService.Reply(command.Message, text, ParseMode.Markdown);
        }
    }
}