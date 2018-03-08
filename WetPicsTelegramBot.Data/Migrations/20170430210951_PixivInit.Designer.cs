using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using WetPicsTelegramBot.Data.Context;

namespace WetPicsTelegramBot.Data.Migrations
{
    [DbContext(typeof(WetPicsDbContext))]
    [Migration("20170430210951_PixivInit")]
    partial class PixivInit
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("WetPicsTelegramBot.Database.Model.ChatSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChatId")
                        .IsRequired();

                    b.Property<string>("TargetId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ChatId")
                        .IsUnique();

                    b.ToTable("ChatSettings");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Database.Model.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChatId")
                        .IsRequired();

                    b.Property<string>("FromUserId")
                        .IsRequired();

                    b.Property<int>("MessageId");

                    b.HasKey("Id");

                    b.HasIndex("FromUserId");

                    b.HasIndex("ChatId", "MessageId")
                        .IsUnique();

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Database.Model.PhotoVote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChatId")
                        .IsRequired();

                    b.Property<bool?>("IsLiked");

                    b.Property<int>("MessageId");

                    b.Property<int?>("Score");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("IsLiked");

                    b.HasIndex("UserId");

                    b.HasIndex("ChatId", "MessageId");

                    b.HasIndex("IsLiked", "UserId");

                    b.HasIndex("UserId", "ChatId", "MessageId")
                        .IsUnique();

                    b.ToTable("PhotoVotes");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Database.Model.PixivImagePost", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("PixivIllustrationId");

                    b.Property<int>("PixivSettingId");

                    b.HasKey("Id");

                    b.HasIndex("PixivSettingId");

                    b.ToTable("PixivImagePosts");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Database.Model.PixivSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ChatId");

                    b.Property<DateTimeOffset?>("LastPostedTime");

                    b.Property<int>("MinutesInterval");

                    b.Property<int>("PixivTopType");

                    b.HasKey("Id");

                    b.HasIndex("ChatId")
                        .IsUnique();

                    b.ToTable("PixivSettings");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Database.Model.PixivImagePost", b =>
                {
                    b.HasOne("WetPicsTelegramBot.Database.Model.PixivSetting", "PixivSetting")
                        .WithMany("PixivImagePosts")
                        .HasForeignKey("PixivSettingId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
