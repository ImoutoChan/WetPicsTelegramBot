using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class PhotoVote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string ChatId { get; set; }

        [Required]
        public int MessageId { get; set; }

        public int? Score { get; set; }

        public bool? IsLiked { get; set; }
    }
}