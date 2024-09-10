using Microsoft.EntityFrameworkCore.Migrations;

namespace Skillbridge.Business.Migrations.Migrations
{
    public partial class addingmustchangepasswordandupdatingprognumlocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Num_Locations",
                table: "PendingOpportunityChanges",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "Num_Locations",
                table: "PendingOpportunityAdditions",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "Num_Locations",
                table: "Opportunities",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "Num_Locations",
                table: "PendingOpportunityChanges",
                type: "int",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<int>(
                name: "Num_Locations",
                table: "PendingOpportunityAdditions",
                type: "int",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<int>(
                name: "Num_Locations",
                table: "Opportunities",
                type: "int",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
