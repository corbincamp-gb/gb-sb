using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Skillbridge.Business.Data;
using Skillbridge.Business.Model.Db;

namespace Skillbridge.Business.Repository.Repositories
{
    public class AuthenticationRepository
    {
        private readonly ApplicationDbContext _db;

        public AuthenticationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<OrganizationModel>> GetOrganizationsByUser(string userId)
        {
            return await _db.Organizations.FromSqlRaw("select * from Organizations where Id in (select OrganizationId from AspNetUserAuthorities where ApplicationUserId = @UserId)", new SqlParameter("UserId", userId))
                .ToListAsync();
        }

        public async Task<List<ProgramModel>> GetProgramsByUser(string userId)
        {
            return await _db.Programs.FromSqlRaw("select * from Programs where Organization_Id in (select Id from Organizations where Id in (select OrganizationId from AspNetUserAuthorities where ApplicationUserId = @UserId and ProgramId is null)) or Id in (select ProgramId from AspNetUserAuthorities where ApplicationUserId = @UserId and ProgramId is not null)", new SqlParameter("UserId", userId))
                .ToListAsync();
        }

        public async Task<List<OpportunityModel>> GetOpportunitiesByUser(string userId)
        {
            return await _db.Opportunities.FromSqlRaw("select * from Opportunities where Program_Id in (select Id from Programs where Organization_Id in (select Id from Organizations where Id in (select OrganizationId from AspNetUserAuthorities where ApplicationUserId = @UserId and ProgramId is null)) or Id in (select ProgramId from AspNetUserAuthorities where ApplicationUserId = @UserId and ProgramId is not null))", new SqlParameter("UserId", userId))
                .ToListAsync();
        }

    }
}