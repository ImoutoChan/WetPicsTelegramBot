using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Database.Migrations
{
    public partial class Indexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PhotoVotes_ChatId_MessageId",
                table: "PhotoVotes",
                columns: new[] { "ChatId", "MessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_PhotoVotes_UserId_ChatId_MessageId",
                table: "PhotoVotes",
                columns: new[] { "UserId", "ChatId", "MessageId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhotoVotes_ChatId_MessageId",
                table: "PhotoVotes");

            migrationBuilder.DropIndex(
                name: "IX_PhotoVotes_UserId_ChatId_MessageId",
                table: "PhotoVotes");
        }
    }
}
