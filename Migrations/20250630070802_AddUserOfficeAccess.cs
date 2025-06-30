using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOfficeAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.CreateTable(
                name: "UserOfficeAccess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OfficeId = table.Column<int>(type: "int", nullable: false),
                    EncodeDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EncodedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdateDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdateddBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOfficeAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOfficeAccess_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOfficeAccess_OfficeCountry_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "OfficeCountry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");


            migrationBuilder.CreateIndex(
                name: "IX_UserOfficeAccess_OfficeId",
                table: "UserOfficeAccess",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOfficeAccess_UserId",
                table: "UserOfficeAccess",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "InventoryStockAuditTrails");

            migrationBuilder.DropTable(
                name: "InventoryStocks");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "UserOfficeAccess");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "ProductCombos");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CustClass");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "OfficeCountry");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropTable(
                name: "Country");
        }
    }
}
