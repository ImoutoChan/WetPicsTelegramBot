using System.Collections.Generic;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    class DialogServiceInitializer : IDialogServiceInitializer
    {
        private readonly List<IDialogService> _dialogServices = new List<IDialogService>();

        public DialogServiceInitializer(IDialogService<HelpDialogService> helpDialogService,
                                        IDialogService<RepostDialogService> repostDialogService,
                                        IDialogService<StatsDialogService> statsDialogService,
                                        IDialogService<PixivDialogService> pixivDialogService,
                                        IDialogService<TopDialogService> topDialogService,
                                        IDialogService<IqdbDialogService> iqdbDialogService)
        {
            _dialogServices.Add(helpDialogService);
            _dialogServices.Add(repostDialogService);
            _dialogServices.Add(statsDialogService);
            _dialogServices.Add(pixivDialogService);
            _dialogServices.Add(topDialogService);
            _dialogServices.Add(iqdbDialogService);
        }

        public void Subscribe()
        {
            foreach (var dialogService in _dialogServices)
            {
                dialogService.Subscribe();
            }
        }
    }
}
