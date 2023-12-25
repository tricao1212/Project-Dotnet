using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    /// <inheritdoc />
    public partial class addORderBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_Oder_Info_Oder_InfoId",
                table: "Order_Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Oder_Info_OrderInfoId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Oder_Info");

            migrationBuilder.RenameColumn(
                name: "Oder_InfoId",
                table: "Order_Details",
                newName: "Order_InfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_Details_Oder_InfoId",
                table: "Order_Details",
                newName: "IX_Order_Details_Order_InfoId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Details_Order_Info_Order_InfoId",
                table: "Order_Details");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Order_Info_OrderInfoId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Order_Info");

            migrationBuilder.RenameColumn(
                name: "Order_InfoId",
                table: "Order_Details",
                newName: "Oder_InfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_Details_Order_InfoId",
                table: "Order_Details",
                newName: "IX_Order_Details_Oder_InfoId");

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
    }
}
