using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class UpdateStirv2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Stirs_MixingInfoID",
                table: "Stirs",
                column: "MixingInfoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Stirs_MixingInfos_MixingInfoID",
                table: "Stirs",
                column: "MixingInfoID",
                principalTable: "MixingInfos",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stirs_MixingInfos_MixingInfoID",
                table: "Stirs");

            migrationBuilder.DropIndex(
                name: "IX_Stirs_MixingInfoID",
                table: "Stirs");
        }
    }
}
