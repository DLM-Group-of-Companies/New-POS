using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class AddedSalesByInOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_PromoSettings_ProductId",
            //    table: "PromoSettings");

            migrationBuilder.AddColumn<string>(
                name: "SalesBy",
                table: "Orders",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            //migrationBuilder.CreateIndex(
            //    name: "IX_PromoSettings_ProductId",
            //    table: "PromoSettings",
            //    column: "ProductId",
            //    unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PromoSettings_ProductId",
                table: "PromoSettings");

            migrationBuilder.DropColumn(
                name: "SalesBy",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_PromoSettings_ProductId",
                table: "PromoSettings",
                column: "ProductId");
        }
    }
}
