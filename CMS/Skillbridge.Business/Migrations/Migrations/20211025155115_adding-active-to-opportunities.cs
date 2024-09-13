using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge.Business.Migrations.Migrations
{
    public partial class addingactivetoopportunities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is_Active",
                table: "Opportunities",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is_Active",
                table: "Opportunities");
        }
    }
}
