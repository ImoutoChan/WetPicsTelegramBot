using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class Photo : EntityBase
    {
        [Required]
        public int FromUserId { get; set; }

        [Required]
        public long ChatId { get; set; }

        [Required]
        public int MessageId { get; set; }
    }
}