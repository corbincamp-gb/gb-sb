using Microsoft.EntityFrameworkCore.Migrations;

namespace Skillbridge.Business.Migrations.Migrations
{
    public partial class changingdeliverymethodonprogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Delivery_Method",
                table: "Programs",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "DeliveryMethods",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingProgramDeliveryMethod",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Program_Id = table.Column<int>(nullable: false),
                    Pending_Program_Id = table.Column<int>(nullable: false),
                    Delivery_Method_Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingProgramDeliveryMethod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramDeliveryMethod",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Program_Id = table.Column<int>(nullable: false),
                    Delivery_Method_Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramDeliveryMethod", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryMethods");

            migrationBuilder.DropTable(
                name: "PendingProgramDeliveryMethod");

            migrationBuilder.DropTable(
                name: "ProgramDeliveryMethod");

            migrationBuilder.AlterColumn<int>(
                name: "Delivery_Method",
                table: "Programs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
