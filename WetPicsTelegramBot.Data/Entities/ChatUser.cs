using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Data.Entities
{
    public class ChatUser : EntityBase
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; set; }
    }
}