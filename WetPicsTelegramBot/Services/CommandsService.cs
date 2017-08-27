using WetPicsTelegramBot.Services.Abstract;

namespace WetPicsTelegramBot.Services
{
    class CommandsService : ICommandsService
    {
        public string HelpCommandText => "/help";

        public string StartCommandText => "/start";

        public string DeactivatePhotoRepostCommandText => "/deactivatePhotoRepost";

        public string ActivatePhotoRepostCommandText => "/activatePhotoRepost";

        public string ActivatePhotoRepostHelpCommandText => "/activatePhotoRepostHelp";

        public string MyStatsCommandText => "/mystats";

        public string StatsCommandText => "/stats";

        public string ActivatePixivCommandText => "/pixivon";

        public string DeactivatePixivCommandText => "/pixivoff";
    }
}