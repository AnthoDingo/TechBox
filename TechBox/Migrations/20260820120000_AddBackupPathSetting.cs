using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechBox.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupPathSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[] { 3, "backup_path", "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
