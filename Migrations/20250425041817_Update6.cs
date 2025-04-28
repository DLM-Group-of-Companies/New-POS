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
            migrationBuilder.AlterColumn<int>(
                name: "OfficeId",
                table: "Customer",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_OfficeId",
                table: "Customer",
                column: "OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_OfficeCountry_OfficeId",
                table: "Customer",
                column: "OfficeId",
                principalTable: "OfficeCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_OfficeCountry_OfficeId",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_OfficeId",
                table: "Customer");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeId",
                table: "Customer",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
