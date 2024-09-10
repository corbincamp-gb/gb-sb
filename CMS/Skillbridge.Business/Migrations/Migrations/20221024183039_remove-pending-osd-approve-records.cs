using Microsoft.EntityFrameworkCore.Migrations;

namespace Skillbridge.Business.Migrations.Migrations
{
    public partial class removependingosdapproverecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingProgramChanges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingProgramAdditions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingOrganizationChanges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingOpportunityChanges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingOpportunityAdditions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
