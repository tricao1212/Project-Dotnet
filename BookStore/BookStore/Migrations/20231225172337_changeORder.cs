using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    /// <inheritdoc />
    public partial class changeORder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_Order_Info_Order_InfoId",
                table: "Order_Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Order_Info_OrderInfoId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Order_Info");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderInfoId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Order_Details_Order_InfoId",
                table: "Order_Details");

            migrationBuilder.DropColumn(
                name: "OrderInfoId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Order_InfoId",
                table: "Order_Details");

            migrationBuilder.AddColumn<int>(
                name: "CartId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartId",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "OrderInfoId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order_InfoId",
                table: "Order_Details",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Order_Info",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order_Info", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderInfoId",
                table: "Orders",
                column: "OrderInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Details_Order_InfoId",
                table: "Order_Details",
                column: "Order_InfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Details_Order_Info_Order_InfoId",
                table: "Order_Details",
                column: "Order_InfoId",
                principalTable: "Order_Info",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Order_Info_OrderInfoId",
                table: "Orders",
                column: "OrderInfoId",
                principalTable: "Order_Info",
                principalColumn: "Id");
        }
    }
}
