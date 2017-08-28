﻿using System.ComponentModel.DataAnnotations;

namespace WetPicsTelegramBot.Database.Model
{
    public class ChatSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long ChatId { get; set; }

        [Required]
        public string TargetId { get; set; }
    }
}