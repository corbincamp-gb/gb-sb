using Microsoft.EntityFrameworkCore;
using SkillBridgeConsoleApp.Models.MouRenewalNotifications;

namespace SkillBridgeConsoleApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Mou> Mous { get; set; }

        public DbSet<Organization> Organizations { get; set; }
    }
}
