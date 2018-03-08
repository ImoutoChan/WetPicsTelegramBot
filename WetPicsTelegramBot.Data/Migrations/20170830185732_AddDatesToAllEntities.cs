using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Data.Migrations
{
    public partial class AddDatesToAllEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AddedDate",
                table: "RepostSettings",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                table: "RepostSettings",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AddedDate",
                table: "PixivSettings",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                table: "PixivSettings",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AddedDate",
                table: "PixivImagePosts",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                table: "PixivImagePosts",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AddedDate",
                table: "PhotoVotes",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                table: "PhotoVotes",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AddedDate",
                table: "Photos",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                table: "Photos",
                type: "timestamptz",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "RepostSettings");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "RepostSettings");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "PixivSettings");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "PixivSettings");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "PixivImagePosts");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "PixivImagePosts");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "PhotoVotes");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "PhotoVotes");

            migrationBuilder.DropColumn(
                name: "AddedDate",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Photos");
        }
    }
}
