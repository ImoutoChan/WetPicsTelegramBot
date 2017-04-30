using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Database.Migrations
{
    public partial class AddedChatId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MessageId",
                table: "PhotoVotes",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<string>(
                name: "ChatId",
                table: "PhotoVotes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "PhotoVotes");

            migrationBuilder.AlterColumn<long>(
                name: "MessageId",
                table: "PhotoVotes",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
