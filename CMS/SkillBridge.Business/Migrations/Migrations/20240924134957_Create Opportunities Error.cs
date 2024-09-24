using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge.Business.Migrations.Migrations
{
    public partial class CreateOpportunitiesError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityGroups_Opportunities_Opportunity_Id",
                table: "OpportunityGroups");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityGroups_Opportunity_Id",
                table: "OpportunityGroups");

            migrationBuilder.AddColumn<int>(
                name: "OpportunityId",
                table: "OpportunityGroups",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityGroups_OpportunityId",
                table: "OpportunityGroups",
                column: "OpportunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityGroups_Opportunities_OpportunityId",
                table: "OpportunityGroups",
                column: "OpportunityId",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityGroups_Opportunities_OpportunityId",
                table: "OpportunityGroups");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityGroups_OpportunityId",
                table: "OpportunityGroups");

            migrationBuilder.DropColumn(
                name: "OpportunityId",
                table: "OpportunityGroups");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityGroups_Opportunity_Id",
                table: "OpportunityGroups",
                column: "Opportunity_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityGroups_Opportunities_Opportunity_Id",
                table: "OpportunityGroups",
                column: "Opportunity_Id",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
