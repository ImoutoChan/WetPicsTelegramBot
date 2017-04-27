using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace WetPicsTelegramBot
{
    public class WetPicsDbContext : DbContext
    {
        public DbSet<PhotoVote> PhotoVotes { get; set; }

        public DbSet<ChatSetting> ChatSettings { get; set; }

        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.UserId, vote.ChatId, vote.MessageId }).IsUnique();
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.ChatId, vote.MessageId }).IsUnique(false);

            builder.Entity<ChatSetting>()
                .HasIndex(vote => new { vote.ChatId, vote.TargetId }).IsUnique(true);

            builder.Entity<Photo>()
                .HasIndex(vote => new { vote.ChatId, vote.MessageId }).IsUnique(true);
            builder.Entity<Photo>()
                .HasIndex(vote => new { vote.FromUserId }).IsUnique(false);


            base.OnModelCreating(builder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=postgresserver;Port=5432;Database=wetpics;User Id=postgres;Password=postgres;");
        }
    }
}