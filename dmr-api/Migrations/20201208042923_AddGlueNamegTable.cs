using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class AddGlueNamegTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlueNameID",
                table: "Glues",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlueName",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlueName", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Glues_GlueNameID",
                table: "Glues",
                column: "GlueNameID");

            migrationBuilder.AddForeignKey(
                name: "FK_Glues_GlueName_GlueNameID",
                table: "Glues",
                column: "GlueNameID",
                principalTable: "GlueName",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Glues_GlueName_GlueNameID",
                table: "Glues");

            migrationBuilder.DropTable(
                name: "GlueName");

            migrationBuilder.DropIndex(
                name: "IX_Glues_GlueNameID",
                table: "Glues");

            migrationBuilder.DropColumn(
                name: "GlueNameID",
                table: "Glues");
        }
    }
}
