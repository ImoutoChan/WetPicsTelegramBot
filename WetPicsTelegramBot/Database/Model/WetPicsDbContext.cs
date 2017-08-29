using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WetPicsTelegramBot.Database.Model
{
    class DesignTimeLocalDbContextFactory : IDesignTimeDbContextFactory<WetPicsDbContext>
    {
        public WetPicsDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"AppSettings.Development.json", true, true)
                    .AddEnvironmentVariables()
                    .Build()
                    .GetSection("Configuration")
                    ["ConnectionString"];

            var optionsBuilder = new DbContextOptionsBuilder<WetPicsDbContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new WetPicsDbContext(optionsBuilder.Options);
        }
    }

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
                .HasIndex(vote => new { vote.IsLiked });
            builder.Entity<PhotoVote>()
                .HasIndex(vote => new { vote.IsLiked, vote.UserId });

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