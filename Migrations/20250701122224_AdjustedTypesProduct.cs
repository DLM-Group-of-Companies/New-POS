using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class AdjustedTypesProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Products_ProductTypes_ProductType",
            //    table: "Products");

            //migrationBuilder.DropIndex(
            //    name: "IX_Products_ProductType",
            //    table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductType",
                table: "Products",
                column: "ProductType");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductTypes_ProductType",
                table: "Products",
                column: "ProductType",
                principalTable: "ProductTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
