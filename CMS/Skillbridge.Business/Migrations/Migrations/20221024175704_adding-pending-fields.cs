using Microsoft.EntityFrameworkCore.Migrations;

namespace Skillbridge.Business.Migrations.Migrations
{
    public partial class addingpendingfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingProgramChanges",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingProgramAdditions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingOrganizationChanges",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingOpportunityChanges",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingOpportunityAdditions",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Requires_OSD_Review",
                table: "PendingProgramChanges");

            migrationBuilder.DropColumn(
                name: "Requires_OSD_Review",
                table: "PendingProgramAdditions");

            migrationBuilder.DropColumn(
                name: "Requires_OSD_Review",
                table: "PendingOrganizationChanges");

            migrationBuilder.DropColumn(
                name: "Requires_OSD_Review",
                table: "PendingOpportunityChanges");

            migrationBuilder.DropColumn(
                name: "Requires_OSD_Review",
                table: "PendingOpportunityAdditions");
        }
    }
}
