using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updateDispatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_MixingInfos_MixingInfoID",
                table: "Dispatches");

            migrationBuilder.AlterColumn<int>(
                name: "MixingInfoID",
                table: "Dispatches",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatches_MixingInfos_MixingInfoID",
                table: "Dispatches",
                column: "MixingInfoID",
                principalTable: "MixingInfos",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_MixingInfos_MixingInfoID",
                table: "Dispatches");

            migrationBuilder.AlterColumn<int>(
                name: "MixingInfoID",
                table: "Dispatches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatches_MixingInfos_MixingInfoID",
                table: "Dispatches",
                column: "MixingInfoID",
                principalTable: "MixingInfos",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
