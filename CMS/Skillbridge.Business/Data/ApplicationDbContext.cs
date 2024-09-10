using Skillbridge.Business.Model.Db;
using Skillbridge.Business.Model.Db.TrainingPlans;

namespace Skillbridge.Business.Data
{
    public class ApplicationDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser>, Audit.EntityFramework.IAuditDbContext
    {
        public Microsoft.EntityFrameworkCore.DbSet<AspNetUserAuthorityModel> AspNetUserAuthorities { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<OrganizationModel> Organizations { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<SB_PendingOrganizationChange> PendingOrganizationChanges { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<ProgramModel> Programs { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramChangeModel> PendingProgramChanges { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramAdditionModel> PendingProgramAdditions { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<OpportunityModel> Opportunities { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingOpportunityChangeModel> PendingOpportunityChanges { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingOpportunityAdditionModel> PendingOpportunityAdditions { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<OpportunityGroupModel> OpportunityGroups { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<MouModel> Mous { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<AuditModel> Audits { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<ParticipationPopulation> ParticipationPopulations { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<ProgramParticipationPopulation> ProgramParticipationPopulation { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramParticipationPopulation> PendingProgramParticipationPopulation { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramAdditionParticipationPopulation> PendingProgramAdditionsParticipationPopulation { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<JobFamily> JobFamilies { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<ProgramJobFamily> ProgramJobFamily { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramJobFamily> PendingProgramJobFamily { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramAdditionJobFamily> PendingProgramAdditionsJobFamily { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<Service> Services { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<ProgramService> ProgramService { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramService> PendingProgramService { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramAdditionService> PendingProgramAdditionsService { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<DeliveryMethod> DeliveryMethods { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<ProgramDeliveryMethod> ProgramDeliveryMethod { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramDeliveryMethod> PendingProgramDeliveryMethod { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<PendingProgramAdditionDeliveryMethod> PendingProgramAdditionsDeliveryMethod { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<State> States { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<ProgramState> ProgramStates { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<SiteConfigurationModel> SiteConfiguration { get; set; }  // Stores the notification information (and can be used for other site wide options editable by users in the future)

        public Microsoft.EntityFrameworkCore.DbSet<APIStateModel> APIState { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<Model.Db.QuestionPro.QPResponse> QPResponses { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<Model.Db.QuestionPro.QPResponseQuestion> QPResponseQuestions { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<Model.Db.QuestionPro.QPResponseQuestionAnswer> QPResponseQuestionAnswers { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<Model.Db.QuestionPro.QPPdf> QPPdfs { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<QuestionProPdfModel> QuestionProPdfModels { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<ProgramTrainingPlan> ProgramTrainingPlans { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<InstructionalMethod> InstructionalMethods { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<TrainingPlan> TrainingPlans { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<TrainingPlanInstructionalMethod> TrainingPlanInstructionalMethods { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<TrainingPlanLength> TrainingPlanLengths { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<TrainingPlanBreakdown> TrainingPlanBreakdowns { get; set; }

        public Microsoft.EntityFrameworkCore.DbSet<OrganizationFile> OrganizationFiles { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<MouFile> MouFiles { get; set; }


        private string _user;
        private string userName;

        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        //public ChangeTracker ChangeTracker { get; }
        public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext> options/*, UserResolverService userService*/, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
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

            Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.SetCommandTimeout(240);
            //Console.WriteLine("================================Database.GetCommandTimeout: " + Database.GetCommandTimeout());
            //SqlCommand.Timeout
        }

        protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var foreignKey in Enumerable.SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict;
            }

            builder
              .Entity<ProgramJobFamily>()
              .HasOne<ProgramModel>()
              .WithMany(d => d.ProgramJobFamilies)
              .HasForeignKey(o => o.Program_Id);

            builder
              .Entity<ProgramDeliveryMethod>()
              .HasOne<ProgramModel>()
              .WithMany(d => d.ProgramDeliveryMethods)
              .HasForeignKey(o => o.Program_Id);

            builder
              .Entity<ProgramParticipationPopulation>()
              .HasOne<ProgramModel>()
              .WithMany(d => d.ProgramParticipationPopulations)
              .HasForeignKey(o => o.Program_Id);

            builder
              .Entity<ProgramService>()
              .HasOne<ProgramModel>()
              .WithMany(d => d.ProgramServices)
              .HasForeignKey(o => o.Program_Id);

            builder
              .Entity<ProgramState>()
              .HasOne<ProgramModel>()
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
              .HasOne(fk => fk.Program)
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
                    Microsoft.EntityFrameworkCore.RelationalPropertyBuilderExtensions.HasColumnName("Id");
                }
            );

            builder.Entity<OrganizationFile>(
                dob =>
                {
                    Microsoft.EntityFrameworkCore.RelationalPropertyBuilderExtensions.HasColumnName("Id");
                    dob.HasOne(o => o.FileBlob).WithOne()
                    .HasForeignKey<FileBlob>(o => o.Id);
                }
            );

            builder.Entity<MouFile>(
                dob =>
                {
                    Microsoft.EntityFrameworkCore.RelationalPropertyBuilderExtensions.HasColumnName("Id");
                    dob.HasOne(o => o.FileBlob).WithOne()
                    .HasForeignKey<MouFileBlob>(o => o.Id);
                }
            );

        }

        public override int SaveChanges()
        {
            new global::Skillbridge.Business.Util.Audit.AuditHelper(this).AddAuditLogs(_httpContextAccessor.HttpContext.User.Identity.Name);
            //OnBeforeSaveChanges("");
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            new global::Skillbridge.Business.Util.Audit.AuditHelper(this).AddAuditLogs(_httpContextAccessor.HttpContext.User.Identity.Name);
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
        public void OnScopeCreated(Audit.Core.IAuditScope auditScope)
        {
            throw new NotImplementedException();
        }

        public void OnScopeSaving(Audit.Core.IAuditScope auditScope)
        {
            throw new NotImplementedException();
        }

        public void OnScopeSaved(Audit.Core.IAuditScope auditScope)
        {
            throw new NotImplementedException();
        }

        public string AuditEventType { get; set; }
        public bool AuditDisabled { get; set; }
        public bool IncludeEntityObjects { get; set; }
        public bool ExcludeValidationResults { get; set; }
        public Audit.EntityFramework.AuditOptionMode Mode { get; set; }
        public Audit.Core.AuditDataProvider AuditDataProvider { get; set; }
        public Audit.Core.IAuditScopeFactory AuditScopeFactory { get; set; }
        public Dictionary<string, object> ExtraFields { get; }
        public bool ExcludeTransactionId { get; set; }
        public bool EarlySavingAudit { get; set; }
        public Microsoft.EntityFrameworkCore.DbContext DbContext { get; }
        public Dictionary<Type, Audit.EntityFramework.ConfigurationApi.EfEntitySettings> EntitySettings { get; set; }
        public bool ReloadDatabaseValues { get; set; }
    }
}