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
    [Migration("20170424005302_ChangedUserIdType")]
    partial class ChangedUserIdType
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

                    b.Property<bool?>("IsLiked");

                    b.Property<long>("MessageId");

                    b.Property<int?>("Score");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.ToTable("PhotoVotes");
                });
        }
    }
}
