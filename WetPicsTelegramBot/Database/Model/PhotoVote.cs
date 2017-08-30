using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class PhotoVote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public long ChatId { get; set; }

        [Required]
        public int MessageId { get; set; }
    }
}