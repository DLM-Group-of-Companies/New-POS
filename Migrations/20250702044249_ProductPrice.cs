using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class ProductPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    RegPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DistPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    StaffPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    BPPPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MedPackPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CorpAccPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    NaturoPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    EncodeDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EncodedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdateDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdateddBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPrices_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPrices_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_CountryId",
                table: "ProductPrices",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPrices_ProductId",
                table: "ProductPrices",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPrices");
        }
    }
}
