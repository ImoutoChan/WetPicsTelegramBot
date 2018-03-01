using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Telegram.Bot.Types;
using WetPicsTelegramBot.WebApp.Models;

namespace WetPicsTelegramBot.WebApp.NotificationHandlers
{
    public class StartDialogHandler : DialogHelper
    {
        protected override bool WantHandle(Update update)
        {
            
        }

        protected override Task Handle(Update update, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }


    public abstract class DialogHelper : IDialogHandler
    {
        public async Task Handle(UpdateNotification notification, CancellationToken cancellationToken)
        {
            if (!WantHandle(notification.Update))
                return;

            await Handle(notification.Update, cancellationToken);
        }

        protected abstract bool WantHandle(Update update);

        protected abstract Task Handle(Update update, CancellationToken cancellationToken);
    }

    public interface IDialogHandler : INotificationHandler<Me>
    {
    }
}