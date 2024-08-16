using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge_System_Prototype.Migrations
{
    public partial class addingdatedeactivated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date_Deativated",
                table: "Programs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Is_Active",
                table: "Programs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date_Deativated",
                table: "Organizations",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Is_Active",
                table: "Organizations",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date_Deativated",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "Is_Active",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "Date_Deativated",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "Is_Active",
                table: "Organizations");
        }
    }
}
