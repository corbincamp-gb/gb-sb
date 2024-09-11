using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Skillbridge.Business.Model.Db;

namespace Skillbridge.Business.Util.Audit
{
    public interface IAuditDbContext
    {
        DbSet<AuditModel> Audits { get; set; }
        ChangeTracker ChangeTracker { get; }
    }
}
