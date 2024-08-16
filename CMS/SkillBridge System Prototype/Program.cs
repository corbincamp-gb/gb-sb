using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Azure.Identity;

namespace SkillBridge_System_Prototype
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                var keyVaultEndpoint = new Uri("https://appsecretdemo.vault.azure.net/"); // IF GETTING URI ERROR, SWAP THIS FOR A VAID URI WHEN MIGRATING
                                                     //Environment.GetEnvironmentVariable("VaultUri")
                                                     //https://skillbridgekeyvault.vault.usgovcloudapi.net/
                                                     //https://sbdevconnectionstring.vault.azure.net/
                config.AddAzureKeyVault(
            keyVaultEndpoint,
            new DefaultAzureCredential(new DefaultAzureCredentialOptions()
            {
                ExcludeManagedIdentityCredential = true
            }));
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
