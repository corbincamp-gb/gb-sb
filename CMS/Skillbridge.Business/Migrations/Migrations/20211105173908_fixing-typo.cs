using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge.Business.Migrations.Migrations
{
    public partial class fixingtypo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date_Deativated",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "Date_Deativated",
                table: "Organizations");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date_Deactivated",
                table: "Programs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Date_Deactivated",
                table: "Organizations",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date_Deactivated",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "Date_Deactivated",
                table: "Organizations");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date_Deativated",
                table: "Programs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Date_Deativated",
                table: "Organizations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
