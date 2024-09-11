﻿// <auto-generated />

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SkillBridge.CMS.Intake.Data;

namespace SkillBridge.Business.Migrations.Migrations.IntakeForm
{
    [DbContext(typeof(IntakeFormContext))]
    partial class IntakeFormContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.Entry", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Address1")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Address2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Ein")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EntryStatusID")
                        .HasColumnType("int");

                    b.Property<string>("ExternalNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InternalNotes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte>("NumberOfPrograms")
                        .HasColumnType("tinyint");

                    b.Property<string>("OrganizationName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PocEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PocFirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PocLastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PocPhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PocTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ReviewedByAnalyst")
                        .HasColumnType("bit");

                    b.Property<bool>("ReviewedByOsd")
                        .HasColumnType("bit");

                    b.Property<int>("StateId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("SubmissionDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ZipCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ZohoTicketId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("StateId");

                    b.ToTable("Entries");
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.EntryStatusTracking", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AddedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("EntryID")
                        .HasColumnType("int");

                    b.Property<int>("NewEntryStatusID")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("OldEntryStatusID")
                        .HasColumnType("int");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("EntryID");

                    b.ToTable("EntryStatusTracking");
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.File", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ID")
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime2");

                    b.Property<long>("ContentLength")
                        .HasColumnType("bigint");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EntryID")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FileBlob", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ID")
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("Blob")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.HasKey("ID");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.Form", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("EntryID")
                        .HasColumnType("int");

                    b.Property<int>("FormOrder")
                        .HasColumnType("int");

                    b.Property<int>("FormTemplateID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("EntryID");

                    b.HasIndex("FormTemplateID");

                    b.ToTable("Forms");
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FormResponse", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Answer")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FormID")
                        .HasColumnType("int");

                    b.Property<bool>("IsResponseRequired")
                        .HasColumnType("bit");

                    b.Property<int>("PartID")
                        .HasColumnType("int");

                    b.Property<int>("QuestionID")
                        .HasColumnType("int");

                    b.Property<int>("SectionID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("FormID");

                    b.ToTable("FormResponses");
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FormResponseChoice", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("AnswerChoiceID")
                        .HasColumnType("int");

                    b.Property<int>("FormResponseID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("FormResponseID");

                    b.ToTable("FormResponseChoices");
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FormResponseFile", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("FileID")
                        .HasColumnType("int");

                    b.Property<int>("FormResponseID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("FormResponseID");

                    b.ToTable("FormResponseFiles");
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FormResponseRow", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Answer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ColumnID")
                        .HasColumnType("int");

                    b.Property<int>("FormResponseID")
                        .HasColumnType("int");

                    b.Property<int>("RowID")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.HasIndex("FormResponseID");

                    b.ToTable("FormResponseRows");
                });

            //modelBuilder.Entity("IntakeForm.Models.Data.Forms.State", b =>
            //    {
            //        b.Property<int>("Id")
            //            .ValueGeneratedOnAdd()
            //            .HasColumnType("int")
            //            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            //        b.Property<string>("Code")
            //            .IsRequired()
            //            .HasColumnType("nvarchar(max)");

            //        b.Property<string>("Label")
            //            .IsRequired()
            //            .HasColumnType("nvarchar(max)");

            //        b.Property<string>("Name")
            //            .IsRequired()
            //            .HasColumnType("nvarchar(max)");

            //        b.HasKey("Id");

            //        b.ToTable("States");
            //    });

            modelBuilder.Entity("IntakeForm.Models.Data.Templates.FormTemplate", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AddedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("RetiredDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("SerializedFormTemplate")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte>("TemplateTypeID")
                        .HasColumnType("tinyint");

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ID");

                    b.ToTable("FormTemplates");
                });

            modelBuilder.Entity("IntakeForm.Models.View.Forms.ProgressBarState", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("FormID")
                        .HasColumnType("int");

                    b.Property<bool>("IsComplete")
                        .HasColumnType("bit");

                    b.Property<bool>("IsResponseRequired")
                        .HasColumnType("bit");

                    b.Property<int>("PartID")
                        .HasColumnType("int");

                    b.Property<int>("QuestionID")
                        .HasColumnType("int");

                    b.Property<int>("SectionID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("ProgressBarStates");
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.Entry", b =>
                {
                    b.HasOne("IntakeForm.Models.Data.Forms.State", "State")
                        .WithMany()
                        .HasForeignKey("StateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.EntryStatusTracking", b =>
                {
                    b.HasOne("IntakeForm.Models.Data.Forms.Entry", "Entry")
                        .WithMany("EntryStatusTracking")
                        .HasForeignKey("EntryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FileBlob", b =>
                {
                    b.HasOne("IntakeForm.Models.Data.Forms.File", null)
                        .WithOne("FileBlob")
                        .HasForeignKey("IntakeForm.Models.Data.Forms.FileBlob", "ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.Form", b =>
                {
                    b.HasOne("IntakeForm.Models.Data.Forms.Entry", "Entry")
                        .WithMany("Forms")
                        .HasForeignKey("EntryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IntakeForm.Models.Data.Templates.FormTemplate", "FormTemplate")
                        .WithMany()
                        .HasForeignKey("FormTemplateID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FormResponse", b =>
                {
                    b.HasOne("IntakeForm.Models.Data.Forms.Form", null)
                        .WithMany("FormResponses")
                        .HasForeignKey("FormID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FormResponseChoice", b =>
                {
                    b.HasOne("IntakeForm.Models.Data.Forms.FormResponse", null)
                        .WithMany("FormResponseChoices")
                        .HasForeignKey("FormResponseID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FormResponseFile", b =>
                {
                    b.HasOne("IntakeForm.Models.Data.Forms.FormResponse", null)
                        .WithMany("FormResponseFiles")
                        .HasForeignKey("FormResponseID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IntakeForm.Models.Data.Forms.FormResponseRow", b =>
                {
                    b.HasOne("IntakeForm.Models.Data.Forms.FormResponse", null)
                        .WithMany("FormResponseRows")
                        .HasForeignKey("FormResponseID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
