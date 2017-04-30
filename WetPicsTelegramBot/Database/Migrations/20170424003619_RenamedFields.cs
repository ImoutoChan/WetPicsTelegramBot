using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Database.Migrations
{
    public partial class RenamedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Vote",
                table: "PhotoVotes");

            migrationBuilder.RenameColumn(
                name: "PhotoId",
                table: "PhotoVotes",
                newName: "MessageId");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "PhotoVotes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "PhotoVotes");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "PhotoVotes",
                newName: "PhotoId");

            migrationBuilder.AddColumn<int>(
                name: "Vote",
                table: "PhotoVotes",
                nullable: false,
                defaultValue: 0);
        }
    }
}
