using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace WetPicsTelegramBot.Database.Migrations
{
    public partial class RemoveScoreAndIsLiked : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhotoVotes_IsLiked",
                table: "PhotoVotes");

            migrationBuilder.DropColumn(
                name: "IsLiked",
                table: "PhotoVotes");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "PhotoVotes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLiked",
                table: "PhotoVotes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "PhotoVotes",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhotoVotes_IsLiked",
                table: "PhotoVotes",
                column: "IsLiked");
        }
    }
}
