using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class UpdateSettingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlueTypeID",
                table: "Settings",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_GlueTypeID",
                table: "Settings",
                column: "GlueTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_GlueTypes_GlueTypeID",
                table: "Settings",
                column: "GlueTypeID",
                principalTable: "GlueTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Settings_GlueTypes_GlueTypeID",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_Settings_GlueTypeID",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "GlueTypeID",
                table: "Settings");
        }
    }
}
