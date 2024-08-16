using DocumentFormat.OpenXml.Spreadsheet;
using IntakeForm.Models.Data.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SkillBridge_System_Prototype.Data;
using SkillBridge_System_Prototype.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Repositories
{
    public class AuthenticationRepository
    {
        private readonly ApplicationDbContext _db;

        public AuthenticationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<SB_Organization>> GetOrganizationsByUser(string userId)
        {
            return await _db.Organizations.FromSqlRaw("select * from Organizations where Id in (select OrganizationId from AspNetUserAuthorities where ApplicationUserId = @UserId)", new SqlParameter("UserId", userId))
                .ToListAsync();
        }

        public async Task<List<SB_Program>> GetProgramsByUser(string userId)
        {
            return await _db.Programs.FromSqlRaw("select * from Programs where Organization_Id in (select Id from Organizations where Id in (select OrganizationId from AspNetUserAuthorities where ApplicationUserId = @UserId and ProgramId is null)) or Id in (select ProgramId from AspNetUserAuthorities where ApplicationUserId = @UserId and ProgramId is not null)", new SqlParameter("UserId", userId))
                .ToListAsync();
        }

        public async Task<List<SB_Opportunity>> GetOpportunitiesByUser(string userId)
        {
            return await _db.Opportunities.FromSqlRaw("select * from Opportunities where Program_Id in (select Id from Programs where Organization_Id in (select Id from Organizations where Id in (select OrganizationId from AspNetUserAuthorities where ApplicationUserId = @UserId and ProgramId is null)) or Id in (select ProgramId from AspNetUserAuthorities where ApplicationUserId = @UserId and ProgramId is not null))", new SqlParameter("UserId", userId))
                .ToListAsync();
        }

    }
}