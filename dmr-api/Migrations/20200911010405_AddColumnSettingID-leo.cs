using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class AddColumnSettingIDleo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SettingID",
                table: "Stirs",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SettingID",
                table: "Stirs");
        }
    }
}
