using Microsoft.EntityFrameworkCore.Migrations;

namespace Checkout.Api.Migrations
{
    public partial class Price_and_Category : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Products",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");
        }
    }
}
