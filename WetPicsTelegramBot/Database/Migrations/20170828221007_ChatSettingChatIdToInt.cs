using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using WetPicsTelegramBot.Helpers;

namespace WetPicsTelegramBot.Database.Migrations
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
