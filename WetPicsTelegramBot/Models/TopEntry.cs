using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Models
{
    class TopEntry
    {
        public Photo Photo { get; set; }

        public int Likes { get; set; }
    }
}
