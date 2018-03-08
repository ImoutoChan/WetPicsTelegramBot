using Microsoft.EntityFrameworkCore.Migrations;

namespace WetPicsTelegramBot.Data.Helpers
{
    static class MigrationHelpers
    {
        public static void ChangeColumnType<TNewColumnType>(this MigrationBuilder migrationBuilder, string tableName, string columnName, string castType, string scheme = "public", bool nullable = false)
        {
            migrationBuilder.AddColumn<TNewColumnType>("temp", tableName, nullable: true);
            migrationBuilder.Sql($"UPDATE {scheme}.\"{tableName}\" " +
                                 $"SET \"temp\" = \"{columnName}\"::{castType}");
            migrationBuilder.DropColumn(columnName, tableName);
            migrationBuilder.RenameColumn("temp", tableName, columnName);
            migrationBuilder.AlterColumn<TNewColumnType>(columnName, tableName, nullable: nullable);
        }
    }
}
