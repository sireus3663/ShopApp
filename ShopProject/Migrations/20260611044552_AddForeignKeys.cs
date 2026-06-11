using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopProject.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_products_SellerId",
                table: "products",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_ProductId",
                table: "orders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_orders_UserId",
                table: "orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_ProductId",
                table: "favorites",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_UserId",
                table: "favorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_discounts_ProductId",
                table: "discounts",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_carts_ProductId",
                table: "carts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_carts_UserId",
                table: "carts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_carts_products_ProductId",
                table: "carts",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_carts_users_UserId",
                table: "carts",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_discounts_products_ProductId",
                table: "discounts",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_favorites_products_ProductId",
                table: "favorites",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_favorites_users_UserId",
                table: "favorites",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_products_ProductId",
                table: "orders",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_users_UserId",
                table: "orders",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_products_users_SellerId",
                table: "products",
                column: "SellerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_carts_products_ProductId",
                table: "carts");

            migrationBuilder.DropForeignKey(
                name: "FK_carts_users_UserId",
                table: "carts");

            migrationBuilder.DropForeignKey(
                name: "FK_discounts_products_ProductId",
                table: "discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_favorites_products_ProductId",
                table: "favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_favorites_users_UserId",
                table: "favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_products_ProductId",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_users_UserId",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_products_users_SellerId",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_SellerId",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_orders_ProductId",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_UserId",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_favorites_ProductId",
                table: "favorites");

            migrationBuilder.DropIndex(
                name: "IX_favorites_UserId",
                table: "favorites");

            migrationBuilder.DropIndex(
                name: "IX_discounts_ProductId",
                table: "discounts");

            migrationBuilder.DropIndex(
                name: "IX_carts_ProductId",
                table: "carts");

            migrationBuilder.DropIndex(
                name: "IX_carts_UserId",
                table: "carts");
        }
    }
}
