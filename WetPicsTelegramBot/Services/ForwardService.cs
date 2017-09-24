using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using WetPicsTelegramBot.Helpers;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    class ForwardService : IForwardService
    {
        private readonly IMessagesObservableService _messagesObservableService;
        private readonly ITelegramBotClient _telegramApi;

        public ForwardService(IMessagesObservableService messagesObservableService,
                             ITelegramBotClient telegramApi)
        {
            _messagesObservableService = messagesObservableService;
            _telegramApi = telegramApi;
            SetupCallbackObserver();
        }

        private void SetupCallbackObserver()
        {
            _messagesObservableService
                .BaseCallbackObservable
                .Where(IsRepost)
                .HandleAsync(Forward)
                .Subscribe();
        }

        private async Task Forward(CallbackQuery arg)
        {
            var forwardsParts = arg.Data.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);


            var forward = forwardsParts.Last();
                
            var repostParts = forward.Split(new[] {'_'});

            var chatIdString = repostParts.First().Split(new[] {'#'}).Last();
            var messageIdString = repostParts.Last().Split(new[] { '#' }).Last();

            var chatId = Int64.Parse(chatIdString);
            var messageId = Int32.Parse(messageIdString);

            await _telegramApi.ForwardMessageAsync(arg.Message.Chat.Id, chatId, messageId);

            await _telegramApi.AnswerCallbackQueryAsync(arg.Id);
        }

        private bool IsRepost(CallbackQuery query) => query.Data.StartsWith("forward_request");
    }
}
