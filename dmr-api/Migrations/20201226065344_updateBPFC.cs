using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updateBPFC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeleteBy",
                table: "BPFCEstablishes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteTime",
                table: "BPFCEstablishes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsDelete",
                table: "BPFCEstablishes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteBy",
                table: "BPFCEstablishes");

            migrationBuilder.DropColumn(
                name: "DeleteTime",
                table: "BPFCEstablishes");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "BPFCEstablishes");
        }
    }
}
