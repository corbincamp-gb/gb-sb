using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SkillBridge.Business.Model.Db;

namespace SkillBridge.Business.Util.Audit
{
    public interface IAuditDbContext
    {
        DbSet<AuditModel> Audits { get; set; }
        ChangeTracker ChangeTracker { get; }
    }
}
