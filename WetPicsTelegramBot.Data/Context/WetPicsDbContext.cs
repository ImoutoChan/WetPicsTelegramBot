using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WetPicsTelegramBot.Data.Entities;
using WetPicsTelegramBot.Data.Entities.ImageSources;

namespace WetPicsTelegramBot.Data.Context
{
    public class WetPicsDbContext : DbContext
    {
        public WetPicsDbContext(DbContextOptions<WetPicsDbContext> options) 
            : base(options)
        {
        }


        public DbSet<PhotoVote> PhotoVotes { get; set; }

        public DbSet<RepostSetting> RepostSettings { get; set; }

        public DbSet<Photo> Photos { get; set; }

        public DbSet<ChatUser> ChatUsers { get; set; }

        public DbSet<ImageSourcesChatSetting> ImageSourcesChatSettings { get; set; }

        public DbSet<ImageSourceSetting> ImageSourceSettings { get; set; }

        public DbSet<PostedImage> PostedImages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.UserId, vote.ChatId, vote.MessageId }).IsUnique();
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.ChatId, vote.MessageId }).IsUnique(false);
            builder.Entity<PhotoVote>()
                .HasIndex(vote => vote.UserId);

            builder.Entity<RepostSetting>()
               .HasIndex(x => x.ChatId)
               .IsUnique();

            builder.Entity<ImageSourcesChatSetting>()
                .HasIndex(x => x.ChatId)
                .IsUnique();
            
            builder.Entity<ImageSourceSetting>()
                .HasIndex(x => x.ImageSourcesChatSettingId);
            builder.Entity<ImageSourceSetting>()
                .HasOne(x => x.ImageSourcesChatSetting)
                .WithMany(x => x.ImageSourceSettings);
                
            builder.Entity<PostedImage>()
                .HasOne(x => x.ImageSourcesChatSetting)
                .WithMany(x => x.PostedImages);
            builder.Entity<PostedImage>()
                .HasIndex(x => new { x.ImageSourcesChatSettingId, x.ImageSource, x.PostId });

            builder.Entity<Photo>()
                .HasIndex(x => new { x.ChatId, x.MessageId }).IsUnique();
            builder.Entity<Photo>()
                .HasIndex(x => x.FromUserId).IsUnique(false);

            builder.Entity<ChatUser>()
                .HasIndex(x => x.UserId).IsUnique();

            base.OnModelCreating(builder);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess, 
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