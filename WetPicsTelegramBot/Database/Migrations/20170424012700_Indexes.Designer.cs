using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using WetPicsTelegramBot;
using WetPicsTelegramBot.Database.Model;

namespace WetPicsTelegramBot.Migrations
{
    [DbContext(typeof(WetPicsDbContext))]
    [Migration("20170424012700_Indexes")]
    partial class Indexes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.1");

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
