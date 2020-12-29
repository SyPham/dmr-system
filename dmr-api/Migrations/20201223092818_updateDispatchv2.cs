using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updateDispatchv2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_MixingInfos_MixingInfoID",
                table: "Dispatches");

            migrationBuilder.DropIndex(
                name: "IX_Dispatches_MixingInfoID",
                table: "Dispatches");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_MixingInfoID",
                table: "Dispatches",
                column: "MixingInfoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatches_MixingInfos_MixingInfoID",
                table: "Dispatches",
                column: "MixingInfoID",
                principalTable: "MixingInfos",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
