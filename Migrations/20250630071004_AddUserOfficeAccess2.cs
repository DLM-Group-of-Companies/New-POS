using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLI_POS.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOfficeAccess2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserOfficeAccess_AspNetUsers_UserId",
                table: "UserOfficeAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_UserOfficeAccess_OfficeCountry_OfficeId",
                table: "UserOfficeAccess");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserOfficeAccess",
                table: "UserOfficeAccess");

            migrationBuilder.RenameTable(
                name: "UserOfficeAccess",
                newName: "UserOfficesAccess");

            migrationBuilder.RenameIndex(
                name: "IX_UserOfficeAccess_UserId",
                table: "UserOfficesAccess",
                newName: "IX_UserOfficesAccess_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserOfficeAccess_OfficeId",
                table: "UserOfficesAccess",
                newName: "IX_UserOfficesAccess_OfficeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserOfficesAccess",
                table: "UserOfficesAccess",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserOfficesAccess_AspNetUsers_UserId",
                table: "UserOfficesAccess",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserOfficesAccess_OfficeCountry_OfficeId",
                table: "UserOfficesAccess",
                column: "OfficeId",
                principalTable: "OfficeCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserOfficesAccess_AspNetUsers_UserId",
                table: "UserOfficesAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_UserOfficesAccess_OfficeCountry_OfficeId",
                table: "UserOfficesAccess");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserOfficesAccess",
                table: "UserOfficesAccess");

            migrationBuilder.RenameTable(
                name: "UserOfficesAccess",
                newName: "UserOfficeAccess");

            migrationBuilder.RenameIndex(
                name: "IX_UserOfficesAccess_UserId",
                table: "UserOfficeAccess",
                newName: "IX_UserOfficeAccess_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserOfficesAccess_OfficeId",
                table: "UserOfficeAccess",
                newName: "IX_UserOfficeAccess_OfficeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserOfficeAccess",
                table: "UserOfficeAccess",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserOfficeAccess_AspNetUsers_UserId",
                table: "UserOfficeAccess",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserOfficeAccess_OfficeCountry_OfficeId",
                table: "UserOfficeAccess",
                column: "OfficeId",
                principalTable: "OfficeCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
