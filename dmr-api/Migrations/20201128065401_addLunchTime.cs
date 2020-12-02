using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addLunchTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dispatches",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<double>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    LineID = table.Column<int>(nullable: false),
                    MixingInfoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispatches", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Dispatches_Buildings_LineID",
                        column: x => x.LineID,
                        principalTable: "Buildings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dispatches_MixingInfos_MixingInfoID",
                        column: x => x.MixingInfoID,
                        principalTable: "MixingInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LunchTime",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingID = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LunchTime", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LunchTime_Buildings_BuildingID",
                        column: x => x.BuildingID,
                        principalTable: "Buildings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_LineID",
                table: "Dispatches",
                column: "LineID");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_MixingInfoID",
                table: "Dispatches",
                column: "MixingInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_LunchTime_BuildingID",
                table: "LunchTime",
                column: "BuildingID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dispatches");

            migrationBuilder.DropTable(
                name: "LunchTime");
        }
    }
}
