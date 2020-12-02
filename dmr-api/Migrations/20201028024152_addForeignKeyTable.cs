using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addForeignKeyTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleID",
                table: "UserRoles",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUser_BuildingID",
                table: "BuildingUser",
                column: "BuildingID");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingUser_Buildings_BuildingID",
                table: "BuildingUser",
                column: "BuildingID",
                principalTable: "Buildings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleID",
                table: "UserRoles",
                column: "RoleID",
                principalTable: "Roles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingUser_Buildings_BuildingID",
                table: "BuildingUser");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleID",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_RoleID",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUser_BuildingID",
                table: "BuildingUser");
        }
    }
}
