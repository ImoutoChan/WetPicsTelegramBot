﻿namespace WetPicsTelegramBot.Services.Abstract
{
    interface ICommandsService
    {
        string ActivatePhotoRepostCommandText { get; }
        string ActivatePhotoRepostHelpCommandText { get; }
        string ActivatePixivCommandText { get; }
        string DeactivatePhotoRepostCommandText { get; }
        string DeactivatePixivCommandText { get; }
        string HelpCommandText { get; }
        string MyStatsCommandText { get; }
        string StartCommandText { get; }
        string StatsCommandText { get; }
        string IgnoreCommand { get; }
        string TopCommandText { get; }
        string MyTopCommandText { get; }
        string GlobalTopCommandText { get; }
        string MyTopSCommandText { get; }
        string TopSCommandText { get; }
        string GlobalTopSCommandText { get; }
    }
}