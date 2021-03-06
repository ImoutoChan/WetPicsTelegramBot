﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers
{
    public class AutoRepostPhotoMessageHandler : MessageHandler
    {
        private readonly IRepostService _repostService;
        private readonly SemaphoreSlim _repostMessageSemaphoreSlim = new SemaphoreSlim(1);
        private static readonly CircleList<int> _lastRepostMessages = new CircleList<int>(10);

        public AutoRepostPhotoMessageHandler(ITgClient tgClient, 
                                             ICommandsProvider commandsProvider, 
                                             ILogger<AutoRepostPhotoMessageHandler> logger,
                                             IMessagesProvider messagesProvider,
                                             IRepostService repostService)
            : base(tgClient,
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
            _repostService = repostService;
        }


        protected override bool WantHandle(Message message, string command)
        {
            var isIgnore =
                   message
                     .Caption?
                     .StartsWith(CommandsProvider.IgnoreCommand, 
                                 StringComparison.OrdinalIgnoreCase) == true
                   || message
                     .Caption?
                     .StartsWith(CommandsProvider.AltIgnoreCommand, 
                                 StringComparison.OrdinalIgnoreCase) == true;

            if (isIgnore)
                return false;

            var isDesiredType = message.Type == MessageType.Photo
                || message.Type == MessageType.Document && message.Document.MimeType == "video/mp4"
                || message.Type == MessageType.Video;

            return isDesiredType;
        }

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var target = await _repostService.TryGetRepostTargetChat(message.Chat.Id);

            if (target == null)
                return;

            await RepostImage(target, message);
        }


        private async Task RepostImage(string targetId, Message message)
        {
            await _repostMessageSemaphoreSlim.WaitAsync();

            try
            {
                Logger.LogDebug($"Reposting image / ChatId: {message.Chat.Id} " +
                                $"/ TargetId: {targetId} / MessageId: {message.MessageId}");

                bool contains;
                lock (_lastRepostMessages)
                {
                    contains = _lastRepostMessages.Contains(message.MessageId);
                }

                if (contains)
                {
                    Logger.LogTrace("Already reposted");
                    return;
                }

                var caption = GetCaption(message.From.GetBeautyName(disableMention: true), message.Caption);

                await _repostService.RepostWithLikes(message, targetId, caption);

                lock (_lastRepostMessages)
                    _lastRepostMessages.Add(message.MessageId);
            }
            finally
            {
                _repostMessageSemaphoreSlim.Release();
            }
        }

        private static string GetCaption(string copyright, string sourceCaption)
        {
            const int maxCaptionLength = 200;

            var caption = $"© {copyright}";

            if (maxCaptionLength < caption.Length)
                return caption.Substring(0, 200);

            if (string.IsNullOrWhiteSpace(sourceCaption))
                return caption;

            var maxSafeLength = maxCaptionLength - 1 - caption.Length;

            var realSafeLength = maxSafeLength < sourceCaption.Length 
                ? maxSafeLength 
                : sourceCaption.Length;

            var captionPart = sourceCaption.Substring(0, realSafeLength);

            return $"{captionPart} {caption}";
        }
    }
}