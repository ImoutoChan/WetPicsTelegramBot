namespace WetPicsTelegramBot.Services
{
    interface IMessagesService
    {
        string ActivatePhotoRepostCommandText { get; }
        string ActivatePhotoRepostHelpCommandText { get; }
        string ActivatePixivCommandText { get; }
        string ActivateRepostMessage { get; }
        string DeactivatePhotoRepostCommandText { get; }
        string DeactivatePixivCommandText { get; }
        string HelpCommandText { get; }
        string HelpMessage { get; }
        string MyStatsCommandText { get; }
        string RepostHelpMessage { get; }
        string SelectPixivIntervalMessage { get; }
        string SelectPixivModeMessage { get; }
        string StartCommandText { get; }
        string StatsCommandText { get; }
    }
}