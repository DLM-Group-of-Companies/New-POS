using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class SalesQuotaUpdateDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuotaMonth",
                table: "SalesQuotas");

            migrationBuilder.DropColumn(
                name: "QuotaYear",
                table: "SalesQuotas");

            migrationBuilder.AddColumn<DateTime>(
                name: "QuotaDate",
                table: "SalesQuotas",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuotaDate",
                table: "SalesQuotas");

            migrationBuilder.AddColumn<int>(
                name: "QuotaMonth",
                table: "SalesQuotas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuotaYear",
                table: "SalesQuotas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
