using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Parts",
                columns: new[] { "Id", "Description", "Name", "PartNumber", "Price", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "Engine oil filter", "Oil Filter", "OIL-001", 12.50m, 15 },
                    { 2, "Front brake pads", "Brake Pads", "BRK-001", 45.00m, 8 },
                    { 3, "Synthetic engine oil", "Engine Oil 1L", "ENG-001", 14.00m, 30 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Parts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Parts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Parts",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
