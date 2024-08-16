using IntakeForm.Models.Data.Forms;
using IntakeForm.Models.Data.Templates;
using IntakeForm.Models.View.Forms;
using Microsoft.EntityFrameworkCore;

namespace SkillBridge_System_Prototype.Intake.Data
{
    public class IntakeFormContext : DbContext
    {
        public IntakeFormContext(DbContextOptions<IntakeFormContext> options) : base(options) { }

        public DbSet<FormTemplate> FormTemplates { get; set; }

        public DbSet<State> States { get; set; }

        public DbSet<Entry> Entries { get; set; }

        public DbSet<EntryStatusTracking> EntryStatusTracking { get; set; }

        public DbSet<IntakeForm.Models.Data.Forms.File> Files { get; set; }

        public DbSet<IntakeForm.Models.Data.Forms.Form> Forms { get; set; }

        public DbSet<FormResponse> FormResponses { get; set; }

        public DbSet<FormResponseChoice> FormResponseChoices { get; set; }

        public DbSet<FormResponseRow> FormResponseRows { get; set; }

        public DbSet<FormResponseFile> FormResponseFiles { get; set; }

        public DbSet<ProgressBarState> ProgressBarStates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FileBlob>(
                dob =>
                {
                    dob.Property(o => o.ID).HasColumnName("ID");
                }
            );

            builder.Entity<IntakeForm.Models.Data.Forms.File>(
                dob =>
                {
                    dob.Property(o => o.ID).HasColumnName("ID");
                    dob.HasOne(o => o.FileBlob).WithOne()
                    .HasForeignKey<FileBlob>(o => o.ID);
                }

            );


        }
    }
}
