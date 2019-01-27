using System;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Data.Migrations
{
    public partial class PixivInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatSettings_ChatId_TargetId",
                table: "ChatSettings");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PhotoVotes",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChatId",
                table: "PhotoVotes",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromUserId",
                table: "Photos",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChatId",
                table: "Photos",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TargetId",
                table: "ChatSettings",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChatId",
                table: "ChatSettings",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PixivSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ChatId = table.Column<long>(nullable: false),
                    LastPostedTime = table.Column<DateTimeOffset>(nullable: true),
                    MinutesInterval = table.Column<int>(nullable: false),
                    PixivTopType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PixivSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PixivImagePosts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    PixivIllustrationId = table.Column<int>(nullable: false),
                    PixivSettingId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PixivImagePosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PixivImagePosts_PixivSettings_PixivSettingId",
                        column: x => x.PixivSettingId,
                        principalTable: "PixivSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSettings_ChatId",
                table: "ChatSettings",
                column: "ChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PixivImagePosts_PixivSettingId",
                table: "PixivImagePosts",
                column: "PixivSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_PixivSettings_ChatId",
                table: "PixivSettings",
                column: "ChatId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PixivImagePosts");

            migrationBuilder.DropTable(
                name: "PixivSettings");

            migrationBuilder.DropIndex(
                name: "IX_ChatSettings_ChatId",
                table: "ChatSettings");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PhotoVotes",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ChatId",
                table: "PhotoVotes",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "FromUserId",
                table: "Photos",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ChatId",
                table: "Photos",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "TargetId",
                table: "ChatSettings",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ChatId",
                table: "ChatSettings",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_ChatSettings_ChatId_TargetId",
                table: "ChatSettings",
                columns: new[] { "ChatId", "TargetId" },
                unique: true);
        }
    }
}
