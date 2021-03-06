﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using WetPicsTelegramBot.Data.Context;

namespace WetPicsTelegramBot.Data.Migrations
{
    [DbContext(typeof(WetPicsDbContext))]
    partial class WetPicsDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("WetPicsTelegramBot.Data.Entities.ChatUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AddedDate");

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<string>("LastName");

                    b.Property<DateTimeOffset?>("ModifiedDate");

                    b.Property<int>("UserId");

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("ChatUsers");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Data.Entities.ImageSources.ImageSourcesChatSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AddedDate");

                    b.Property<long>("ChatId");

                    b.Property<DateTimeOffset?>("LastPostedTime");

                    b.Property<int>("MinutesInterval");

                    b.Property<DateTimeOffset?>("ModifiedDate");

                    b.HasKey("Id");

                    b.HasIndex("ChatId")
                        .IsUnique();

                    b.ToTable("ImageSourcesChatSettings");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Data.Entities.ImageSources.ImageSourceSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AddedDate");

                    b.Property<int>("ImageSource");

                    b.Property<int>("ImageSourcesChatSettingId");

                    b.Property<DateTimeOffset?>("ModifiedDate");

                    b.Property<string>("Options");

                    b.HasKey("Id");

                    b.HasIndex("ImageSourcesChatSettingId");

                    b.ToTable("ImageSourceSettings");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Data.Entities.ImageSources.PostedImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AddedDate");

                    b.Property<int>("ImageSource");

                    b.Property<int>("ImageSourcesChatSettingId");

                    b.Property<DateTimeOffset?>("ModifiedDate");

                    b.Property<int>("PostId");

                    b.HasKey("Id");

                    b.HasIndex("ImageSourcesChatSettingId", "ImageSource", "PostId");

                    b.ToTable("PostedImages");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Data.Entities.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AddedDate");

                    b.Property<long>("ChatId");

                    b.Property<int>("FromUserId");

                    b.Property<int>("MessageId");

                    b.Property<DateTimeOffset?>("ModifiedDate");

                    b.HasKey("Id");

                    b.HasIndex("FromUserId");

                    b.HasIndex("ChatId", "MessageId")
                        .IsUnique();

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Data.Entities.PhotoVote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AddedDate");

                    b.Property<long>("ChatId");

                    b.Property<int>("MessageId");

                    b.Property<DateTimeOffset?>("ModifiedDate");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("ChatId", "MessageId");

                    b.HasIndex("UserId", "ChatId", "MessageId")
                        .IsUnique();

                    b.ToTable("PhotoVotes");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Data.Entities.RepostSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset?>("AddedDate");

                    b.Property<long>("ChatId");

                    b.Property<DateTimeOffset?>("ModifiedDate");

                    b.Property<string>("TargetId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ChatId")
                        .IsUnique();

                    b.ToTable("RepostSettings");
                });

            modelBuilder.Entity("WetPicsTelegramBot.Data.Entities.ImageSources.ImageSourceSetting", b =>
                {
                    b.HasOne("WetPicsTelegramBot.Data.Entities.ImageSources.ImageSourcesChatSetting", "ImageSourcesChatSetting")
                        .WithMany("ImageSourceSettings")
                        .HasForeignKey("ImageSourcesChatSettingId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WetPicsTelegramBot.Data.Entities.ImageSources.PostedImage", b =>
                {
                    b.HasOne("WetPicsTelegramBot.Data.Entities.ImageSources.ImageSourcesChatSetting", "ImageSourcesChatSetting")
                        .WithMany("PostedImages")
                        .HasForeignKey("ImageSourcesChatSettingId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
