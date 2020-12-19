using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class AddMixingInfoDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MixingInfoDetails",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Batch = table.Column<string>(maxLength: 50, nullable: true),
                    Position = table.Column<string>(maxLength: 2, nullable: true),
                    Amount = table.Column<double>(nullable: false),
                    IngredientID = table.Column<int>(nullable: false),
                    MixingInfoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixingInfoDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MixingInfoDetails_Ingredients_IngredientID",
                        column: x => x.IngredientID,
                        principalTable: "Ingredients",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MixingInfoDetails_MixingInfos_MixingInfoID",
                        column: x => x.MixingInfoID,
                        principalTable: "MixingInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MixingInfoDetails_IngredientID",
                table: "MixingInfoDetails",
                column: "IngredientID");

            migrationBuilder.CreateIndex(
                name: "IX_MixingInfoDetails_MixingInfoID",
                table: "MixingInfoDetails",
                column: "MixingInfoID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MixingInfoDetails");
        }
    }
}
