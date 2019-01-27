using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace WetPicsTelegramBot.Data.Migrations
{
    public partial class ImageSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageSourcesChatSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AddedDate = table.Column<DateTimeOffset>(nullable: true),
                    ChatId = table.Column<long>(nullable: false),
                    LastPostedTime = table.Column<DateTimeOffset>(nullable: true),
                    MinutesInterval = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageSourcesChatSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImageSourceSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AddedDate = table.Column<DateTimeOffset>(nullable: true),
                    ImageSource = table.Column<int>(nullable: false),
                    ImageSourcesChatSettingId = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: true),
                    Options = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageSourceSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageSourceSettings_ImageSourcesChatSettings_ImageSourcesChatSettingId",
                        column: x => x.ImageSourcesChatSettingId,
                        principalTable: "ImageSourcesChatSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostedImages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AddedDate = table.Column<DateTimeOffset>(nullable: true),
                    ImageSource = table.Column<int>(nullable: false),
                    ImageSourcesChatSettingId = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: true),
                    PostId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostedImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostedImages_ImageSourcesChatSettings_ImageSourcesChatSettingId",
                        column: x => x.ImageSourcesChatSettingId,
                        principalTable: "ImageSourcesChatSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageSourcesChatSettings_ChatId",
                table: "ImageSourcesChatSettings",
                column: "ChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageSourceSettings_ImageSourcesChatSettingId",
                table: "ImageSourceSettings",
                column: "ImageSourcesChatSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_PostedImages_ImageSourcesChatSettingId_ImageSource_PostId",
                table: "PostedImages",
                columns: new[] { "ImageSourcesChatSettingId", "ImageSource", "PostId" });
            
            
            // MIGRATION OF DATA
            //            insert CHAT SETTING
            //            
            //            insert into "PostedImages" ("AddedDate", "ImageSource", "ImageSourcesChatSettingId", "ModifiedDate", "PostId")
            //            select pip."AddedDate", 0, 1, pip."ModifiedDate", pip."PixivIllustrationId" 
            //            from "PixivImagePosts" pip
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageSourceSettings");

            migrationBuilder.DropTable(
                name: "PostedImages");

            migrationBuilder.DropTable(
                name: "ImageSourcesChatSettings");

            migrationBuilder.CreateTable(
                name: "PixivSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AddedDate = table.Column<DateTimeOffset>(nullable: true),
                    ChatId = table.Column<long>(nullable: false),
                    LastPostedTime = table.Column<DateTimeOffset>(nullable: true),
                    MinutesInterval = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: true),
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
                    AddedDate = table.Column<DateTimeOffset>(nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(nullable: true),
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
                name: "IX_PixivImagePosts_PixivSettingId",
                table: "PixivImagePosts",
                column: "PixivSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_PixivSettings_ChatId",
                table: "PixivSettings",
                column: "ChatId",
                unique: true);
        }
    }
}
