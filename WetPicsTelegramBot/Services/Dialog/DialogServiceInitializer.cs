using System.Collections.Generic;
using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services.Dialog
{
    class DialogServiceInitializer : IDialogServiceInitializer
    {
        private readonly List<IDialogService> _dialogServices = new List<IDialogService>();

        public DialogServiceInitializer(IDialogService<HelpDialogService> helpDialogService)
        {
            _dialogServices.Add(helpDialogService);
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
