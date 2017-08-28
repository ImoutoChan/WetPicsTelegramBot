namespace WetPicsTelegramBot.Services.Abstract
{
    interface IMessagesService
    {
        string ActivateRepostMessage { get; }
        string HelpMessage { get; }
        string RepostHelpMessage { get; }
        string SelectPixivIntervalMessage { get; }
        string SelectPixivModeMessage { get; }
        string DeactivatePhotoRepostMessage { get; }
        string RepostWrongIdFormat { get; }
        string RepostActivateTargetSuccess { get; }
        string RepostActivateSourceSuccess { get; }
        string RepostActivateSourceFailure { get; }
        string StatsReplyToUser { get; }
        string StatsResult { get; }
        string PixivWasDeactivated { get; }
    }
}