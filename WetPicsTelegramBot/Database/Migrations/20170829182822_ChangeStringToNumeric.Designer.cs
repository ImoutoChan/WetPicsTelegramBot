﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using WetPicsTelegramBot.Database.Context;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Database.Migrations
{
    [DbContext(typeof(WetPicsDbContext))]
    [Migration("20170829182822_ChangeStringToNumeric")]
    partial class ChangeStringToNumeric
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("WetPicsTelegramBot.Database.Model.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ChatId");

                    b.Property<int>("FromUserId");

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

                    b.Property<long>("ChatId");

                    b.Property<bool?>("IsLiked");

                    b.Property<int>("MessageId");

                    b.Property<int?>("Score");

                    b.Property<int>("UserId");

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

            modelBuilder.Entity("WetPicsTelegramBot.Database.Model.RepostSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ChatId");

                    b.Property<string>("TargetId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ChatId")
                        .IsUnique();

                    b.ToTable("RepostSettings");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Database.Model.PixivImagePost", b =>
                {
                    b.HasOne("WetPicsTelegramBot.Database.Model.PixivSetting", "PixivSetting")
                        .WithMany("PixivImagePosts")
                        .HasForeignKey("PixivSettingId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
