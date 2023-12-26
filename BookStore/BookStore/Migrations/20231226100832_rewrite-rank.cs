using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Migrations
{
    /// <inheritdoc />
    public partial class rewriterank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Profile");

            migrationBuilder.AddColumn<int>(
                name: "RankId",
                table: "Profile",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ranks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    discount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ranks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Profile_RankId",
                table: "Profile",
                column: "RankId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profile_Ranks_RankId",
                table: "Profile",
                column: "RankId",
                principalTable: "Ranks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profile_Ranks_RankId",
                table: "Profile");

            migrationBuilder.DropTable(
                name: "Ranks");

            migrationBuilder.DropIndex(
                name: "IX_Profile_RankId",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "RankId",
                table: "Profile");

            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "Profile",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
