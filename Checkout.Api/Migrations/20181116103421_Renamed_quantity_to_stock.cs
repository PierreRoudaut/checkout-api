using Microsoft.EntityFrameworkCore.Migrations;

namespace Checkout.Api.Migrations
{
    public partial class Renamed_quantity_to_stock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Products",
                newName: "Stock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Stock",
                table: "Products",
                newName: "Quantity");
        }
    }
}
