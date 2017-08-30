using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Models;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    internal class RepostDialogService : IDialogService<RepostDialogService>
    {
        private readonly IDialogObserverService _baseDialogService;
        private readonly ILogger<RepostDialogService> _logger;
        private readonly IMessagesService _messagesService;
        private readonly ICommandsService _commandsService;
        private readonly IRepostSettingsService _chatSettings;
        private readonly ITelegramBotClient _telegramApi;

        private Dictionary<string, Func<Command, Task>> _commandHandlers;

        public RepostDialogService(IDialogObserverService baseDialogService,
                                            ILogger<RepostDialogService> logger,
                                            IMessagesService messagesService,
                                            ICommandsService commandsService,
                                            IRepostSettingsService chatSettings,
                                            ITelegramBotClient telegramApi)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
            _commandsService = commandsService;
            _chatSettings = chatSettings;
            _telegramApi = telegramApi;

            SetupCommandHandlers();
        }

        private void SetupCommandHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<Command, Task>>
            {
                {_commandsService.ActivatePhotoRepostCommandText, OnNextActivateRepostCommand},
                {_commandsService.ActivatePhotoRepostHelpCommandText, OnNextRepostHelpCommand},
                {_commandsService.DeactivatePhotoRepostCommandText, OnNextDeactivateRepostCommand},
            };
        }

        public void Subscribe()
        {
            _baseDialogService
                .MessageObservable
                .GroupBy(x => x.CommandName)
                .Where(group => _commandHandlers.ContainsKey(group.Key))
                .Subscribe(group => group.HandleAsync(_commandHandlers[group.Key]).Subscribe());

            _baseDialogService
                .RepliesObservable
                .Where(IsReplyToActivateRepost)
                .HandleAsync(OnNextActivateReply)
                .Subscribe();
        }

        private bool IsReplyToActivateRepost(Message x) 
            => x.ReplyToMessage.Text.RemoveWhiteSpaces() == _messagesService.ActivateRepostMessage.RemoveWhiteSpaces();

        private async Task OnNextActivateReply(Message message)
        {
            _logger.LogTrace($"\"{message.Text}\" reply recieved");

            var targetId = BuildId(message.Text);

            if (targetId == null)
            {
                await _baseDialogService.Reply(message, _messagesService.RepostWrongIdFormat);
            }
            else
            {
                await SetRepostId(targetId, message);
            }
        }

        private string BuildId(string inputId)
        {
            if (String.IsNullOrWhiteSpace(inputId))
            {
                return null;
            }

            var firstLetter = inputId[0];
            string idString;
            switch (firstLetter)
            {
                case '@':
                    idString = inputId;
                    break;
                case 'u':
                case 'g':
                    idString = "-" + inputId.Substring(1);
                    break;
                case 'c':
                    idString = "-100" + inputId.Substring(1);
                    break;
                default:
                    return null;
            }
            

            return idString;
        }

        private async Task SetRepostId(string targetChatId, Message message)
        {
            try
            {
                await _telegramApi.SendTextMessageAsync(targetChatId, _messagesService.RepostActivateTargetSuccess);

                await _chatSettings.Add(message.Chat.Id, targetChatId);

                await _telegramApi.SendTextMessageAsync(message.Chat.Id, _messagesService.RepostActivateSourceSuccess);
            }
            catch (Exception e)
            {
                try
                {
                    await _baseDialogService.Reply(message, _messagesService.RepostActivateSourceFailure);
                    _logger.LogError(e, $"Error occured in {nameof(SetRepostId)} method");
                }
                catch (Exception e1)
                {
                    _logger.LogError(e1, $"Error occured in {nameof(SetRepostId)} method while sending error report");
                }
            }
        }

        private async Task OnNextRepostHelpCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");
            var text = _messagesService.RepostHelpMessage;

            await _baseDialogService.Reply(command.Message, text, ParseMode.Html);
        }

        private async Task OnNextDeactivateRepostCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");

            await _chatSettings.Remove(command.Message.Chat.Id);

            var text = _messagesService.DeactivatePhotoRepostMessage;

            await _baseDialogService.Reply(command.Message, text);
        }

        private async Task OnNextActivateRepostCommand(Command command)
        {
            _logger.LogTrace($"{command.CommandName} command recieved");

            var text = _messagesService.ActivateRepostMessage;

            await _baseDialogService.Reply(command.Message, text, replyMarkup: new ForceReply { Force = true });
        }
    }
}