using WetPicsTelegramBot.Data.Entities;

namespace WetPicsTelegramBot.Data.Models
{
    public class TopEntry
    {
        public Photo Photo { get; set; }

        public int Likes { get; set; }

        public ChatUser User { get; set; }
    }
}
