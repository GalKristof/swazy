using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Swazy.Migrations
{
    /// <inheritdoc />
    public partial class removeBookedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_BookedByUserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookedByUserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookedByUserId",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BookedByUserId",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookedByUserId",
                table: "Bookings",
                column: "BookedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_BookedByUserId",
                table: "Bookings",
                column: "BookedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
