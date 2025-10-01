using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Swazy.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeWeeklySchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    BufferTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeWeeklySchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeWeeklySchedules_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeWeeklySchedules_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDaySchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeWeeklyScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    IsWorkingDay = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDaySchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDaySchedules_EmployeeWeeklySchedules_EmployeeWeekly~",
                        column: x => x.EmployeeWeeklyScheduleId,
                        principalTable: "EmployeeWeeklySchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDaySchedules_EmployeeWeeklyScheduleId_DayOfWeek",
                table: "EmployeeDaySchedules",
                columns: new[] { "EmployeeWeeklyScheduleId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWeeklySchedules_BusinessId",
                table: "EmployeeWeeklySchedules",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWeeklySchedules_UserId_BusinessId",
                table: "EmployeeWeeklySchedules",
                columns: new[] { "UserId", "BusinessId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDaySchedules");

            migrationBuilder.DropTable(
                name: "EmployeeWeeklySchedules");
        }
    }
}
