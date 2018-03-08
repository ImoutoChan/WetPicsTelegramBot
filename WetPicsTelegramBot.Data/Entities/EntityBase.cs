using System;
using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Data.Entities
{
    public class EntityBase
    {
        [Key]
        public int Id { get; set; }
        
        public DateTimeOffset? AddedDate { get; set; }
        
        public DateTimeOffset? ModifiedDate { get; set; }
    }
}
