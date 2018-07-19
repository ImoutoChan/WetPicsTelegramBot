using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.Services
{
    public class RepostService : IRepostService
    {
        private readonly ILogger<RepostService> _logger;
        private readonly ITgClient _tgClient;
        private readonly IDbRepository _dbRepository;
        private readonly IRepostSettingsService _repostSettingsService;

        public RepostService(ILogger<RepostService> logger, 
                             ITgClient tgClient, 
                             IDbRepository dbRepository, 
                             IRepostSettingsService repostSettingsService)
        {
            _logger = logger;
            _tgClient = tgClient;
            _dbRepository = dbRepository;
            _repostSettingsService = repostSettingsService;
        }

        public async Task<string> TryGetRepostTargetChat(long forChatId)
        {
            var allSettings = await _repostSettingsService.GetSettings();
            return allSettings.FirstOrDefault(x => x.ChatId == forChatId)?.TargetId;
        }

        public async Task RepostWithLikes(Message message, string targetId, string caption)
        {
            if (message.Photo?.Any() == true)
            {
                _logger.LogTrace("Sending photo file");
                var sendedMessage = await SendPhoto(targetId,
                                                      message.Photo.Last()?.FileId,
                                                      caption,
                                                      _tgClient.GetPhotoKeyboard(0));
                _logger.LogTrace("Image was sent");

                await _dbRepository.AddPhoto(message.From.Id,
                                             sendedMessage.Chat.Id,
                                             sendedMessage.MessageId);
                _logger.LogTrace("Image was saved");
            }
            else if (message.Document != null)
            {
                _logger.LogTrace("Sending doc file");
                var sendedMessage = await SendDocument(targetId,
                                                          message.Document.FileId,
                                                          caption,
                                                          _tgClient.GetPhotoKeyboard(0));
                _logger.LogTrace("Doc was sent");

                await _dbRepository.AddPhoto(message.From.Id,
                                             sendedMessage.Chat.Id,
                                             sendedMessage.MessageId);
                _logger.LogTrace("Doc was saved");
            }
            else if (message.Video != null)
            {
                _logger.LogTrace("Sending video file");
                var sendedMessage = await SendVideo(targetId,
                                                    message.Video.FileId,
                                                    caption,
                                                    _tgClient.GetPhotoKeyboard(0));
                _logger.LogTrace("Video file was sent");

                await _dbRepository.AddPhoto(message.From.Id,
                                             sendedMessage.Chat.Id,
                                             sendedMessage.MessageId);
                _logger.LogTrace("Video file was saved");
            }
        }

        private async Task<Message> SendPhoto(string targetId,
                                                string fileId,
                                                string caption,
                                                InlineKeyboardMarkup keyboard)
            => await _tgClient.Client.SendPhotoAsync(targetId,
                                                        new InputOnlineFile(fileId),
                                                        caption,
                                                        ParseMode.Html,
                                                        replyMarkup: keyboard, 
                                                        disableNotification: true);

        private async Task<Message> SendDocument(string targetId,
                                                    string fileId,
                                                    string caption,
                                                    InlineKeyboardMarkup keyboard)
            => await _tgClient.Client.SendDocumentAsync(targetId,
                                                        new InputOnlineFile(fileId),
                                                        caption,
                                                        ParseMode.Html,
                                                        replyMarkup: keyboard, 
                                                        disableNotification: true);

        private async Task<Message> SendVideo(string targetId,
                                                string fileId,
                                                string caption,
                                                InlineKeyboardMarkup keyboard)
            => await _tgClient.Client.SendVideoAsync(targetId,
                                                     new InputOnlineFile(fileId),
                                                     caption: caption,
                                                     parseMode: ParseMode.Html,
                                                     replyMarkup: keyboard, 
                                                     disableNotification: true);
    }
}
