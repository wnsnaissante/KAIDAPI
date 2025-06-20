using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaidAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixSuperiorIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_SuperiorId",
                table: "Memberships");

            migrationBuilder.AlterColumn<Guid>(
                name: "SuperiorId",
                table: "Memberships",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_SuperiorId",
                table: "Memberships",
                column: "SuperiorId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_SuperiorId",
                table: "Memberships");

            migrationBuilder.AlterColumn<Guid>(
                name: "SuperiorId",
                table: "Memberships",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_SuperiorId",
                table: "Memberships",
                column: "SuperiorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
