using Microsoft.EntityFrameworkCore.Migrations;

namespace Skillbridge.Business.Migrations.Migrations
{
    public partial class fixingactiveopp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is_Active",
                table: "PendingOpportunityChanges",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is_Active",
                table: "PendingOpportunityChanges");
        }
    }
}
