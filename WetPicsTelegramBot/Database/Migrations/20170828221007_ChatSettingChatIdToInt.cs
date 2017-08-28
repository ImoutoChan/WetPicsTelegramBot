using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WetPicsTelegramBot.Database.Migrations
{
    public partial class ChatSettingChatIdToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ChatId",
                table: "ChatSettings",
                type: "int8",
                nullable: false,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ChatId",
                table: "ChatSettings",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "int8");
        }
    }
}
