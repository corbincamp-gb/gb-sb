using Microsoft.EntityFrameworkCore.Migrations;

namespace Skillbridge.Business.Migrations.Migrations
{
    public partial class tryingtogetadditionsworking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingOpportunityAdditions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Group_Id = table.Column<int>(nullable: false),
                    Organization_Id = table.Column<int>(nullable: false),
                    Opportunity_Id = table.Column<int>(nullable: false),
                    Program_Id = table.Column<int>(nullable: false),
                    Is_Active = table.Column<bool>(nullable: false),
                    Program_Name = table.Column<string>(nullable: true),
                    Opportunity_Url = table.Column<string>(nullable: true),
                    Date_Program_Initiated = table.Column<DateTime>(nullable: false),
                    Date_Created = table.Column<DateTime>(nullable: false),
                    Date_Updated = table.Column<DateTime>(nullable: false),
                    Employer_Poc_Name = table.Column<string>(nullable: true),
                    Employer_Poc_Email = table.Column<string>(nullable: true),
                    Training_Duration = table.Column<string>(nullable: true),
                    Service = table.Column<string>(nullable: true),
                    Delivery_Method = table.Column<string>(nullable: true),
                    Multiple_Locations = table.Column<bool>(nullable: false),
                    Program_Type = table.Column<string>(nullable: true),
                    Job_Families = table.Column<string>(nullable: true),
                    Participation_Populations = table.Column<string>(nullable: true),
                    Support_Cohorts = table.Column<bool>(nullable: false),
                    Enrollment_Dates = table.Column<string>(nullable: true),
                    Mous = table.Column<bool>(nullable: false),
                    Num_Locations = table.Column<int>(nullable: false),
                    Installation = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    Zip = table.Column<string>(maxLength: 5, nullable: true),
                    Lat = table.Column<double>(nullable: false),
                    Long = table.Column<double>(nullable: false),
                    Nationwide = table.Column<bool>(nullable: false),
                    Online = table.Column<bool>(nullable: false),
                    Summary_Description = table.Column<string>(nullable: true),
                    Jobs_Description = table.Column<string>(nullable: true),
                    Links_To_Prospective_Jobs = table.Column<string>(nullable: true),
                    Locations_Of_Prospective_Jobs_By_State = table.Column<string>(nullable: true),
                    Salary = table.Column<string>(nullable: true),
                    Prospective_Job_Labor_Demand = table.Column<string>(nullable: true),
                    Target_Mocs = table.Column<string>(nullable: true),
                    Other_Eligibility_Factors = table.Column<string>(nullable: true),
                    Cost = table.Column<string>(nullable: true),
                    Other = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    Created_By = table.Column<string>(nullable: true),
                    Updated_By = table.Column<string>(nullable: true),
                    For_Spouses = table.Column<bool>(nullable: false),
                    Legacy_Opportunity_Id = table.Column<int>(nullable: false),
                    Legacy_Program_Id = table.Column<int>(nullable: false),
                    Legacy_Provider_Id = table.Column<int>(nullable: false),
                    Pending_Change_Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingOpportunityAdditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingProgramAdditions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Program_Id = table.Column<int>(nullable: false),
                    Is_Active = table.Column<bool>(nullable: false),
                    Program_Name = table.Column<string>(nullable: true),
                    Organization_Name = table.Column<string>(nullable: true),
                    Organization_Id = table.Column<int>(nullable: false),
                    Lhn_Intake_Ticket_Id = table.Column<string>(nullable: true),
                    Has_Intake = table.Column<bool>(nullable: false),
                    Intake_Form_Version = table.Column<string>(nullable: true),
                    Qp_Intake_Submission_Id = table.Column<string>(nullable: true),
                    Location_Details_Available = table.Column<bool>(nullable: false),
                    Has_Consent = table.Column<bool>(nullable: false),
                    Qp_Location_Submission_Id = table.Column<string>(nullable: true),
                    Lhn_Location_Ticket_Id = table.Column<string>(nullable: true),
                    Has_Multiple_Locations = table.Column<bool>(nullable: false),
                    Reporting_Form_2020 = table.Column<bool>(nullable: false),
                    Date_Authorized = table.Column<DateTime>(nullable: false),
                    Mou_Link = table.Column<string>(nullable: true),
                    Mou_Creation_Date = table.Column<DateTime>(nullable: false),
                    Mou_Expiration_Date = table.Column<DateTime>(nullable: false),
                    Nationwide = table.Column<bool>(nullable: false),
                    Online = table.Column<bool>(nullable: false),
                    Participation_Populations = table.Column<string>(nullable: true),
                    Delivery_Method = table.Column<string>(nullable: true),
                    States_Of_Program_Delivery = table.Column<string>(nullable: true),
                    Program_Duration = table.Column<int>(nullable: false),
                    Support_Cohorts = table.Column<bool>(nullable: false),
                    Opportunity_Type = table.Column<string>(nullable: true),
                    Job_Family = table.Column<string>(nullable: true),
                    Services_Supported = table.Column<string>(nullable: true),
                    Enrollment_Dates = table.Column<string>(nullable: true),
                    Date_Created = table.Column<DateTime>(nullable: false),
                    Date_Updated = table.Column<DateTime>(nullable: false),
                    Created_By = table.Column<string>(nullable: true),
                    Updated_By = table.Column<string>(nullable: true),
                    Program_Url = table.Column<string>(nullable: true),
                    Program_Status = table.Column<bool>(nullable: false),
                    Admin_Poc_First_Name = table.Column<string>(nullable: true),
                    Admin_Poc_Last_Name = table.Column<string>(nullable: true),
                    Admin_Poc_Email = table.Column<string>(nullable: true),
                    Admin_Poc_Phone = table.Column<string>(nullable: true),
                    Public_Poc_Name = table.Column<string>(nullable: true),
                    Public_Poc_Email = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    For_Spouses = table.Column<bool>(nullable: false),
                    Legacy_Program_Id = table.Column<int>(nullable: false),
                    Legacy_Provider_Id = table.Column<int>(nullable: false),
                    Pending_Change_Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingProgramAdditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingProgramAdditionsDeliveryMethod",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pending_Program_Id = table.Column<int>(nullable: false),
                    Delivery_Method_Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingProgramAdditionsDeliveryMethod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingProgramAdditionsJobFamily",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pending_Program_Id = table.Column<int>(nullable: false),
                    Job_Family_Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingProgramAdditionsJobFamily", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingProgramAdditionsParticipationPopulation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pending_Program_Id = table.Column<int>(nullable: false),
                    Participation_Population_Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingProgramAdditionsParticipationPopulation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingProgramAdditionsService",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pending_Program_Id = table.Column<int>(nullable: false),
                    Service_Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingProgramAdditionsService", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingOpportunityAdditions");

            migrationBuilder.DropTable(
                name: "PendingProgramAdditions");

            migrationBuilder.DropTable(
                name: "PendingProgramAdditionsDeliveryMethod");

            migrationBuilder.DropTable(
                name: "PendingProgramAdditionsJobFamily");

            migrationBuilder.DropTable(
                name: "PendingProgramAdditionsParticipationPopulation");

            migrationBuilder.DropTable(
                name: "PendingProgramAdditionsService");
        }
    }
}
