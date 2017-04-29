using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Migrations
{
    public partial class VotesIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PhotoVotes_IsLiked",
                table: "PhotoVotes",
                column: "IsLiked");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoVotes_UserId",
                table: "PhotoVotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoVotes_IsLiked_UserId",
                table: "PhotoVotes",
                columns: new[] { "IsLiked", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhotoVotes_IsLiked",
                table: "PhotoVotes");

            migrationBuilder.DropIndex(
                name: "IX_PhotoVotes_UserId",
                table: "PhotoVotes");

            migrationBuilder.DropIndex(
                name: "IX_PhotoVotes_IsLiked_UserId",
                table: "PhotoVotes");
        }
    }
}
