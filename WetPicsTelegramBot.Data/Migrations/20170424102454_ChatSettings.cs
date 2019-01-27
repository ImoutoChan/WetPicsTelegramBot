using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Data.Migrations
{
    public partial class ChatSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ChatId = table.Column<string>(nullable: true),
                    TargetId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSettings_ChatId_TargetId",
                table: "ChatSettings",
                columns: new[] { "ChatId", "TargetId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatSettings");
        }
    }
}
