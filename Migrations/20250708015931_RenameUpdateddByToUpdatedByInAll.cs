using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class RenameUpdateddByToUpdatedByInAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdateddBy",
                table: "UserOfficesAccess",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdateddBy",
                table: "ProductPrices",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdateddBy",
                table: "Orders",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdateddBy",
                table: "OfficeCountry",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdateddBy",
                table: "Customer",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdateddBy",
                table: "CustClass",
                newName: "UpdatedBy");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "UserOfficesAccess",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "UserOfficesAccess",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "Orders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "Orders",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "OfficeCountry",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "OfficeCountry",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "Customer",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "Customer",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "CustClass",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "CustClass",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "UserOfficesAccess",
                newName: "UpdateddBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "ProductPrices",
                newName: "UpdateddBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Orders",
                newName: "UpdateddBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "OfficeCountry",
                newName: "UpdateddBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Customer",
                newName: "UpdateddBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "CustClass",
                newName: "UpdateddBy");

            migrationBuilder.UpdateData(
                table: "UserOfficesAccess",
                keyColumn: "EncodedBy",
                keyValue: null,
                column: "EncodedBy",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "UserOfficesAccess",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "UserOfficesAccess",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "EncodedBy",
                keyValue: null,
                column: "EncodedBy",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "Orders",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "Orders",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "OfficeCountry",
                keyColumn: "EncodedBy",
                keyValue: null,
                column: "EncodedBy",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "OfficeCountry",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "OfficeCountry",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Customer",
                keyColumn: "EncodedBy",
                keyValue: null,
                column: "EncodedBy",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "Customer",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "Customer",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "CustClass",
                keyColumn: "EncodedBy",
                keyValue: null,
                column: "EncodedBy",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "EncodedBy",
                table: "CustClass",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EncodeDate",
                table: "CustClass",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);
        }
    }
}
