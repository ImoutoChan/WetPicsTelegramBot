using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
