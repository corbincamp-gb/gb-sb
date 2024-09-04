using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge_System_Prototype.Migrations
{
    public partial class MovingAzureSubscriptions : Migration
    {
       
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            return;
            //migrationBuilder.DropForeignKey(
            //    name: "FK_ProgramTrainingPlans_TrainingPlans_TrainingPlanId1",
            //    table: "ProgramTrainingPlans");

            //migrationBuilder.DropTable(
            //    name: "OpportunityTrainingPlans");

            //migrationBuilder.DropIndex(
            //    name: "IX_ProgramTrainingPlans_TrainingPlanId1",
            //    table: "ProgramTrainingPlans");

            //migrationBuilder.DropColumn(
            //    name: "Description",
            //    table: "TrainingPlanBreakdowns");

            
            //migrationBuilder.DropColumn(
            //    name: "WeekNumber",
            //    table: "TrainingPlanBreakdowns");

            //migrationBuilder.DropColumn(
            //    name: "TrainingPlanId1",
            //    table: "ProgramTrainingPlans");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TrainingPlans",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LearningObjective",
                table: "TrainingPlanBreakdowns",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowId",
                table: "TrainingPlanBreakdowns",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHours",
                table: "TrainingPlanBreakdowns",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TrainingModuleTitle",
                table: "TrainingPlanBreakdowns",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationChangeDate",
                table: "ProgramTrainingPlans",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProgramTrainingPlans",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SerializedTrainingPlan",
                table: "PendingProgramChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerializedTrainingPlan",
                table: "PendingProgramAdditions",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Other",
                table: "Opportunities",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cost",
                table: "Opportunities",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NotificationDate30Days",
                table: "Mous",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NotificationDate60Days",
                table: "Mous",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NotificationDate90Days",
                table: "Mous",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AspNetUserAuthorities",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(nullable: true),
                    OrganizationId = table.Column<int>(nullable: false),
                    ProgramId = table.Column<int>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserAuthorities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserAuthorities_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserAuthorities_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MouFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MouId = table.Column<int>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    ContentLength = table.Column<long>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    CreateBy = table.Column<string>(nullable: true),
                    Blob = table.Column<byte[]>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MouFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MouFiles_Mous_MouId",
                        column: x => x.MouId,
                        principalTable: "Mous",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(nullable: false),
                    FileType = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    ContentLength = table.Column<long>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    CreateBy = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    Blob = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationFiles_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramTrainingPlans_ProgramId",
                table: "ProgramTrainingPlans",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_Program_Id",
                table: "Opportunities",
                column: "Program_Id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserAuthorities_OrganizationId",
                table: "AspNetUserAuthorities",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserAuthorities_ProgramId",
                table: "AspNetUserAuthorities",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_MouFiles_MouId",
                table: "MouFiles",
                column: "MouId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationFiles_OrganizationId",
                table: "OrganizationFiles",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_Programs_Program_Id",
                table: "Opportunities",
                column: "Program_Id",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramTrainingPlans_Programs_ProgramId",
                table: "ProgramTrainingPlans",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_Programs_Program_Id",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramTrainingPlans_Programs_ProgramId",
                table: "ProgramTrainingPlans");

            migrationBuilder.DropTable(
                name: "AspNetUserAuthorities");

            migrationBuilder.DropTable(
                name: "MouFiles");

            migrationBuilder.DropTable(
                name: "OrganizationFiles");

            migrationBuilder.DropIndex(
                name: "IX_ProgramTrainingPlans_ProgramId",
                table: "ProgramTrainingPlans");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_Program_Id",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TrainingPlans");

            migrationBuilder.DropColumn(
                name: "LearningObjective",
                table: "TrainingPlanBreakdowns");

            migrationBuilder.DropColumn(
                name: "RowId",
                table: "TrainingPlanBreakdowns");

            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "TrainingPlanBreakdowns");

            migrationBuilder.DropColumn(
                name: "TrainingModuleTitle",
                table: "TrainingPlanBreakdowns");

            migrationBuilder.DropColumn(
                name: "ActivationChangeDate",
                table: "ProgramTrainingPlans");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProgramTrainingPlans");

            migrationBuilder.DropColumn(
                name: "SerializedTrainingPlan",
                table: "PendingProgramChanges");

            migrationBuilder.DropColumn(
                name: "SerializedTrainingPlan",
                table: "PendingProgramAdditions");

            migrationBuilder.DropColumn(
                name: "NotificationDate30Days",
                table: "Mous");

            migrationBuilder.DropColumn(
                name: "NotificationDate60Days",
                table: "Mous");

            migrationBuilder.DropColumn(
                name: "NotificationDate90Days",
                table: "Mous");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TrainingPlanBreakdowns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeekNumber",
                table: "TrainingPlanBreakdowns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrainingPlanId1",
                table: "ProgramTrainingPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Other",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Cost",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateTable(
                name: "OpportunityTrainingPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    TrainingPlanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityTrainingPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityTrainingPlans_TrainingPlans_TrainingPlanId",
                        column: x => x.TrainingPlanId,
                        principalTable: "TrainingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramTrainingPlans_TrainingPlanId1",
                table: "ProgramTrainingPlans",
                column: "TrainingPlanId1");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityTrainingPlans_TrainingPlanId",
                table: "OpportunityTrainingPlans",
                column: "TrainingPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramTrainingPlans_TrainingPlans_TrainingPlanId1",
                table: "ProgramTrainingPlans",
                column: "TrainingPlanId1",
                principalTable: "TrainingPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
