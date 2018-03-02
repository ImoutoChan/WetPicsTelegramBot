using Microsoft.EntityFrameworkCore.Migrations;
using WetPicsTelegramBot.Data.Helpers;

namespace WetPicsTelegramBot.Data.Migrations
{
    public partial class ChangeStringToNumeric : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        { 
            migrationBuilder.ChangeColumnType<int>("PhotoVotes", "UserId", "integer");
            migrationBuilder.ChangeColumnType<long>("PhotoVotes", "ChatId", "bigint");
            migrationBuilder.ChangeColumnType<int>("Photos", "FromUserId", "integer");
            migrationBuilder.ChangeColumnType<long>("Photos", "ChatId", "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.ChangeColumnType<string>("PhotoVotes", "UserId", "text");
            migrationBuilder.ChangeColumnType<string>("PhotoVotes", "ChatId", "text");
            migrationBuilder.ChangeColumnType<string>("Photos", "FromUserId", "text");
            migrationBuilder.ChangeColumnType<string>("Photos", "ChatId", "text");
        }
    }
}
