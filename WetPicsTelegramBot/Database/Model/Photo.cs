using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }

        public string FromUserId { get; set; }

        public string ChatId { get; set; }

        public int MessageId { get; set; }
    }
}