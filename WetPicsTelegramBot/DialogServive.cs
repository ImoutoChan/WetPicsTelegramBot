using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace WetPicsTelegramBot
{
    class DialogServive
    {
        private readonly ILogger<DialogServive> _logger;
        private readonly IChatSettings _chatSettings;
        private readonly ITelegramBotClient _api;

        private User _me;
        private string _repostHelpMessage = $"Id может начинаться с @ для публичных каналов/чатов с заданным username. Для определения Id приватных получателей перейдите в web клиент, выберете нужного получателя.{Environment.NewLine}{Environment.NewLine}" +
                                             $"Вы увидете ссылки вида:{Environment.NewLine}{Environment.NewLine}" +
                                             $"web.telegram org/#/ im?p=<b>с00000000</b>_00000000000000000 для канала,{Environment.NewLine}" +
                                             $"web.telegram org/#/ im?p=<b>g00000000</b> для группы.{Environment.NewLine}{Environment.NewLine}" +
                                             $"Выделенная жирным часть и будет являться Id.";

        private string _helpMessage = $"Список доступных комманд:{Environment.NewLine}{Environment.NewLine}" +
                                       $"/activatePhotoRepost — включает репост фотографий из данного чата в выбранный канал или группу{Environment.NewLine}" +
                                       $"/deactivatePhotoRepost — отключает репост фотографий из данного чата";

        private string _activateRepostMessage = $"Введите Id канала/чата для репоста. Для корректной работы, бот должен быть администратором канала, либо должен состоять в выбранной группе.{Environment.NewLine}" +
                                                $"Пример Id: @channelName u00000000 с00000000 g00000000{Environment.NewLine}" +
                                                $"Справка: /activatePhotoRepostHelp";

        public DialogServive(ITelegramBotClient api, ILogger<DialogServive> logger, IChatSettings chatSettings)
        {
            _api = api;
            _logger = logger;
            _chatSettings = chatSettings;

            _api.OnMessage += BotOnMessageReceived;
        }

        private async Task<User> GetMe()
        {
            return _me ?? (_me = await _api.GetMeAsync());
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;

                if (message?.Type != MessageType.TextMessage)
                    return;

                var command = message.Text.Split('@').First();
                var me = await GetMe();

                if (command == "/help" || command == "/start")
                {
                    await ProcessHelpCommand(message);
                }
                else if (command == "/deactivatePhotoRepost")
                {
                    await ProcessDeactivatePhotoRepostCommand(message);
                }
                else if (command == "/activatePhotoRepost")
                {
                    await ProcessActivatePhotoRepostCommand(message);
                }
                else if (command == "/activatePhotoRepostHelp")
                {
                    await ProcessActivatePhotoRepostHelpCommand(message);
                }

                // if reply to me
                else if (me != null 
                    && message.ReplyToMessage?.From != null 
                    && (string) message.ReplyToMessage.From.Id == (string) me.Id)
                {
                    // if reply to activateRepost command
                    if (message.ReplyToMessage?.Text.StartsWith(_activateRepostMessage.Substring(0, 15)) ?? false)
                    {
                        await ProcessReplyToActivatePhotoRepostCommand(message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"unable to process message" + e.ToString());
            }
        }

        private async Task ProcessReplyToActivatePhotoRepostCommand(Message message)
        {
            _logger.LogTrace($"reply recieved {message.Text}");

            var fl = message.Text[0];

            switch (fl)
            {
                case '@':
                    await SetRepostId(message.Text, message);
                    break;
                case 'u':
                    await SetRepostId("-" + message.Text.Substring(1), message);
                    break;
                case 'g':
                    await SetRepostId("-" + message.Text.Substring(1), message);
                    break;
                case 'c':
                    await SetRepostId("-100" + message.Text.Substring(1), message);
                    break;
                default:
                    await _api.SendTextMessageAsync(message.Chat.Id,
                        "Неверный формат Id",
                        replyToMessageId: message.MessageId);
                    break;
            }
        }

        private async Task ProcessActivatePhotoRepostHelpCommand(Message message)
        {
            _logger.LogTrace("activatePhotoRepostHelp command recieved");
            
            var text = _repostHelpMessage;

            await _api.SendTextMessageAsync(message.Chat.Id,
                text,
                ParseMode.Html,
                replyToMessageId: message.MessageId);
        }

        private async Task ProcessActivatePhotoRepostCommand(Message message)
        {
            _logger.LogTrace("activatePhotoRepost command recieved");
            
            var text = _activateRepostMessage;

            await _api.SendTextMessageAsync(message.Chat.Id,
                text,
                replyMarkup: new ForceReply { Force = true },
                replyToMessageId: message.MessageId);
        }

        private async Task ProcessDeactivatePhotoRepostCommand(Message message)
        {
            _logger.LogTrace("deactivatePhotoRepost command recieved");

            await _chatSettings.Remove((string)message.Chat.Id);

            await _api.SendTextMessageAsync(message.Chat.Id,
                "Пересылка фотографий отключена.",
                replyToMessageId: message.MessageId);
        }

        private async Task ProcessHelpCommand(Message message)
        {
            _logger.LogTrace("help command recieved");
            
            var text = _helpMessage;

            await _api.SendTextMessageAsync(message.Chat.Id,
                text,
                ParseMode.Markdown,
                replyToMessageId: message.MessageId);
        }

        private async Task SetRepostId(string messageText, Message message)
        {
            try
            {
                var mes = await _api.SendTextMessageAsync(new ChatId(messageText), "Пересылка фотографий настроена.");

                await _chatSettings.Add((string) message.Chat.Id, messageText);

                await _api.SendTextMessageAsync(message.Chat.Id, "Пересылка фотографий включена.");
            }
            catch (Exception e)
            {
                try
                {
                    await _api.SendTextMessageAsync(message.Chat.Id,
                        "Не удается сохранить изменения. Нет доступа к каналу/группе или неверный формат Id.",
                        replyToMessageId: message.MessageId);
                    _logger.LogError($"unable to set repost id" + e.ToString());
                }
                catch (Exception exception)
                {
                    _logger.LogError($"unable to send repost id error" + exception.ToString());
                }
            }
        }
    }
}