using AutoService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AutoService.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261125180000_AssignMechanics")]
    public class AssignMechanics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedMechanicUserId",
                table: "RepairOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_AssignedMechanicUserId",
                table: "RepairOrders",
                column: "AssignedMechanicUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrders_Users_AssignedMechanicUserId",
                table: "RepairOrders",
                column: "AssignedMechanicUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_Users_AssignedMechanicUserId",
                table: "RepairOrders");
            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_AssignedMechanicUserId",
                table: "RepairOrders");
            migrationBuilder.DropColumn(
                name: "AssignedMechanicUserId",
                table: "RepairOrders");
        }
    }
}
