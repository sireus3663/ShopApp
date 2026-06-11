using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopProject.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePasswordWithHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "users",
                newName: "PasswordHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "Password");
        }
    }
}
