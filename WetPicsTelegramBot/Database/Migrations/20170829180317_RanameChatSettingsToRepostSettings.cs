using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Database.Migrations
{
    public partial class RanameChatSettingsToRepostSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("ChatSettings", newName: "RepostSettings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("RepostSettings", newName: "ChatSettings");
        }
    }
}
