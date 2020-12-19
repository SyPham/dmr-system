using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addGlueTypeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlueTypeID",
                table: "Ingredients",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlueTypes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlueTypes", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_GlueTypeID",
                table: "Ingredients",
                column: "GlueTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_GlueTypes_GlueTypeID",
                table: "Ingredients",
                column: "GlueTypeID",
                principalTable: "GlueTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_GlueTypes_GlueTypeID",
                table: "Ingredients");

            migrationBuilder.DropTable(
                name: "GlueTypes");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_GlueTypeID",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "GlueTypeID",
                table: "Ingredients");
        }
    }
}
