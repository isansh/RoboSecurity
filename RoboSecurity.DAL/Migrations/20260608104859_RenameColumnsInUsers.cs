using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoboSecurity.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsInUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_password",
                table: "Users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "user_mail",
                table: "Users",
                newName: "mail");

            migrationBuilder.RenameColumn(
                name: "role_name",
                table: "Roles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "secret_token",
                table: "Robots",
                newName: "token");

            migrationBuilder.RenameColumn(
                name: "robo_name",
                table: "Robots",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "Alarms",
                newName: "persent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "user_password");

            migrationBuilder.RenameColumn(
                name: "mail",
                table: "Users",
                newName: "user_mail");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Roles",
                newName: "role_name");

            migrationBuilder.RenameColumn(
                name: "token",
                table: "Robots",
                newName: "secret_token");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Robots",
                newName: "robo_name");

            migrationBuilder.RenameColumn(
                name: "persent",
                table: "Alarms",
                newName: "message");
        }
    }
}
