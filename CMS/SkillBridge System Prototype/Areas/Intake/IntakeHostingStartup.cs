using System;
using System.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillBridge_System_Prototype.Intake.Data;

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