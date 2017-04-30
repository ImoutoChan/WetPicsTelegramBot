using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Database.Migrations
{
    public partial class ChangedUserIdType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PhotoVotes",
                nullable: true,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "PhotoVotes",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
