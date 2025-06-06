using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaidAPI.Migrations
{
    /// <inheritdoc />
    public partial class TaskStatusModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaskStatuses",
                keyColumn: "StatusId",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TaskStatuses",
                columns: new[] { "StatusId", "StatusDescription", "StatusName" },
                values: new object[] { 5, "Done", "Done" });
        }
    }
}
