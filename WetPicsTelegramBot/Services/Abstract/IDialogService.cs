namespace WetPicsTelegramBot.Services.Abstract
{
    interface IDialogService<T> : IDialogService
    {
    }

    interface IDialogService
    {
        void Subscribe();
    }
}