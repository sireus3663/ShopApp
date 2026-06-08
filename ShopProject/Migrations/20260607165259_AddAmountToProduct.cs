using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopProject.Migrations
{

    public partial class AddAmountToProduct : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "products");
        }
    }
}
