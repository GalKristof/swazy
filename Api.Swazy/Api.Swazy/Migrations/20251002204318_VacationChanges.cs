using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Swazy.Migrations
{
    /// <inheritdoc />
    public partial class VacationChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnVacation",
                table: "EmployeeWeeklySchedules");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VacationFrom",
                table: "EmployeeWeeklySchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VacationTo",
                table: "EmployeeWeeklySchedules",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VacationFrom",
                table: "EmployeeWeeklySchedules");

            migrationBuilder.DropColumn(
                name: "VacationTo",
                table: "EmployeeWeeklySchedules");

            migrationBuilder.AddColumn<bool>(
                name: "IsOnVacation",
                table: "EmployeeWeeklySchedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
