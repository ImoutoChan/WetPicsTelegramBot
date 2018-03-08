using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Data.Entities
{
    public class PhotoVote : EntityBase
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public long ChatId { get; set; }

        [Required]
        public int MessageId { get; set; }
    }
}