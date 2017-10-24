namespace WetPicsTelegramBot.Services.Abstract
{
    interface IMessagesService
    {
        string ActivateRepostMessage { get; }
        string HelpMessage { get; }
        string RepostHelpMessage { get; }
        string SelectPixivIntervalMessageF { get; }
        string SelectPixivModeMessage { get; }
        string DeactivatePhotoRepostMessage { get; }
        string RepostWrongIdFormat { get; }
        string RepostActivateTargetSuccess { get; }
        string RepostActivateSourceSuccess { get; }
        string RepostActivateSourceFailure { get; }
        string StatsReplyToUser { get; }
        string TopReplyToUser { get; }
        string StatsResultF { get; }
        string PixivWasDeactivated { get; }
        string PixivIncorrectMode { get; }
        string PixivIncorrectInterval { get; }
        string PixivWasActivated { get; }
        string ReplyToImage { get; }
        string IqdbNotFound { get; }
        string ChangeLogMessage { get; }
    }
}