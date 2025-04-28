using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class update7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CustClass",
                table: "Customer",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CustClass",
                table: "Customer",
                column: "CustClass");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_CustClass_CustClass",
                table: "Customer",
                column: "CustClass",
                principalTable: "CustClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_CustClass_CustClass",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_CustClass",
                table: "Customer");

            migrationBuilder.AlterColumn<string>(
                name: "CustClass",
                table: "Customer",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
