using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class Update10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ComboId",
                table: "Orders",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ProductCombos_ComboId",
                table: "Orders",
                column: "ComboId",
                principalTable: "ProductCombos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ProductCombos_ComboId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ComboId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "Orders");
        }
    }
}
