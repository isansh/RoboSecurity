using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoboSecurity.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //    migrationBuilder.CreateTable(
            //        name: "Roles",
            //        columns: table => new
            //        {
            //            role_id = table.Column<int>(type: "int", nullable: false)
            //                .Annotation("SqlServer:Identity", "1, 1"),
            //            role_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //        },
            //        constraints: table =>
            //        {
            //            table.PrimaryKey("PK_Roles", x => x.role_id);
            //        });

            //    migrationBuilder.CreateTable(
            //        name: "Users",
            //        columns: table => new
            //        {
            //            user_id = table.Column<int>(type: "int", nullable: false)
            //                .Annotation("SqlServer:Identity", "1, 1"),
            //            user_mail = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //            phone_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //            user_password = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //        },
            //        constraints: table =>
            //        {
            //            table.PrimaryKey("PK_Users", x => x.user_id);
            //        });

            //    migrationBuilder.CreateTable(
            //        name: "Robots",
            //        columns: table => new
            //        {
            //            robo_id = table.Column<int>(type: "int", nullable: false)
            //                .Annotation("SqlServer:Identity", "1, 1"),
            //            robo_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //            user_id = table.Column<int>(type: "int", nullable: true),
            //            secret_token = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //            status = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //            created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
            //        },
            //        constraints: table =>
            //        {
            //            table.PrimaryKey("PK_Robots", x => x.robo_id);
            //            table.ForeignKey(
            //                name: "FK_Robots_Users_user_id",
            //                column: x => x.user_id,
            //                principalTable: "Users",
            //                principalColumn: "user_id");
            //        });

            //    migrationBuilder.CreateTable(
            //        name: "UserRoles",
            //        columns: table => new
            //        {
            //            user_id = table.Column<int>(type: "int", nullable: false),
            //            role_id = table.Column<int>(type: "int", nullable: false)
            //        },
            //        constraints: table =>
            //        {
            //            table.PrimaryKey("PK_UserRoles", x => new { x.user_id, x.role_id });
            //            table.ForeignKey(
            //                name: "FK_UserRoles_Roles_role_id",
            //                column: x => x.role_id,
            //                principalTable: "Roles",
            //                principalColumn: "role_id",
            //                onDelete: ReferentialAction.Cascade);
            //            table.ForeignKey(
            //                name: "FK_UserRoles_Users_user_id",
            //                column: x => x.user_id,
            //                principalTable: "Users",
            //                principalColumn: "user_id",
            //                onDelete: ReferentialAction.Cascade);
            //        });

            //    migrationBuilder.CreateTable(
            //        name: "Alarms",
            //        columns: table => new
            //        {
            //            alarm_id = table.Column<int>(type: "int", nullable: false)
            //                .Annotation("SqlServer:Identity", "1, 1"),
            //            robo_id = table.Column<int>(type: "int", nullable: false),
            //            timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
            //            message = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //            is_resolved = table.Column<bool>(type: "bit", nullable: false)
            //        },
            //        constraints: table =>
            //        {
            //            table.PrimaryKey("PK_Alarms", x => x.alarm_id);
            //            table.ForeignKey(
            //                name: "FK_Alarms_Robots_robo_id",
            //                column: x => x.robo_id,
            //                principalTable: "Robots",
            //                principalColumn: "robo_id",
            //                onDelete: ReferentialAction.Cascade);
            //        });

            //    migrationBuilder.CreateIndex(
            //        name: "IX_Alarms_robo_id",
            //        table: "Alarms",
            //        column: "robo_id");

            //    migrationBuilder.CreateIndex(
            //        name: "IX_Robots_user_id",
            //        table: "Robots",
            //        column: "user_id");

            //    migrationBuilder.CreateIndex(
            //        name: "IX_UserRoles_role_id",
            //        table: "UserRoles",
            //        column: "role_id");
        }

        ///// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //    migrationBuilder.DropTable(
            //        name: "Alarms");

            //    migrationBuilder.DropTable(
            //        name: "UserRoles");

            //    migrationBuilder.DropTable(
            //        name: "Robots");

            //    migrationBuilder.DropTable(
            //        name: "Roles");

            //    migrationBuilder.DropTable(
            //        name: "Users");
        }
    }
}
