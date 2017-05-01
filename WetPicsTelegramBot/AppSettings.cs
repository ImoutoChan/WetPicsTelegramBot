namespace WetPicsTelegramBot
{
    public class AppSettings
    {
        public string BotToken { get; set; }

        public string ConnectionString { get; set; }

        public Pixivconfiguration PixivConfiguration { get; set; }
    }

    public class Pixivconfiguration
    {
        public string Login { get; set; }

        public string Password { get; set; }
    }

}
