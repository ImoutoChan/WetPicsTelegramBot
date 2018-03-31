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
            _logger.LogTrace("Receiving file");
            var file = await GetMessageFile(message);

            _logger.LogTrace("Sending file");
            var sendedMessage = await SendMessageFile(targetId,
                                                      file,
                                                      caption,
                                                      _tgClient.GetPhotoKeyboard(0));
            _logger.LogTrace("Image was sent");

            await _dbRepository.AddPhoto(message.From.Id,
                                         sendedMessage.Chat.Id,
                                         sendedMessage.MessageId);
            _logger.LogTrace("Image was saved");
        }

        private async Task<File> GetMessageFile(Message message)
            => await _tgClient.Client.GetFileAsync(message.Photo.LastOrDefault()?.FileId);

        private async Task<Message> SendMessageFile(string targetId,
                                                    File file,
                                                    string caption,
                                                    InlineKeyboardMarkup keyboard)
            => await _tgClient.Client.SendPhotoAsync(targetId,
                                                    new InputOnlineFile(file.FileId),
                                                    caption,
                                                    ParseMode.Html,
                                                    replyMarkup: keyboard, 
                                                    disableNotification: true);
    }
}
