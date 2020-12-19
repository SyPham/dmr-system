using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class UpdateSomeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "Dispatches");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishStiringTime",
                table: "Stirs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartStiringTime",
                table: "Stirs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedFinishTime",
                table: "Dispatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedStartTime",
                table: "Dispatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishDispatchingTime",
                table: "Dispatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDispatchingTime",
                table: "Dispatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishStiringTime",
                table: "Stirs");

            migrationBuilder.DropColumn(
                name: "StartStiringTime",
                table: "Stirs");

            migrationBuilder.DropColumn(
                name: "EstimatedFinishTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "EstimatedStartTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "FinishDispatchingTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "StartDispatchingTime",
                table: "Dispatches");

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedTime",
                table: "Dispatches",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
