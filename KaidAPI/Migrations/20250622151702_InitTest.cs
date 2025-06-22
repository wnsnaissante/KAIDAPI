using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaidAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_StatusId",
                table: "ProjectTasks",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_TaskStatuses_StatusId",
                table: "ProjectTasks",
                column: "StatusId",
                principalTable: "TaskStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_TaskStatuses_StatusId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_StatusId",
                table: "ProjectTasks");
        }
    }
}
