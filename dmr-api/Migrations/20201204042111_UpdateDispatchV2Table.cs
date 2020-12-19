using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class UpdateDispatchV2Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedFinishTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "EstimatedStartTime",
                table: "Dispatches");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Dispatches",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Dispatches");

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedFinishTime",
                table: "Dispatches",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedStartTime",
                table: "Dispatches",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
