using System;
using System.Linq;
using System.Reactive.Linq;
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
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    internal class ImageRepostService : IImageRepostService
    {
        private readonly ILogger<ImageRepostService> _logger;
        private readonly ITelegramBotClient _api;
        private readonly IDbRepository _dbRepository;

        private readonly IRepostSettingsService _repostSettings;
        private readonly IMessagesObservableService _messagesObservableService;
        private readonly ICommandsService _commandsService;

        public ImageRepostService(ITelegramBotClient api, 
                                    ILogger<ImageRepostService> logger, 
                                    IDbRepository dbRepository, 
                                    IRepostSettingsService repostSettings,
                                    IMessagesObservableService messagesObservableService,
                                    ICommandsService commandsService)
        {
            _api = api;
            _logger = logger;
            _dbRepository = dbRepository;
            _repostSettings = repostSettings;
            _messagesObservableService = messagesObservableService;
            _commandsService = commandsService;

            SetupImageObserver();
            SetupCallbackObserver();
        }


        private void SetupCallbackObserver()
        {
            _messagesObservableService
                .BaseCallbackObservable
                .Where(IsLike)
                .HandleAsync(SetLike)
                .Subscribe();
        }

        private bool IsLike(CallbackQuery query) 
            => query.Data.StartsWith("vote_") && query.Data.Last() == 'l';

        private async Task SetLike(CallbackQuery query)
        {
            try
            {
                _logger.LogTrace($"Setting like (user: {query.From.Id} chat: {query.Message.Chat.Id} message: {query.Message.MessageId})");

                var isChanged = await _dbRepository.AddOrUpdateVote(query.From.Id, query.Message.Chat.Id, query.Message.MessageId);

                _logger.LogTrace($"Setting like (isChanged: {isChanged})");

                if (isChanged)
                {
                    var likesCount = await _dbRepository.GetVotes(query.Message.MessageId, query.Message.Chat.Id);

                    _logger.LogTrace($"Setting like (likes after set: {likesCount})");

                    await FlowExtensions.RepeatAsync(
                                                     async () =>
                                                     {
                                                         _logger.LogTrace($"Setting like (update likes count | chatId: {query.Message.Chat.Id} messageId: {query.Message.MessageId})");
                                                         await _api.EditMessageReplyMarkupAsync(query.Message.Chat.Id, query.Message.MessageId, GetPhotoKeyboard(likesCount));

                                                         // TODO find out delay (3s) reason
                                                         _logger.LogTrace($"Setting like (likes updated | chatId: {query.Message.Chat.Id} messageId: {query.Message.MessageId})");
                                                     },
                                                     ex => _logger.LogError(ex, $"Setting like (update likes count | retry)"),
                                                     ex => _logger.LogError(ex, $"Setting like (update likes count | error)"));
                }

                await _api.AnswerCallbackQueryAsync(query.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred in {nameof(SetLike)} method (user: {query.From.Id} chat: {query.Message.Chat.Id} message: {query.Message.MessageId})");
            }
        }


        private void SetupImageObserver()
        {
            _messagesObservableService
                .BaseObservable
                .Where(IsRepostNeeded)
                .Select(x => (RepostSettings: GetRepostSettings(x), Message: x))
                .Where(x => x.RepostSettings != null)
                .HandleAsync(RepostImage)
                .HandleException<object, Exception>(ex => _logger.LogError(ex, "Exception occurred in repost method"))
                .Subscribe();

        }

        private bool IsRepostNeeded(Message message) => message.Type == MessageType.PhotoMessage &&
                                                        message.Caption?.StartsWith(_commandsService.IgnoreCommand) != true;

        private async Task RepostImage((RepostSetting setting, Message message) input)
        {
            var (setting, message) = input;

            var file = await GetMessageFile(message);

            var sendedMessage =
                await SendMessageFile(setting.TargetId, file, message.From.GetBeautyName(), GetPhotoKeyboard(0));
            _logger.LogTrace("Image was sent");

            await _dbRepository.AddPhoto(message.From.Id, sendedMessage.Chat.Id, sendedMessage.MessageId);
            _logger.LogTrace("Image was saved");
        }

        private RepostSetting GetRepostSettings(Message message) 
            => _repostSettings.Settings.FirstOrDefault(x => x.ChatId == message.Chat.Id);

        private async Task<File> GetMessageFile(Message message)
            => await _api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);

        private async Task<Message> SendMessageFile(string targetId, File file, string username, InlineKeyboardMarkup keyboard)
            => await _api.SendPhotoAsync(targetId, new FileToSend(file.FileId), $"© {username}", replyMarkup: keyboard);

        
        public async Task PostToTargetIfExists(long chatId, string caption, string fileId, int fromUserId)
        {
            var settings = _repostSettings.Settings.FirstOrDefault(x => x.ChatId == chatId);

            if (settings == null)
                return;

            var file = await _api.GetFileAsync(fileId);

            var keyboard = GetPhotoKeyboard(0);

            var mes = await _api.SendPhotoAsync(settings.TargetId, new FileToSend(file.FileId), $"{caption}", replyMarkup: keyboard);

            await _dbRepository.AddPhoto(fromUserId, mes.Chat.Id, mes.MessageId);
        }


        private InlineKeyboardMarkup GetPhotoKeyboard(int likesCount) 
            => new InlineKeyboardMarkup(new[] { new[] { new InlineKeyboardCallbackButton($"❤️ ({likesCount})", "vote_l")} });
    }
}