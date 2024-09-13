using Microsoft.EntityFrameworkCore;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;

namespace SkillBridge.Business.Repository
{
    public class MouFileRepository
    {
        private readonly ApplicationDbContext _db;

        public MouFileRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<MouFile> GetMouFile(int id)
        {
            return await _db.MouFiles.Include(o => o.Mou).Include(o => o.FileBlob).FirstOrDefaultAsync(f => f.Id == id && f.IsActive);
        }

        public async Task<MouFile> GetMouFileByMouId(int mouId)
        {
            return await _db.MouFiles.FirstOrDefaultAsync(f => f.MouId == mouId && f.IsActive);
        }

        public async Task<MouFile> SaveMouFile(MouFile model)
        {
            var mouFile = await _db.MouFiles.Include(o => o.FileBlob).FirstOrDefaultAsync(f => (f.MouId == model.MouId || (model.Id > 0 && f.Id == model.Id)) && f.IsActive);

            if (mouFile == null)
            {
                mouFile = new MouFile();
                _db.MouFiles.Add(mouFile);
            }

            if (mouFile.FileBlob == null) mouFile.FileBlob = new MouFileBlob();

            mouFile.MouId = model.MouId;
            mouFile.FileName = model.FileName;
            mouFile.ContentType = model.ContentType;
            mouFile.ContentLength = model.ContentLength;
            mouFile.FileBlob.Blob = model.FileBlob.Blob;
            mouFile.CreateDate = DateTime.Now;
            mouFile.CreateBy = model.CreateBy;
            mouFile.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            return mouFile;
        }

    }
}