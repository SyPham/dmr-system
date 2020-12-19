using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class UpdateGlueIngredientTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlueNameID",
                table: "GlueIngredient",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlueIngredient_GlueNameID",
                table: "GlueIngredient",
                column: "GlueNameID");

            migrationBuilder.AddForeignKey(
                name: "FK_GlueIngredient_GlueName_GlueNameID",
                table: "GlueIngredient",
                column: "GlueNameID",
                principalTable: "GlueName",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GlueIngredient_GlueName_GlueNameID",
                table: "GlueIngredient");

            migrationBuilder.DropIndex(
                name: "IX_GlueIngredient_GlueNameID",
                table: "GlueIngredient");

            migrationBuilder.DropColumn(
                name: "GlueNameID",
                table: "GlueIngredient");
        }
    }
}
