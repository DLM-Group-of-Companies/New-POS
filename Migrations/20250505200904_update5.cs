using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class update5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainProductId",
                table: "ProductCombos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCombos_MainProductId",
                table: "ProductCombos",
                column: "MainProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCombos_Products_MainProductId",
                table: "ProductCombos",
                column: "MainProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCombos_Products_MainProductId",
                table: "ProductCombos");

            migrationBuilder.DropIndex(
                name: "IX_ProductCombos_MainProductId",
                table: "ProductCombos");

            migrationBuilder.DropColumn(
                name: "MainProductId",
                table: "ProductCombos");
        }
    }
}
