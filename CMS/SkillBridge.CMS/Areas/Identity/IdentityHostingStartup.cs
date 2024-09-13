using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(SkillBridge.CMS.Areas.Identity.IdentityHostingStartup))]
namespace SkillBridge.CMS.Areas.Identity
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