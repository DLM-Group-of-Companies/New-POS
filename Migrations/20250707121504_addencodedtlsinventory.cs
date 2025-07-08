using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class addencodedtlsinventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EncodeDate",
                table: "InventoryStocks",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EncodedBy",
                table: "InventoryStocks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "InventoryStocks",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdateddBy",
                table: "InventoryStocks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncodeDate",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "EncodedBy",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "UpdateddBy",
                table: "InventoryStocks");
        }
    }
}
