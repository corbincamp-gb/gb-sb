using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge_System_Prototype.Migrations
{
    public partial class addstatestoorg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "States_Of_Program_Delivery",
                table: "Organizations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "States_Of_Program_Delivery",
                table: "Organizations");
        }
    }
}
