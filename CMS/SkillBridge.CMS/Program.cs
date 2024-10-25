using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Azure.Identity;

namespace SkillBridge.CMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            //.ConfigureAppConfiguration((context, config) =>
            //    {
            //        //var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri") ?? "https://skillbridgecmskeyvault.vault.usgovcloudapi.net/");
            //        //config.AddAzureKeyVault(
            //        //    keyVaultEndpoint,
            //        //    new DefaultAzureCredential(
            //        //            new DefaultAzureCredentialOptions()
            //        //            {
            //        //                TenantId = "2a0f615a-e938-4206-846e-7fd256ce6e88",
            //        //            }
            //        //        )
            //        //    );
            //    }
            //)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}