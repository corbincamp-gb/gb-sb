using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(SkillBridge_System_Prototype.Areas.Identity.IdentityHostingStartup))]
namespace SkillBridge_System_Prototype.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}