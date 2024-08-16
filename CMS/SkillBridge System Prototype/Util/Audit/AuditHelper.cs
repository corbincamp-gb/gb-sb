using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SkillBridge_System_Prototype.Models;

namespace SkillBridge_System_Prototype.Util.Audit
{
    public class AuditHelper
    {
        readonly IAuditDbContext Db;

        public AuditHelper(IAuditDbContext db)
        {
            Db = db;
        }

        public void AddAuditLogs(string userName)
        {
            Db.ChangeTracker.DetectChanges();
            List<AuditEntry> auditEntries = new List<AuditEntry>();
            foreach (EntityEntry entry in Db.ChangeTracker.Entries())
            {
                if (entry.Entity is SB_Audit || entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                {
                    continue;
                }
                var auditEntry = new AuditEntry(entry, userName);
                auditEntries.Add(auditEntry);
            }

            if (auditEntries.Any())
            {
                var logs = auditEntries.Select(x => x.ToAudit());
                Db.Audits.AddRange(logs);
            }
        }
    }
}