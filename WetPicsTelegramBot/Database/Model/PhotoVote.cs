using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot
{
    public class PhotoVote
    {
        [Key]
        public int Id { get; set; }
        
        public string UserId { get; set; }
        
        public string ChatId { get; set; }
        
        public int MessageId { get; set; }

        public int? Score { get; set; }

        public bool? IsLiked { get; set; }
    }
}