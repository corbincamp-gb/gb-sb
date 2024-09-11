using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge.Business.Migrations.Migrations
{
    public partial class removingtrainingplan1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrainingPlanId1",
                table: "ProgramTrainingPlans",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramTrainingPlans_TrainingPlanId1",
                table: "ProgramTrainingPlans",
                column: "TrainingPlanId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramTrainingPlans_TrainingPlans_TrainingPlanId1",
                table: "ProgramTrainingPlans",
                column: "TrainingPlanId1",
                principalTable: "TrainingPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgramTrainingPlans_TrainingPlans_TrainingPlanId1",
                table: "ProgramTrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_ProgramTrainingPlans_TrainingPlanId1",
                table: "ProgramTrainingPlans");

            migrationBuilder.DropColumn(
                name: "TrainingPlanId1",
                table: "ProgramTrainingPlans");
        }
    }
}
