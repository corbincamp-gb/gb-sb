using Microsoft.EntityFrameworkCore.Migrations;

namespace Skillbridge.Business.Migrations.Migrations
{
    public partial class pendingfieldnullableadd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingProgramChanges",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingProgramChanges",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingProgramAdditions",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingProgramAdditions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingOrganizationChanges",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingOrganizationChanges",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingOpportunityChanges",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "Requires_OSD_Review",
                table: "PendingOpportunityChanges",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingOpportunityAdditions",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

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

            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingProgramChanges",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingProgramAdditions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingOrganizationChanges",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingOpportunityChanges",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Last_Admin_Action_Time",
                table: "PendingOpportunityAdditions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
