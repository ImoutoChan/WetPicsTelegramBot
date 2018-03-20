﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.Data.Helpers;

namespace WetPicsTelegramBot.Data.Context
{
    public class WetPicsDbContext : DbContext
    {
        public WetPicsDbContext(DbContextOptions<WetPicsDbContext> options) : base(options)
        {
        }

        public DbSet<PhotoVote> PhotoVotes { get; set; }

        public DbSet<RepostSetting> RepostSettings { get; set; }

        public DbSet<Photo> Photos { get; set; }

        public DbSet<PixivSetting> PixivSettings { get; set; }

        public DbSet<PixivImagePost> PixivImagePosts { get; set; }

        public DbSet<ChatUser> ChatUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.UserId, vote.ChatId, vote.MessageId }).IsUnique();
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.ChatId, vote.MessageId }).IsUnique(false);
            builder.Entity<PhotoVote>()
                .HasIndex(vote => vote.UserId);

            builder.Entity<RepostSetting>()
                .HasIndex(x => x.ChatId).IsUnique();

            builder.Entity<PixivSetting>()
                .HasIndex(x => x.ChatId).IsUnique();
            builder.Entity<PixivImagePost>()
                .HasIndex(x => x.PixivSettingId).IsUnique(false);

            builder.Entity<Photo>()
                .HasIndex(x => new { x.ChatId, x.MessageId }).IsUnique();
            builder.Entity<Photo>()
                .HasIndex(x => x.FromUserId).IsUnique(false);

            builder.Entity<ChatUser>()
                .HasIndex(x => x.UserId).IsUnique();

            builder.AddPostgreSqlRules();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entityEntry in ChangeTracker.Entries<EntityBase>())
            {
                switch (entityEntry.State)
                {
                    case EntityState.Added:
                        entityEntry.Entity.AddedDate = DateTimeOffset.Now;
                        entityEntry.Entity.ModifiedDate = DateTimeOffset.Now;
                        break;
                    case EntityState.Modified:
                        entityEntry.Entity.ModifiedDate = DateTimeOffset.Now;
                        break;
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}