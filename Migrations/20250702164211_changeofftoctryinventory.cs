using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class changeofftoctryinventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_OfficeCountry_OfficeId",
                table: "InventoryStocks");

            migrationBuilder.RenameColumn(
                name: "OfficeId",
                table: "InventoryStocks",
                newName: "CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_OfficeId",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_Country_CountryId",
                table: "InventoryStocks",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_Country_CountryId",
                table: "InventoryStocks");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                table: "InventoryStocks",
                newName: "OfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_CountryId",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_OfficeCountry_OfficeId",
                table: "InventoryStocks",
                column: "OfficeId",
                principalTable: "OfficeCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
