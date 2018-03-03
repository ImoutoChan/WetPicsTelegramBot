using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Data;
using WetPicsTelegramBot.WebApp.NotificationHandlers.Abstract;
using WetPicsTelegramBot.WebApp.Providers.Abstract;
using WetPicsTelegramBot.WebApp.Services.Abstract;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers
{
    public class SetLikeCallbackHandler : CallbackHandler
    {
        private readonly IDbRepository _dbRepository;

        public SetLikeCallbackHandler(ITgClient tgClient,
                                      ICommandsProvider commandsProvider,
                                      ILogger<SetLikeCallbackHandler> logger,
                                      IMessagesProvider messagesProvider,
                                      IDbRepository dbRepository)
            : base(tgClient, 
                   logger,
                   commandsProvider,
                   messagesProvider)
        {
            _dbRepository = dbRepository;
        }

        protected override bool WantHandle(CallbackQuery query)
            => query.Data == "vote_l";

        protected override async Task Handle(CallbackQuery query, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogTrace($"Setting like (user: {query.From.Id} " +
                                $"/ chat: {query.Message.Chat.Id} " +
                                $"/ message: {query.Message.MessageId})");

                var isChanged = await _dbRepository.AddOrUpdateVote(query.From.Id, 
                                                                    query.Message.Chat.Id, 
                                                                    query.Message.MessageId);

                Logger.LogTrace($"Setting like (isChanged: {isChanged})");

                if (isChanged)
                {
                    var likesCount = await _dbRepository.GetVotes(query.Message.MessageId, 
                                                                  query.Message.Chat.Id);

                    Logger.LogTrace($"Setting like (likes after set: {likesCount})");

                    var result 
                        = await Policy
                               .Handle<Exception>()
                               .RetryAsync(3, (ex, i) => Logger.LogError(ex,
                                                                         $"Setting like (update likes count / retry)"))
                               .ExecuteAndCaptureAsync(async () =>
                                {
                                    Logger.LogTrace($"Setting like (update likes count " +
                                                    $"/ chatId: {query.Message.Chat.Id} " +
                                                    $"/ messageId: {query.Message.MessageId})");

                                    await TgClient.Client.EditMessageReplyMarkupAsync(query.Message.Chat.Id,
                                                                                      query.Message.MessageId,
                                                                                      TgClient.GetPhotoKeyboard(likesCount),
                                                                                      cancellationToken);

                                    // TODO find out delay (3s) reason
                                    Logger.LogTrace($"Setting like (likes updated " +
                                                    $"/ chatId: {query.Message.Chat.Id} " +
                                                    $"/ messageId: {query.Message.MessageId})");
                                });

                    if (result.FinalException != null)
                    {
                        Logger.LogError(result.FinalException, $"Setting like (update likes count / error)");
                    }
                }

                await TgClient.Client.AnswerCallbackQueryAsync(query.Id, cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error occurred in {nameof(Handle)} method " +
                                   $"(user: {query.From.Id} " +
                                   $"/ chat: {query.Message.Chat.Id} " +
                                   $"/ message: {query.Message.MessageId})");
            }
        }
    }
}