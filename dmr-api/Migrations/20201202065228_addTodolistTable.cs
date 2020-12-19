using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addTodolistTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PrintTime",
                table: "MixingInfos",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ToDoList",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanID = table.Column<int>(nullable: false),
                    MixingInfoID = table.Column<int>(nullable: false),
                    GlueID = table.Column<int>(nullable: false),
                    BuildingID = table.Column<int>(nullable: false),
                    LineID = table.Column<int>(nullable: false),
                    LineName = table.Column<string>(nullable: true),
                    GlueName = table.Column<string>(nullable: true),
                    Supplier = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    StartMixingTime = table.Column<DateTime>(nullable: true),
                    FinishMixingTime = table.Column<DateTime>(nullable: true),
                    StartStirTime = table.Column<DateTime>(nullable: true),
                    FinishStirTime = table.Column<DateTime>(nullable: true),
                    StartDispatchingTime = table.Column<DateTime>(nullable: true),
                    FinishDispatchingTime = table.Column<DateTime>(nullable: true),
                    PrintTime = table.Column<DateTime>(nullable: true),
                    StandardConsumption = table.Column<double>(nullable: false),
                    MixedConsumption = table.Column<double>(nullable: false),
                    DeliveredConsumption = table.Column<double>(nullable: false),
                    EstimatedStartTime = table.Column<DateTime>(nullable: false),
                    EstimatedFinishTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToDoList", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ToDoList_Plans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "Plans",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToDoList_PlanID",
                table: "ToDoList",
                column: "PlanID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToDoList");

            migrationBuilder.DropColumn(
                name: "PrintTime",
                table: "MixingInfos");
        }
    }
}
