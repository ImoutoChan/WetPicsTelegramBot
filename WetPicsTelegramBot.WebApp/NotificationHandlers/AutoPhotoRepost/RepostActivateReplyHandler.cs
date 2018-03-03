using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Helpers;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers.AutoPhotoRepost
{
    public class RepostActivateReplyHandler : MessageHandler
    {
        private readonly IRepostSettingsService _repostSettingsService;

        public RepostActivateReplyHandler(ITgClient tgClient,
                                          ILogger<RepostActivateReplyHandler> logger,      
                                          ICommandsProvider commandsProvider,
                                          IMessagesProvider messagesProvider,
                                          IRepostSettingsService repostSettingsService)
            : base(tgClient,
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
            _repostSettingsService = repostSettingsService;
        }

        protected override bool WantHandle(Message message, string command)
            => IsReplyToActivateRepost(message);

        protected override async Task Handle(Message message,
                                             string command,
                                             CancellationToken cancellationToken)
        {
            var targetId = BuildId(message.Text);

            if (targetId == null)
            {
                await TgClient.Reply(message,
                                     MessagesProvider.RepostWrongIdFormat,
                                     cancellationToken);
            }

            var hasRights = await TgClient.CheckOnAdmin(targetId, message.From.Id);

            if (!hasRights)
            {
                Logger.LogError($"Set repost was aborted. " +
                                $"User {message.From.GetBeautyName()} must be admin in target chat.");

                await TgClient.Reply(message, 
                                     MessagesProvider.RepostActivateTargetRestrict, 
                                     cancellationToken);
                return;
            }

            try
            {
                await TgClient.Client.SendTextMessageAsync(targetId, 
                                                           MessagesProvider.RepostActivateTargetSuccess, 
                                                           cancellationToken: cancellationToken);
                
                await _repostSettingsService.Add(message.Chat.Id, targetId);

                await TgClient.Client.SendTextMessageAsync(message.Chat.Id, 
                                                           MessagesProvider.RepostActivateSourceSuccess, 
                                                           cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                try
                {
                    await TgClient.Reply(message, 
                                         MessagesProvider.RepostActivateSourceFailure, 
                                         cancellationToken);

                    Logger.LogMethodError(e);
                }
                catch (Exception eInner)
                {
                    Logger.LogError(eInner, $"Error occurred while sending error report");
                }
            }
        }
        private bool IsReplyToActivateRepost(Message x)
            => x.ReplyToMessage?.Text.RemoveWhiteSpaces().Substring(0, 10) == MessagesProvider.ActivateRepostMessage.Message.RemoveWhiteSpaces().Substring(0, 10);

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
                case 'u':
                    idString = inputId.Substring(1);
                    break;
                case 'g':
                    idString = "-" + inputId.Substring(1);
                    break;
                case 'c':
                    idString = "-100" + inputId.Substring(1);
                    break;
                case '@':
                default:
                    idString = inputId.Trim();
                    break;
            }


            return idString;
        }
    }
}