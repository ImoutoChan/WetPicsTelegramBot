using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using WetPicsTelegramBot.Data.Context;

namespace WetPicsTelegramBot.Data.Migrations
{
    [DbContext(typeof(WetPicsDbContext))]
    [Migration("20170424003619_RenamedFields")]
    partial class RenamedFields
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

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.ToTable("PhotoVotes");
                });
        }
    }
}
