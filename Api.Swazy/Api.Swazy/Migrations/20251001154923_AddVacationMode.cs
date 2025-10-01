using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Swazy.Migrations
{
    /// <inheritdoc />
    public partial class AddVacationMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnVacation",
                table: "EmployeeWeeklySchedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnVacation",
                table: "EmployeeWeeklySchedules");
        }
    }
}
