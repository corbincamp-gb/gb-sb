using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SkillBridge_System_Prototype.Models;

namespace SkillBridge_System_Prototype.Util.Audit
{
    public interface IAuditDbContext
    {
        DbSet<SB_Audit> Audits { get; set; }
        ChangeTracker ChangeTracker { get; }
    }
}
