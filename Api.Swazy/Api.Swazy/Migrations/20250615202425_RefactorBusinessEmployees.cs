using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Swazy.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBusinessEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Employees",
                table: "Businesses");

            migrationBuilder.CreateTable(
                name: "BusinessEmployees",
                columns: table => new
                {
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    HiredDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    HiredBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEmployees", x => new { x.BusinessId, x.UserId });
                    table.ForeignKey(
                        name: "FK_BusinessEmployees_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessEmployees_Users_HiredBy",
                        column: x => x.HiredBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessEmployees_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEmployees_HiredBy",
                table: "BusinessEmployees",
                column: "HiredBy");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEmployees_UserId",
                table: "BusinessEmployees",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessEmployees");

            migrationBuilder.AddColumn<string>(
                name: "Employees",
                table: "Businesses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
