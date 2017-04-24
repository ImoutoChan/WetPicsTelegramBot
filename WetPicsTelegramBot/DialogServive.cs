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
        ILogger Logger { get; } = ApplicationLogging.CreateLogger<DialogServive>();

        private readonly TelegramBotClient _api;
        private User _me;

        public DialogServive(TelegramBotClient api)
        {
            _api = api;

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
                if (message == null)
                    return;

                if (message.Type != MessageType.TextMessage)
                    return;

                var command = message.Text.Split('@').First();
                var me = await GetMe();
                if (command == "/help" || command == "/start")
                {
                    Logger.LogTrace("help command recieved");

                    var text = "Список доступных комманд:" + Environment.NewLine + Environment.NewLine +
                               "/activatePhotoRepost — включает репост фотографий из данного чата в выбранный канал или группу" +
                               Environment.NewLine +
                               "/deactivatePhotoRepost — отключает репост фотографий из данного чата";
                    //Environment.NewLine +
                    //"/removeLast — удаляет репост последнего запощенного вами изображения (В разработке)" +
                    //Environment.NewLine +
                    //"/showStats — показывает статистику по пользователям (В разработке)" +
                    //Environment.NewLine;

                    await _api.SendTextMessageAsync(message.Chat.Id,
                        text,
                        ParseMode.Markdown,
                        replyToMessageId: message.MessageId);
                }
                else if (command == "/deactivatePhotoRepost")
                {
                    Logger.LogTrace("deactivatePhotoRepost command recieved");

                    await DbRepository.Instance.RemoveChatSettings((string) message.Chat.Id);

                    await _api.SendTextMessageAsync(message.Chat.Id,
                        "Пересылка фотографий отключена.",
                        replyToMessageId: message.MessageId);
                }
                else if (command == "/activatePhotoRepost")
                {
                    Logger.LogTrace("activatePhotoRepost command recieved");

                    var text =
                        "Введите Id канала/чата для репоста. Для корректной работы, бот должен быть администратором канала, либо должен состоять в выбранной группе." +
                        Environment.NewLine +
                        "Пример Id: @channelName u00000000 с00000000 g00000000" + Environment.NewLine +
                        "Справка: /activatePhotoRepostHelp";

                    await _api.SendTextMessageAsync(message.Chat.Id,
                        text,
                        replyMarkup: new ForceReply {Force = true},
                        replyToMessageId: message.MessageId);
                }
                else if (command == "/activatePhotoRepostHelp")
                {
                    Logger.LogTrace("activatePhotoRepostHelp command recieved");

                    var text = "Id может начинаться с @ для публичных каналов/чатов с заданным username. " +
                               "Для определения Id приватных получателей перейдите в web клиент, выберете нужного получателя." +
                               Environment.NewLine + Environment.NewLine +
                               "Вы увидете ссылки вида:" + Environment.NewLine + Environment.NewLine +
                               //"web.telegram org/#/ im?p=<b>u00000000</b>_00000000000000000 для пользователя, " + Environment.NewLine +
                               "web.telegram org/#/ im?p=<b>с00000000</b>_00000000000000000 для канала," +
                               Environment.NewLine +
                               "web.telegram org/#/ im?p=<b>g00000000</b> для группы." + Environment.NewLine +
                               Environment.NewLine +
                               "Выделенная жирным часть и будет являться Id.";

                    await _api.SendTextMessageAsync(message.Chat.Id,
                        text,
                        ParseMode.Html,
                        replyToMessageId: message.MessageId);
                }
                else if (me != null && message.ReplyToMessage?.From != null && (string) message.ReplyToMessage.From.Id == (string) me.Id)
                {
                    if (message.ReplyToMessage?.Text.StartsWith("Введите Id к") ?? false)
                    {
                        Logger.LogTrace($"reply recieved {message.Text}");

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
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"unable to process message"+ e.ToString());
            }
        }

        private async Task SetRepostId(string messageText, Message message)
        {
            try
            {
                var mes = await _api.SendTextMessageAsync(new ChatId(messageText), "Пересылка фотографий настроена.");

                await DbRepository.Instance.SetChatSettings((string) message.Chat.Id, messageText);

                await _api.SendTextMessageAsync(message.Chat.Id, "Пересылка фотографий включена.");
            }
            catch (Exception e)
            {
                try
                {
                    await _api.SendTextMessageAsync(message.Chat.Id,
                        "Неверный формат Id",
                        replyToMessageId: message.MessageId);
                    Logger.LogError($"unable to set repost id"+ e.ToString());
                }
                catch (Exception exception)
                {
                    Logger.LogError($"unable to send repost id error" + exception.ToString());
                }
            }
        }
    }
}
