using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Data.Migrations
{
    public partial class Photos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ChatId = table.Column<string>(nullable: true),
                    FromUserId = table.Column<string>(nullable: true),
                    MessageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_FromUserId",
                table: "Photos",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_ChatId_MessageId",
                table: "Photos",
                columns: new[] { "ChatId", "MessageId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Photos");
        }
    }
}
