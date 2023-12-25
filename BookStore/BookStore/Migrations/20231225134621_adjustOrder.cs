using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    /// <inheritdoc />
    public partial class adjustOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Cart_CartId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "CartId",
                table: "Orders",
                newName: "OrderInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CartId",
                table: "Orders",
                newName: "IX_Orders_OrderInfoId");

            migrationBuilder.AddColumn<int>(
                name: "Oder_InfoId",
                table: "Order_Details",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Oder_Info",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oder_Info", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_Details_Oder_InfoId",
                table: "Order_Details",
                column: "Oder_InfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Details_Oder_Info_Oder_InfoId",
                table: "Order_Details",
                column: "Oder_InfoId",
                principalTable: "Oder_Info",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Oder_Info_OrderInfoId",
                table: "Orders",
                column: "OrderInfoId",
                principalTable: "Oder_Info",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_Oder_Info_Oder_InfoId",
                table: "Order_Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Oder_Info_OrderInfoId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Oder_Info");

            migrationBuilder.DropIndex(
                name: "IX_Order_Details_Oder_InfoId",
                table: "Order_Details");

            migrationBuilder.DropColumn(
                name: "Oder_InfoId",
                table: "Order_Details");

            migrationBuilder.RenameColumn(
                name: "OrderInfoId",
                table: "Orders",
                newName: "CartId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_OrderInfoId",
                table: "Orders",
                newName: "IX_Orders_CartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Cart_CartId",
                table: "Orders",
                column: "CartId",
                principalTable: "Cart",
                principalColumn: "Id");
        }
    }
}
