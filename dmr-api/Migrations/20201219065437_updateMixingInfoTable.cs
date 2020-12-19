using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updateMixingInfoTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchA",
                table: "MixingInfos");

            migrationBuilder.DropColumn(
                name: "BatchB",
                table: "MixingInfos");

            migrationBuilder.DropColumn(
                name: "BatchC",
                table: "MixingInfos");

            migrationBuilder.DropColumn(
                name: "BatchD",
                table: "MixingInfos");

            migrationBuilder.DropColumn(
                name: "BatchE",
                table: "MixingInfos");

            migrationBuilder.DropColumn(
                name: "ChemicalA",
                table: "MixingInfos");

            migrationBuilder.DropColumn(
                name: "ChemicalB",
                table: "MixingInfos");

            migrationBuilder.DropColumn(
                name: "ChemicalC",
                table: "MixingInfos");

            migrationBuilder.DropColumn(
                name: "ChemicalD",
                table: "MixingInfos");

            migrationBuilder.DropColumn(
                name: "ChemicalE",
                table: "MixingInfos");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatchA",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchB",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchC",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchD",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchE",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChemicalA",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChemicalB",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChemicalC",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChemicalD",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChemicalE",
                table: "MixingInfos",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
