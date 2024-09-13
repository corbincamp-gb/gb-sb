using Microsoft.EntityFrameworkCore;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;

namespace SkillBridge.Business.Repository
{
    public class OrganizationFileRepository
    {
        private readonly ApplicationDbContext _db;

        public OrganizationFileRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<OrganizationFile> GetFile(int fileId)
        {
            return await _db.OrganizationFiles.Include(o => o.FileBlob).FirstAsync(o => o.Id == fileId && o.IsActive);
        }

        public async Task<List<OrganizationFile>> GetAllOrganizationFiles()
        {
            return await _db.OrganizationFiles.Include(o => o.Organization).ToListAsync();
        }

        public async Task<List<OrganizationFile>> GetAllOrganizationFiles(string fileType)
        {
            return await _db.OrganizationFiles.Include(o => o.Organization).Where(f => f.FileType == fileType).ToListAsync();
        }

        public async Task<List<OrganizationFile>> GetAllOrganizationFiles(int organizationId)
        {
            return await _db.OrganizationFiles.Include(o => o.Organization).Where(f => f.OrganizationId == organizationId).ToListAsync();
        }

        public async Task<List<OrganizationFile>> GetAllOrganizationFiles(List<int> organizationIds)
        {
            return await _db.OrganizationFiles.Include(o => o.Organization).Where(f => organizationIds.Contains(f.OrganizationId)).ToListAsync();
        }

        public async Task<List<OrganizationFile>> GetAllOrganizationFiles(int organizationId, string fileType)
        {
            return await _db.OrganizationFiles.Include(o => o.Organization).Where(f => f.OrganizationId == organizationId && f.FileType == fileType).ToListAsync();
        }

        public async Task<List<OrganizationFile>> GetAllOrganizationFiles(List<int> organizationIds, string fileType)
        {
            return await _db.OrganizationFiles.Include(o => o.Organization).Where(f => organizationIds.Contains(f.OrganizationId) && f.FileType == fileType).ToListAsync();
        }

    }
}