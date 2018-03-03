using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers
{
    public class AutoRepostPhotoMessageHandler : MessageHandler
    {
        private readonly IRepostSettingsService _repostSettingsService;
        private readonly IDbRepository _dbRepository;
        private readonly SemaphoreSlim _repostMessageSemaphoreSlim = new SemaphoreSlim(1);
        private readonly CircleList<int> _lastRepostMessages = new CircleList<int>(10);

        public AutoRepostPhotoMessageHandler(ITgClient tgClient, 
                                             ICommandsProvider commandsProvider, 
                                             ILogger<AutoRepostPhotoMessageHandler> logger,
                                             IMessagesProvider messagesProvider,
                                             IRepostSettingsService repostSettingsService,
                                             IDbRepository dbRepository)
            : base(tgClient,
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
            _repostSettingsService = repostSettingsService;
            _dbRepository = dbRepository;
        }


        protected override bool WantHandle(Message message, string command)
        {
            return message.Type == MessageType.Photo

                   && message
                     .Caption?
                     .StartsWith(CommandsProvider.IgnoreCommand, 
                                 StringComparison.OrdinalIgnoreCase) != true
                   && message
                     .Caption?
                     .StartsWith(CommandsProvider.AltIgnoreCommand, 
                                 StringComparison.OrdinalIgnoreCase) != true;
        }

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var allSettings = await _repostSettingsService.GetSettings();
            var settings = allSettings.FirstOrDefault(x => x.ChatId == message.Chat.Id);

            if (settings == null)
            {
                return;
            }

            await RepostImage(settings, message);
        }


        private async Task RepostImage(RepostSetting setting, Message message)
        {
            await _repostMessageSemaphoreSlim.WaitAsync();

            try
            {
                Logger.LogDebug($"Reposting image / ChatId: {setting.ChatId} " +
                                $"/ TargetId: {setting.TargetId} / MessageId: {message.MessageId}");

                if (_lastRepostMessages.Contains(message.MessageId))
                {
                    Logger.LogTrace("Already reposted");
                    return;
                }


                Logger.LogTrace("Receiving file");
                var file = await GetMessageFile(message);

                Logger.LogTrace("Sending file");
                var sendedMessage = await SendMessageFile(setting.TargetId, 
                                                          file, 
                                                          message.From.GetBeautyName(true), 
                                                          TgClient.GetPhotoKeyboard(0));
                Logger.LogTrace("Image was sent");

                await _dbRepository.AddPhoto(message.From.Id, 
                                             sendedMessage.Chat.Id, 
                                             sendedMessage.MessageId);
                Logger.LogTrace("Image was saved");

                _lastRepostMessages.Add(message.MessageId);
            }
            finally
            {
                _repostMessageSemaphoreSlim.Release();
            }
        }

        private async Task<File> GetMessageFile(Message message)
            => await TgClient.Client.GetFileAsync(message.Photo.LastOrDefault()?.FileId);

        private async Task<Message> SendMessageFile(string targetId, 
                                                    File file, 
                                                    string username, 
                                                    InlineKeyboardMarkup keyboard)
            => await TgClient.Client.SendPhotoAsync(targetId, 
                                                    new InputOnlineFile(file.FileId), 
                                                    $"© {username}", 
                                                    replyMarkup: keyboard);
    }
}