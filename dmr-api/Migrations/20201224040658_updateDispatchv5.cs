using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updateDispatchv5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeleteBy",
                table: "Dispatches",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteTime",
                table: "Dispatches",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StationID",
                table: "Dispatches",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteBy",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "DeleteTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "StationID",
                table: "Dispatches");
        }
    }
}
