using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using WetPicsTelegramBot.Database.Context;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Database.Migrations
{
    [DbContext(typeof(WetPicsDbContext))]
    [Migration("20170424123640_Photos")]
    partial class Photos
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("WetPicsTelegramBot.ChatSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChatId");

                    b.Property<string>("TargetId");

                    b.HasKey("Id");

                    b.HasIndex("ChatId", "TargetId")
                        .IsUnique();

                    b.ToTable("ChatSettings");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChatId");

                    b.Property<string>("FromUserId");

                    b.Property<int>("MessageId");

                    b.HasKey("Id");

                    b.HasIndex("FromUserId");

                    b.HasIndex("ChatId", "MessageId")
                        .IsUnique();

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("WetPicsTelegramBot.PhotoVote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChatId");

                    b.Property<bool?>("IsLiked");

                    b.Property<int>("MessageId");

                    b.Property<int?>("Score");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ChatId", "MessageId");

                    b.HasIndex("UserId", "ChatId", "MessageId")
                        .IsUnique();

                    b.ToTable("PhotoVotes");
                });
        }
    }
}
