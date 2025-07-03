using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class OrderReferenceNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefNo",
                table: "Orders",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefNo",
                table: "Orders");
        }
    }
}
