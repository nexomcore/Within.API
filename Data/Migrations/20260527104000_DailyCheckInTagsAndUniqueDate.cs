using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WithinAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class DailyCheckInTagsAndUniqueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "Tags",
                schema: "within",
                table: "DailyCheckIns",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.CreateIndex(
                name: "IX_DailyCheckIns_UserId_CheckInDate",
                schema: "within",
                table: "DailyCheckIns",
                columns: new[] { "UserId", "CheckInDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DailyCheckIns_UserId_CheckInDate",
                schema: "within",
                table: "DailyCheckIns");

            migrationBuilder.DropColumn(
                name: "Tags",
                schema: "within",
                table: "DailyCheckIns");
        }
    }
}
