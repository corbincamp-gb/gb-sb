using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge.Business.Migrations.Migrations
{
    public partial class addingactivetopending : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is_Active",
                table: "PendingProgramChanges",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Is_Active",
                table: "PendingOrganizationChanges",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is_Active",
                table: "PendingProgramChanges");

            migrationBuilder.DropColumn(
                name: "Is_Active",
                table: "PendingOrganizationChanges");
        }
    }
}
