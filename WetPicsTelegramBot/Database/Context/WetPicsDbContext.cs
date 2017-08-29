using Microsoft.EntityFrameworkCore;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Database.Context
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.UserId, vote.ChatId, vote.MessageId }).IsUnique();
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.ChatId, vote.MessageId }).IsUnique(false);
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.UserId });
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.UserId });

            builder.Entity<RepostSetting>()
                .HasIndex(vote => new { vote.ChatId }).IsUnique();

            builder.Entity<PixivSetting>()
                .HasIndex(vote => new { vote.ChatId }).IsUnique();

            builder.Entity<Photo>()
                .HasIndex(vote => new { vote.ChatId, vote.MessageId }).IsUnique();
            builder.Entity<Photo>()
                .HasIndex(vote => new { vote.FromUserId }).IsUnique(false);


            base.OnModelCreating(builder);
        }
    }
}