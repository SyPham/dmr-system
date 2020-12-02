using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class CreateScalingMachine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScaleMachines",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineType = table.Column<string>(nullable: true),
                    Unit = table.Column<string>(nullable: true),
                    BuildingID = table.Column<int>(nullable: false),
                    MachineID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScaleMachines", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScaleMachines");
        }
    }
}
