using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopProject.Migrations
{
    /// <inheritdoc />
    public partial class newcart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceAtMoment",
                table: "carts");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "orders");

            migrationBuilder.AddColumn<int>(
                name: "PriceAtMoment",
                table: "carts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
