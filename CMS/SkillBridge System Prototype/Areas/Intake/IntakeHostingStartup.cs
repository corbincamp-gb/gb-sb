using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

[assembly: HostingStartup(typeof(SkillBridge_System_Prototype.Areas.Intake.IntakeHostingStartup))]
namespace SkillBridge_System_Prototype.Areas.Intake
{
    public class IntakeHostingStartup : IHostingStartup
    {

        public IConfiguration _configuration { get; }

        public IntakeHostingStartup()
        {
        }

        public IntakeHostingStartup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {

            });
        }
    }
}