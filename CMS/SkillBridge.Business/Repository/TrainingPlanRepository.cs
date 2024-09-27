using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Model.Db.TrainingPlans;

namespace SkillBridge.Business.Repository
{
    public class TrainingPlanRepository
    {
        private readonly ApplicationDbContext _db;

        public TrainingPlanRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<TrainingPlan>> GetAllTrainingPlansAsync()
        {
            return await _db.TrainingPlans
                .Include(o => o.TrainingPlanLength)
                .Include(o => o.TrainingPlanInstructionalMethods)
                .Include("TrainingPlanInstructionalMethods.InstructionalMethod")
                .Include(o => o.ProgramTrainingPlans)
                .Include("ProgramTrainingPlans.Program")
                .ToListAsync();
        }

        public async Task<List<TrainingPlan>> GetTrainingPlansByUserIdAsync(string userId)
        {
            return await _db.TrainingPlans
                .FromSqlRaw("select * from TrainingPlans where Id in (select TrainingPlanId from ProgramTrainingPlans ptp join Programs p on ptp.ProgramId = p.Id join AspNetUserAuthorities a on a.OrganizationId = p.Organization_Id and isnull(a.ProgramId, p.Id) = p.Id and a.ApplicationUserId = @UserId)", new SqlParameter("UserId", userId))
                .Include(o => o.TrainingPlanLength)
                .Include(o => o.TrainingPlanInstructionalMethods)
                .Include("TrainingPlanInstructionalMethods.InstructionalMethod")
                .Include(o => o.ProgramTrainingPlans)
                .Include("ProgramTrainingPlans.Program")
                .OrderBy(o => o.JobTitle)
                .ToListAsync();
        }

        public async Task<List<TrainingPlan>> GetTrainingPlansByOrgIdAsync(int orgId)
        {
            return await _db.TrainingPlans
                .FromSqlRaw("select * from TrainingPlans where Id in (select TrainingPlanId from ProgramTrainingPlans ptp join Programs p on ptp.ProgramId = p.Id and p.Organization_Id = @OrgId)", new SqlParameter("OrgId", orgId))
                .Include(o => o.TrainingPlanLength)
                .Include(o => o.TrainingPlanInstructionalMethods)
                .Include("TrainingPlanInstructionalMethods.InstructionalMethod")
                .Include(o => o.ProgramTrainingPlans)
                .Include("ProgramTrainingPlans.Program")
                .OrderBy(o => o.JobTitle)
                .ToListAsync();
        }

        public async Task<List<ProgramTrainingPlan>> GetProgramTrainingPlansByProgramIdAsync(int programId)
        {
            return await _db.ProgramTrainingPlans
                .Include(o => o.TrainingPlan)
                .Include("TrainingPlan.TrainingPlanInstructionalMethods")
                .Include("TrainingPlan.TrainingPlanInstructionalMethods.InstructionalMethod")
                .Include("TrainingPlan.TrainingPlanLength")
                .Include("TrainingPlan.TrainingPlanBreakdowns")
                .Where(o => o.ProgramId == programId)
                .ToListAsync();
        }

        public async Task<List<TrainingPlan>> GetAllTrainingPlansByProgramIdAsync(int programId)
        {
            return await _db.TrainingPlans
                .Where(o => o.ProgramTrainingPlans.Any(ptp => ptp.ProgramId == programId))
                .Include(o => o.TrainingPlanInstructionalMethods)
                .Include("TrainingPlanInstructionalMethods.InstructionalMethod")
                .Include(o => o.TrainingPlanLength)
                .Include(o => o.TrainingPlanBreakdowns)
                .Include(o => o.ProgramTrainingPlans)
                .Include("ProgramTrainingPlans.Program")
                .ToListAsync();
        }

        public async Task<TrainingPlan> GetTrainingPlanAsync(int trainingPlanId)
        {
            return await _db.TrainingPlans
                .Include(o => o.TrainingPlanLength)
                .Include(o => o.TrainingPlanInstructionalMethods)
                .Include("TrainingPlanInstructionalMethods.InstructionalMethod")
                .Include(o => o.TrainingPlanBreakdowns)
                .Include(o => o.ProgramTrainingPlans)
                .Include("ProgramTrainingPlans.Program")
                .Include("ProgramTrainingPlans.Program.Organization")
                .Where(o => o.Id == trainingPlanId)
                .FirstOrDefaultAsync();
        }

