using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class Update6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProductCombos");

            migrationBuilder.AddColumn<string>(
                name: "ProductIdList",
                table: "ProductCombos",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "ProductsDesc",
                table: "ProductCombos",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "QuantityList",
                table: "ProductCombos",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductIdList",
                table: "ProductCombos");

            migrationBuilder.DropColumn(
                name: "ProductsDesc",
                table: "ProductCombos");

            migrationBuilder.DropColumn(
                name: "QuantityList",
                table: "ProductCombos");

            migrationBuilder.AddColumn<int>(
                name: "MainProductId",
                table: "ProductCombos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
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
    }
}
