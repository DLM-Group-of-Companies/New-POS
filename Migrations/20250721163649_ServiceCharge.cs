using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class ServiceCharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ServiceCharge",
                table: "PaymentMethods",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ServiceChargeAmount",
                table: "OrderPayments",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ServiceChargePercent",
                table: "OrderPayments",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceCharge",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "ServiceChargeAmount",
                table: "OrderPayments");

            migrationBuilder.DropColumn(
                name: "ServiceChargePercent",
                table: "OrderPayments");
        }
    }
}
