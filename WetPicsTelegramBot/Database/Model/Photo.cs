using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FromUserId { get; set; }

        [Required]
        public string ChatId { get; set; }

        [Required]
        public int MessageId { get; set; }
    }
}