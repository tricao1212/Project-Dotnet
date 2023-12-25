using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Index",
                table: "Order_Details",
                newName: "IndexTemp");

            migrationBuilder.RenameColumn(
                name: "Index",
                table: "Cart",
                newName: "IndexTemp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IndexTemp",
                table: "Order_Details",
                newName: "Index");

            migrationBuilder.RenameColumn(
                name: "IndexTemp",
                table: "Cart",
                newName: "Index");
        }
    }
}
