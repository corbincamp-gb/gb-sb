using Microsoft.Extensions.Configuration;

namespace SkillBridge.Business.Model.Db
{
    public static class GetConString
    {
        public static string ConString()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var config = builder.Build();
            string constring = ConfigurationExtensions.GetConnectionString(config, "AzureConnection");
            return constring;
        }
    }
}
