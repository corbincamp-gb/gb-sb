using System;
using System.Collections.Generic;
using SkillBridge_System_Prototype.Models;
using SkillBridge_System_Prototype.Util.Audit;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Linq;
using SkillBridge_System_Prototype.Models.QuestionPro;
using SkillBridge_System_Prototype.Models.TrainingPlans;

namespace SkillBridge_System_Prototype.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IAuditDbContext
    {
        public DbSet<AspNetUserAuthority> AspNetUserAuthorities { get; set; }

        public DbSet<SB_Organization> Organizations { get; set; }
        public DbSet<SB_PendingOrganizationChange> PendingOrganizationChanges { get; set; }
        public DbSet<SB_Program> Programs { get; set; }
        public DbSet<SB_PendingProgramChange> PendingProgramChanges { get; set; }
        public DbSet<SB_PendingProgramAddition> PendingProgramAdditions { get; set; }
        public DbSet<SB_Opportunity> Opportunities { get; set; }
        public DbSet<SB_PendingOpportunityChange> PendingOpportunityChanges { get; set; }
        public DbSet<SB_PendingOpportunityAddition> PendingOpportunityAdditions { get; set; }
        public DbSet<SB_OpportunityGroup> OpportunityGroups { get; set; }
        public DbSet<SB_Mou> Mous { get; set; }
        public DbSet<SB_Audit> Audits { get; set; }

        public DbSet<ParticipationPopulation> ParticipationPopulations { get; set; }
        public DbSet<ProgramParticipationPopulation> ProgramParticipationPopulation { get; set; }
        public DbSet<PendingProgramParticipationPopulation> PendingProgramParticipationPopulation { get; set; }
        public DbSet<PendingProgramAdditionParticipationPopulation> PendingProgramAdditionsParticipationPopulation { get; set; }

        public DbSet<JobFamily> JobFamilies { get; set; }
        public DbSet<ProgramJobFamily> ProgramJobFamily { get; set; }
        public DbSet<PendingProgramJobFamily> PendingProgramJobFamily { get; set; }
        public DbSet<PendingProgramAdditionJobFamily> PendingProgramAdditionsJobFamily { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<ProgramService> ProgramService { get; set; }
        public DbSet<PendingProgramService> PendingProgramService { get; set; }
        public DbSet<PendingProgramAdditionService> PendingProgramAdditionsService { get; set; }

        public DbSet<DeliveryMethod> DeliveryMethods { get; set; }
        public DbSet<ProgramDeliveryMethod> ProgramDeliveryMethod { get; set; }
        public DbSet<PendingProgramDeliveryMethod> PendingProgramDeliveryMethod { get; set; }
        public DbSet<PendingProgramAdditionDeliveryMethod> PendingProgramAdditionsDeliveryMethod { get; set; }

        public DbSet<State> States { get; set; }
        public DbSet<ProgramState> ProgramStates { get; set; }

        public DbSet<SB_SiteConfiguration> SiteConfiguration { get; set; }  // Stores the notification information (and can be used for other site wide options editable by users in the future)

        public DbSet<SB_APIState> APIState { get; set; }

        public DbSet<QPResponse> QPResponses { get; set; }

        public DbSet<QPResponseQuestion> QPResponseQuestions { get; set; }

        public DbSet<QPResponseQuestionAnswer> QPResponseQuestionAnswers { get; set; }

        public DbSet<QPPdf> QPPdfs { get; set; }

        public DbSet<QuestionProPdfModel> QuestionProPdfModels { get; set; }

        public DbSet<ProgramTrainingPlan> ProgramTrainingPlans { get; set; }
        public DbSet<Models.TrainingPlans.InstructionalMethod> InstructionalMethods { get; set; }
        public DbSet<Models.TrainingPlans.TrainingPlan> TrainingPlans { get; set; }
        public DbSet<Models.TrainingPlans.TrainingPlanInstructionalMethod> TrainingPlanInstructionalMethods { get; set; }
        public DbSet<Models.TrainingPlans.TrainingPlanLength> TrainingPlanLengths { get; set; }
        public DbSet<Models.TrainingPlans.TrainingPlanBreakdown> TrainingPlanBreakdowns { get; set; }

        public DbSet<OrganizationFile> OrganizationFiles { get; set; }
        public DbSet<MouFile> MouFiles { get; set; }


        private string _user;
        private string userName;

        private readonly IHttpContextAccessor _httpContextAccessor;

        //public ChangeTracker ChangeTracker { get; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options/*, UserResolverService userService*/, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            /*if(userService != null)
            {
                _user = userService.GetUser();
            }*/

            /*if(httpContextAccessor != null)
            {
                if (httpContextAccessor.HttpContext != null)
                {
                    if (httpContextAccessor.HttpContext.User != null)
                    {
                        userName = httpContextAccessor.HttpContext.User.Identity.Name;
                    }
                }
            }*/

            _httpContextAccessor = httpContextAccessor;

            //userName = _httpContextAccessor.HttpContext.User.Identity.Name;
            
             Database.EnsureCreated();

            Database.SetCommandTimeout(240);
            //Console.WriteLine("================================Database.GetCommandTimeout: " + Database.GetCommandTimeout());
            //SqlCommand.Timeout
        }
       
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            foreach(var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            builder
              .Entity<ProgramJobFamily>()
              .HasOne<SB_Program>()
              .WithMany(d => d.ProgramJobFamilies)
              .HasForeignKey(o => o.Program_Id);

            builder
              .Entity<ProgramDeliveryMethod>()
              .HasOne<SB_Program>()
              .WithMany(d => d.ProgramDeliveryMethods)
              .HasForeignKey(o => o.Program_Id);

            builder
              .Entity<ProgramParticipationPopulation>()
              .HasOne<SB_Program>()
              .WithMany(d => d.ProgramParticipationPopulations)
              .HasForeignKey(o => o.Program_Id);

            builder
              .Entity<ProgramService>()
              .HasOne<SB_Program>()
              .WithMany(d => d.ProgramServices)
              .HasForeignKey(o => o.Program_Id);

            builder
              .Entity<ProgramState>()
              .HasOne<SB_Program>()
              .WithMany(d => d.ProgramStates)
              .HasForeignKey(o => o.Program_Id);

            builder
              .Entity<TrainingPlan>()
              .HasMany(fk => fk.ProgramTrainingPlans)
              .WithOne(pk => pk.TrainingPlan)
              .HasForeignKey(pk => pk.TrainingPlanId)
              .IsRequired(false);

            builder
              .Entity<ProgramTrainingPlan>()
              .HasOne(fk => fk.SB_Program)
              .WithMany(pk => pk.ProgramTrainingPlans)
              .HasForeignKey(pk => pk.ProgramId)
              .IsRequired(false);

            builder
              .Entity<TrainingPlanInstructionalMethod>()
              .HasOne<TrainingPlan>()
              .WithMany(d => d.TrainingPlanInstructionalMethods)
              .HasForeignKey(o => o.TrainingPlanId);

            builder
              .Entity<TrainingPlanBreakdown>()
              .HasOne<TrainingPlan>()
              .WithMany(d => d.TrainingPlanBreakdowns)
              .HasForeignKey(o => o.TrainingPlanId);

            builder.Entity<FileBlob>(
                dob =>
                {
                    dob.Property(o => o.Id).HasColumnName("Id");
                }
            );

            builder.Entity<OrganizationFile>(
                dob =>
                {
                    dob.Property(o => o.Id).HasColumnName("Id");
                    dob.HasOne(o => o.FileBlob).WithOne()
                    .HasForeignKey<FileBlob>(o => o.Id);
                }
            );

            builder.Entity<MouFile>(
                dob =>
                {
                    dob.Property(o => o.Id).HasColumnName("Id");
                    dob.HasOne(o => o.FileBlob).WithOne()
                    .HasForeignKey<MouFileBlob>(o => o.Id);
                }
            );

        }

        public override int SaveChanges()
        {
            new AuditHelper(this).AddAuditLogs(_httpContextAccessor.HttpContext.User.Identity.Name);
            //OnBeforeSaveChanges("");
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            new AuditHelper(this).AddAuditLogs(_httpContextAccessor.HttpContext.User.Identity.Name);
            //OnBeforeSaveChanges("");
            return base.SaveChangesAsync(cancellationToken);
        }

        /*public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            new AuditHelper(this).AddAuditLogs(_user);
            await OnBeforeSaveChangesAsync(_user);
            return await base.SaveChangesAsync(cancellationToken);
        }*/

        /*public virtual async Task<int> SaveChangesAsync(string userId = null)
        {
            OnBeforeSaveChanges(userId);
            var result = await base.SaveChangesAsync();
            return result;
        }*/

        /*public virtual async Task<int> SaveChangesAsync(string userId = null)
        {
            OnBeforeSaveChanges(userId);
            var result = await base.SaveChangesAsync();
            return result;
        }*/

        /*public override int SaveChanges()
        {
            var userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value
            OnBeforeSaveChanges(userId);
            return base.SaveChanges();
        }*/

        /*private void OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is SB_Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;
                var auditEntry = new AuditEntry(entry, userId);
                auditEntry.TableName = entry.Entity.GetType().Name;
                //auditEntry.UserId = userId;
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = Enums.AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = Enums.AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = Enums.AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            foreach (var auditEntry in auditEntries)
            {
                SB_Audit newAuditEntry = new SB_Audit
                {
                    Id = Guid.NewGuid(),
                    AuditDateTimeUtc = DateTime.UtcNow,
                    AuditType = auditEntry.AuditType.ToString(),
                    AuditUser = auditEntry.AuditUser,
                    TableName = auditEntry.TableName,
                    KeyValues = JsonConvert.SerializeObject(auditEntry.KeyValues),
                    OldValues = auditEntry.OldValues.Count == 0 ?
                                      null : JsonConvert.SerializeObject(auditEntry.OldValues),
                    NewValues = auditEntry.NewValues.Count == 0 ?
                                      null : JsonConvert.SerializeObject(auditEntry.NewValues),
                    ChangedColumns = auditEntry.ChangedColumns.Count == 0 ?
                                           null : JsonConvert.SerializeObject(auditEntry.ChangedColumns)
            };

                Audits.Add(newAuditEntry);
            }
        }

        private async Task OnBeforeSaveChangesAsync(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is SB_Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;
                var auditEntry = new AuditEntry(entry, userId);
                auditEntry.TableName = entry.Entity.GetType().Name;
                //auditEntry.UserId = userId;
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = Enums.AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = Enums.AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = Enums.AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            foreach (var auditEntry in auditEntries)
            {
                SB_Audit newAuditEntry = new SB_Audit
                {
                    Id = Guid.NewGuid(),
                    AuditDateTimeUtc = DateTime.UtcNow,
                    AuditType = auditEntry.AuditType.ToString(),
                    AuditUser = auditEntry.AuditUser,
                    TableName = auditEntry.TableName,
                    KeyValues = JsonConvert.SerializeObject(auditEntry.KeyValues),
                    OldValues = auditEntry.OldValues.Count == 0 ?
                                      null : JsonConvert.SerializeObject(auditEntry.OldValues),
                    NewValues = auditEntry.NewValues.Count == 0 ?
                                      null : JsonConvert.SerializeObject(auditEntry.NewValues),
                    ChangedColumns = auditEntry.ChangedColumns.Count == 0 ?
                                           null : JsonConvert.SerializeObject(auditEntry.ChangedColumns)
                };

                Audits.Add(newAuditEntry);
            }
        }*/

        /*private void OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is SB_Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;
                var auditEntry = new AuditEntry(entry, userId);
                auditEntry.TableName = entry.Entity.GetType().Name;
                //auditEntry.UserId = userId;
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = Enums.AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = Enums.AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = Enums.AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            foreach (var auditEntry in auditEntries)
            {
                Audit.Add(auditEntry.ToAudit());
            }
        }*/
    }
}
