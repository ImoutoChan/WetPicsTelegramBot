using System;
using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class EntityBase
    {
        [Key]
        public int Id { get; set; }
        
        public DateTimeOffset? AddedDate { get; set; }
        
        public DateTimeOffset? ModifiedDate { get; set; }
    }
}
