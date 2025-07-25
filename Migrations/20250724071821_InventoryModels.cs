using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class InventoryModels : Migration
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
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_OfficeId",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_LocationId");

            migrationBuilder.CreateTable(
                name: "InventoryLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LocationType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OfficeId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryLocations_OfficeCountry_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "OfficeCountry",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderNo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FromLocationId = table.Column<int>(type: "int", nullable: true),
                    ToLocationId = table.Column<int>(type: "int", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EncodedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_InventoryLocations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "InventoryLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_InventoryLocations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "InventoryLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "InventoryLocations",
                columns: new[] { "Id", "IsActive", "LocationType", "Name", "OfficeId" },
                values: new object[,]
                {
                    { 1, true, "Warehouse", "Main Warehouse", null },
                    { 2, true, "Stockroom", "Central Stockroom", null },
                    { 3, true, "Office", "Manila HQ", 1 },
                    { 4, true, "Office", "Singapore Office", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryLocations_OfficeId",
                table: "InventoryLocations",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_FromLocationId",
                table: "InventoryTransactions",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId",
                table: "InventoryTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ToLocationId",
                table: "InventoryTransactions",
                column: "ToLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_InventoryLocations_LocationId",
                table: "InventoryStocks",
                column: "LocationId",
                principalTable: "InventoryLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_InventoryLocations_LocationId",
                table: "InventoryStocks");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "InventoryLocations");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "InventoryStocks",
                newName: "OfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_LocationId",
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
