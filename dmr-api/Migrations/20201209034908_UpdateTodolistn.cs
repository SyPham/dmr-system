using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class UpdateTodolistn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlueNameID",
                table: "ToDoList",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ToDoList_GlueNameID",
                table: "ToDoList",
                column: "GlueNameID");

            migrationBuilder.AddForeignKey(
                name: "FK_ToDoList_GlueName_GlueNameID",
                table: "ToDoList",
                column: "GlueNameID",
                principalTable: "GlueName",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToDoList_GlueName_GlueNameID",
                table: "ToDoList");

            migrationBuilder.DropIndex(
                name: "IX_ToDoList_GlueNameID",
                table: "ToDoList");

            migrationBuilder.DropColumn(
                name: "GlueNameID",
                table: "ToDoList");
        }
    }
}
