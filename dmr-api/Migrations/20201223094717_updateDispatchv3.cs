using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updateDispatchv3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MixingInfoID",
                table: "Dispatches",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_MixingInfos_MixingInfoID",
                table: "Dispatches");

            migrationBuilder.DropIndex(
                name: "IX_Dispatches_MixingInfoID",
                table: "Dispatches");

            migrationBuilder.AlterColumn<int>(
                name: "MixingInfoID",
                table: "Dispatches",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
