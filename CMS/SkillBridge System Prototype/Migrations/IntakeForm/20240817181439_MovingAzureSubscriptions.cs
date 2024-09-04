using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SkillBridge_System_Prototype.Migrations.IntakeForm
{
    public partial class MovingAzureSubscriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryID = table.Column<int>(nullable: false),
                    ContentType = table.Column<string>(nullable: false),
                    FileName = table.Column<string>(nullable: false),
                    ContentLength = table.Column<long>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    Blob = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FormTemplates",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateTypeID = table.Column<byte>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false),
                    AddedBy = table.Column<string>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: false),
                    SerializedFormTemplate = table.Column<string>(nullable: false),
                    RetiredDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormTemplates", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ProgressBarStates",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormID = table.Column<int>(nullable: false),
                    PartID = table.Column<int>(nullable: false),
                    SectionID = table.Column<int>(nullable: false),
                    QuestionID = table.Column<int>(nullable: false),
                    IsResponseRequired = table.Column<bool>(nullable: false),
                    IsComplete = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressBarStates", x => x.ID);
                });

            //migrationBuilder.CreateTable(
            //    name: "States",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Code = table.Column<string>(nullable: false),
            //        Name = table.Column<string>(nullable: false),
            //        Label = table.Column<string>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_States", x => x.Id);
            //    });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZohoTicketId = table.Column<string>(nullable: false),
                    EntryStatusID = table.Column<int>(nullable: false),
                    OrganizationName = table.Column<string>(nullable: false),
                    Ein = table.Column<string>(nullable: false),
                    Address1 = table.Column<string>(nullable: false),
                    Address2 = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: false),
                    StateId = table.Column<int>(nullable: false),
                    ZipCode = table.Column<string>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    PocFirstName = table.Column<string>(nullable: false),
                    PocLastName = table.Column<string>(nullable: false),
                    PocTitle = table.Column<string>(nullable: false),
                    PocPhoneNumber = table.Column<string>(nullable: false),
                    PocEmail = table.Column<string>(nullable: false),
                    NumberOfPrograms = table.Column<byte>(nullable: false),
                    SubmissionDate = table.Column<DateTime>(nullable: true),
                    InternalNotes = table.Column<string>(nullable: true),
                    RejectionReason = table.Column<string>(nullable: true),
                    ExternalNotes = table.Column<string>(nullable: true),
                    ReviewedByAnalyst = table.Column<bool>(nullable: false),
                    ReviewedByOsd = table.Column<bool>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Entries_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntryStatusTracking",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryID = table.Column<int>(nullable: false),
                    Role = table.Column<string>(nullable: false),
                    OldEntryStatusID = table.Column<int>(nullable: false),
                    NewEntryStatusID = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false),
                    AddedBy = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryStatusTracking", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EntryStatusTracking_Entries_EntryID",
                        column: x => x.EntryID,
                        principalTable: "Entries",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryID = table.Column<int>(nullable: false),
                    FormTemplateID = table.Column<int>(nullable: false),
                    FormOrder = table.Column<int>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Forms_Entries_EntryID",
                        column: x => x.EntryID,
                        principalTable: "Entries",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Forms_FormTemplates_FormTemplateID",
                        column: x => x.FormTemplateID,
                        principalTable: "FormTemplates",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormResponses",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormID = table.Column<int>(nullable: false),
                    PartID = table.Column<int>(nullable: false),
                    SectionID = table.Column<int>(nullable: false),
                    QuestionID = table.Column<int>(nullable: false),
                    Answer = table.Column<string>(nullable: true),
                    IsResponseRequired = table.Column<bool>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormResponses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FormResponses_Forms_FormID",
                        column: x => x.FormID,
                        principalTable: "Forms",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormResponseChoices",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormResponseID = table.Column<int>(nullable: false),
                    AnswerChoiceID = table.Column<int>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormResponseChoices", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FormResponseChoices_FormResponses_FormResponseID",
                        column: x => x.FormResponseID,
                        principalTable: "FormResponses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormResponseFiles",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormResponseID = table.Column<int>(nullable: false),
                    FileID = table.Column<int>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormResponseFiles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FormResponseFiles_FormResponses_FormResponseID",
                        column: x => x.FormResponseID,
                        principalTable: "FormResponses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormResponseRows",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormResponseID = table.Column<int>(nullable: false),
                    RowID = table.Column<int>(nullable: false),
                    ColumnID = table.Column<int>(nullable: false),
                    Answer = table.Column<string>(nullable: false),
                    AddedDate = table.Column<DateTime>(nullable: false),
                    UpdatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormResponseRows", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FormResponseRows_FormResponses_FormResponseID",
                        column: x => x.FormResponseID,
                        principalTable: "FormResponses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_StateId",
                table: "Entries",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryStatusTracking_EntryID",
                table: "EntryStatusTracking",
                column: "EntryID");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponseChoices_FormResponseID",
                table: "FormResponseChoices",
                column: "FormResponseID");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponseFiles_FormResponseID",
                table: "FormResponseFiles",
                column: "FormResponseID");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponseRows_FormResponseID",
                table: "FormResponseRows",
                column: "FormResponseID");

            migrationBuilder.CreateIndex(
                name: "IX_FormResponses_FormID",
                table: "FormResponses",
                column: "FormID");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_EntryID",
                table: "Forms",
                column: "EntryID");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_FormTemplateID",
                table: "Forms",
                column: "FormTemplateID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntryStatusTracking");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "FormResponseChoices");

            migrationBuilder.DropTable(
                name: "FormResponseFiles");

            migrationBuilder.DropTable(
                name: "FormResponseRows");

            migrationBuilder.DropTable(
                name: "ProgressBarStates");

            migrationBuilder.DropTable(
                name: "FormResponses");

            migrationBuilder.DropTable(
                name: "Forms");

            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "FormTemplates");

            migrationBuilder.DropTable(
                name: "States");
        }
    }
}
