using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge_System_Prototype.Migrations
{
    public partial class addinglabeltostates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "States",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "States");
        }
    }
}
