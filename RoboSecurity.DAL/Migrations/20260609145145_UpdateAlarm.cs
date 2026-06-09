using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoboSecurity.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAlarm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "snapshot_path",
                table: "Alarms",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "snapshot_path",
                table: "Alarms");
        }
    }
}
