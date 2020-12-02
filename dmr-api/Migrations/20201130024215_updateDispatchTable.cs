using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updateDispatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedTime",
                table: "Dispatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "Dispatches");
        }
    }
}
