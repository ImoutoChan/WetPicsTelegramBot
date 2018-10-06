using WetPicsTelegramBot.WebApp.Models.AppSettings;

namespace WetPicsTelegramBot.WebApp
{
    public class AppSettings
    {
        public string BotToken { get; set; }

        public string ConnectionString { get; set; }

        public Pixivconfiguration PixivConfiguration { get; set; }

        public SankakuConfiguration SankakuConfiguration { get; set; }

        public DanbooruConfiguration DanbooruConfiguration { get; set; }

        public string WebHookAdress { get; set; }
    }
}