using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
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
        private readonly ITopImageDrawService _topImageDrawService;

        private Dictionary<string, Func<Command, Task>> _commandHandlers;

        public TopDialogService(IDialogObserverService baseDialogService,
                                ILogger<TopDialogService> logger,
                                IMessagesService messagesService,
                                ICommandsService commandsService,
                                IDbRepository dbRepository,
                                ITelegramBotClient telegramApi,
                                ITopImageDrawService topImageDrawService)
        {
            _baseDialogService = baseDialogService;
            _logger = logger;
            _messagesService = messagesService;
            _commandsService = commandsService;
            _dbRepository = dbRepository;
            _telegramApi = telegramApi;
            _topImageDrawService = topImageDrawService;

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

        private async Task PostTop(Command command, TopSource topSource = TopSource.Reply, int count = 5)
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

            var results = await _dbRepository.GetTopSlow(user?.Id, count);

            messageText.AppendLine();

            int counter = 1;
            foreach (var topEntry in results)
            {
                messageText.AppendLine($"{counter++}. Лайков: {topEntry.Likes}");
            }

            messageText.AppendLine();
            messageText.AppendLine("Нажав на кнопку ниже, вы можете запросить форвард изображения.");

            var topPhotots = results.Select(x => x.Photo).ToList();

            var fileStreams = new List<MemoryStream>();
            foreach (var topPhoto in topPhotots)
            {
                var photoMessage = new Message {MessageId = topPhoto.MessageId};
                photoMessage.Chat = new Chat {Id = topPhoto.ChatId};

                var replyToPhotomessage = await _telegramApi.Reply(photoMessage, "Сорян, грузим пикчу...");

                try
                {
                    var photos = replyToPhotomessage.ReplyToMessage.Photo;

                    if (photos.Any())
                    {
                        var photo = photos.Last();

                        var ms = new MemoryStream();
                        await _telegramApi.GetFileAsync(photo.FileId, ms);

                        fileStreams.Add(ms);
                    }
                }
                finally
                {
                    await _telegramApi.DeleteMessageAsync(replyToPhotomessage.Chat.Id, replyToPhotomessage.MessageId);
                }
            }

            if (fileStreams.Any())
            {

                var stream =
                    await Task.Run(() => _topImageDrawService.DrawTopImage(fileStreams.Cast<Stream>().ToList()));
                stream.Seek(0, SeekOrigin.Begin);

                var inlineKeyboardMarkup = GetReplyKeyboardMarkup(topPhotots.Select(x => (x.ChatId, x.MessageId)));
                await _telegramApi.SendPhotoAsync(command.Message.Chat.Id,
                                                  new FileToSend("name", stream),
                                                  messageText.ToString(),
                                                  replyToMessageId: message.MessageId,
                                                  replyMarkup: inlineKeyboardMarkup);

                stream.Dispose();
            }

            fileStreams.ForEach(x => x.Dispose());
        }

        public InlineKeyboardMarkup GetReplyKeyboardMarkup(IEnumerable<(long ChatId, int MessageId)> messagesEnumerable)
        {
            var messages = messagesEnumerable as IList<(long ChatId, int MessageId)> ?? messagesEnumerable.ToList();
            int counter = 0;

            var buttons = messages
                .Select(x 
                    => new InlineKeyboardCallbackButton($"{++counter}", 
                                                        $"forward_request|chatId#{x.ChatId}_messageId#{x.MessageId}"))
                .ToList();
            
            return new InlineKeyboardMarkup(buttons.ToArray());
        }
    }
}