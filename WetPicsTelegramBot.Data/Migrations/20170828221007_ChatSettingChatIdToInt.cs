using Microsoft.EntityFrameworkCore.Migrations;
using WetPicsTelegramBot.Data.Helpers;

namespace WetPicsTelegramBot.Data.Migrations
{
    public partial class ChatSettingChatIdToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.ChangeColumnType<long>("ChatSettings", "ChatId", "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.ChangeColumnType<string>("ChatSettings", "ChatId", "text");
        }
    }
}
