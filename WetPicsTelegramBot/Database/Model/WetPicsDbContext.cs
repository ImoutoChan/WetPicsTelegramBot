using Microsoft.EntityFrameworkCore;

namespace WetPicsTelegramBot.Database.Model
{
    public class WetPicsDbContext : DbContext
    {
        public WetPicsDbContext(DbContextOptions<WetPicsDbContext> options) : base(options)
        {
        }

        public DbSet<PhotoVote> PhotoVotes { get; set; }

        public DbSet<ChatSetting> ChatSettings { get; set; }

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
                .HasIndex(vote => new { vote.IsLiked });
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.IsLiked, vote.UserId });

            builder.Entity<ChatSetting>()
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