        public async Task<TrainingPlan> SaveTrainingPlanAsync(TrainingPlan model, string userName)
        {
            var tp = await _db.TrainingPlans.FirstOrDefaultAsync(o => o.Id == model.Id);
            var now = DateTime.Now;

            // Create training plan if it doesn't exist
            if (tp == null)
            {
                tp = new TrainingPlan { CreateDate = now, CreateBy = userName };
                _db.TrainingPlans.Add(tp);
            }

            // Update training plan data
            tp.Name = model.Name;
            tp.JobTitle = model.JobTitle;
            tp.Description = model.Description;
            tp.TrainingPlanLengthId = model.TrainingPlanLengthId;
            tp.BreakdownCount = model.BreakdownCount;
            tp.InstructionalModules = model.InstructionalModules;
            tp.WhoDelivers = model.WhoDelivers;
            tp.GradingRubric = model.GradingRubric;
            tp.CredentialsEarned = model.CredentialsEarned;
            tp.IsActive = model.IsActive;
            tp.UpdateDate = now;
            tp.UpdateBy = model.UpdateBy ?? tp.CreateBy;

            await _db.SaveChangesAsync();

            // Sync instructional methods
            await _db.TrainingPlanInstructionalMethods.Where(o => o.TrainingPlanId == tp.Id).DeleteFromQueryAsync();
            foreach (var instructionalMethodModel in model.TrainingPlanInstructionalMethods)
            {
                var tpim = new TrainingPlanInstructionalMethod { TrainingPlanId = tp.Id, InstructionalMethodId = instructionalMethodModel.InstructionalMethodId, OtherText = instructionalMethodModel.OtherText, CreateDate = now, CreateBy = tp.CreateBy };
                _db.TrainingPlanInstructionalMethods.Add(tpim);
            }

            // Sync time breakdowns
            var tpbds = await _db.TrainingPlanBreakdowns.Where(o => o.TrainingPlanId == tp.Id).ToListAsync();
            foreach (var breakdownModel in model.TrainingPlanBreakdowns)
            {
                var tpbd = tpbds.FirstOrDefault(o => o.RowId == breakdownModel.RowId);
                if (tpbd == null)
                {
                    tpbd = new TrainingPlanBreakdown { TrainingPlanId = tp.Id, CreateDate = now, CreateBy = tp.CreateBy, RowId = breakdownModel.RowId };
                    _db.TrainingPlanBreakdowns.Add(tpbd);
                }

                tpbd.TrainingModuleTitle = breakdownModel.TrainingModuleTitle;
                tpbd.LearningObjective = breakdownModel.LearningObjective;
                tpbd.TotalHours = breakdownModel.TotalHours;
                tpbd.UpdateDate = now;
                tpbd.UpdateBy = tp.CreateBy;
            }

            // Delete weeks no longer used
            var breakdowns = model.TrainingPlanBreakdowns.Select(o => o.RowId).ToList();
            await _db.TrainingPlanBreakdowns.Where(o => o.TrainingPlanId == tp.Id && !breakdowns.Contains(o.RowId)).DeleteFromQueryAsync();

            // Save all changes back to DB
            await _db.SaveChangesAsync();

            return tp;
        }

        public async Task SaveTrainingPlanToProgramAsync(int programId, int trainingPlanId, string createBy)
        {
            if (trainingPlanId > 0)
            {
                var now = DateTime.Now;
                await _db.ProgramTrainingPlans.Where(o => o.ProgramId == programId && o.IsActive).UpdateFromQueryAsync(o => new ProgramTrainingPlan { IsActive = false, ActivationChangeDate = now });

                var ptp = new ProgramTrainingPlan { TrainingPlanId = trainingPlanId, ProgramId = programId, CreateDate = now, CreateBy = createBy, IsActive = true, ActivationChangeDate = now };
                _db.ProgramTrainingPlans.Add(ptp);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeactivateProgramTrainingPlanAsync(int trainingPlanId, int programId)
        {
            var rel = await _db.ProgramTrainingPlans.FirstOrDefaultAsync(o => o.TrainingPlanId == trainingPlanId && o.ProgramId == programId);

            if (rel != null)
            {
                rel.IsActive = false;
                rel.ActivationChangeDate = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }

        public async Task ActivateProgramTrainingPlanAsync(int trainingPlanId, int programId)
        {
            var rel = await _db.ProgramTrainingPlans.FirstOrDefaultAsync(o => o.TrainingPlanId == trainingPlanId && o.ProgramId == programId);

            if (rel != null)
            {
                rel.IsActive = true;
                rel.ActivationChangeDate = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }
    }
}