using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SkillBridgeConsoleApp.Data;

namespace SkillBridgeConsoleApp
{
    public class UploadHistoricalTrainingPlans
    {
        private static IConfiguration _configuration;
        private readonly ApplicationDbContext _db;

        private const string basePath = "C:\\Work\\SkillBridgeCMS\\SkillBridgeConsoleApp\\SkillBridgeConsoleApp\\Files\\TrainingPlans";

        public UploadHistoricalTrainingPlans(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _db = context;
        }

        public async Task Run()
        {
            var files = System.IO.Directory.EnumerateFiles(basePath);

            foreach (var file in files)
            {
                var fileInfo = new System.IO.FileInfo(file);

                var orgName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf("-")).Trim();

                var orgs = await _db.Organizations.Where(o => o.Name.StartsWith(orgName)).ToListAsync();

                var orgId = 0;

                if (orgs != null && orgs.Count == 1)
                {
                    orgId = orgs[0].Id;
                }
                else
                {
                    var org = orgs.Where(o => o.Name == orgName).ToList();
                    if (org != null && org.Count == 1)
                    { 
                        orgId = org[0].Id; 
                    }
                }

                if (orgId <= 0)
                {
                    if (orgName.IndexOf("Raytheon") > -1)
                    {
                        orgId = 1305;
                    }
                    if (orgName.IndexOf("ProSol") > -1)
                    {
                        orgId = 730;
                    }
                    if (orgName.IndexOf("Jones Lange") > -1)
                    {
                        orgId = 313;
                    }
                }

                if (orgId > 0) {
                    var bytes = await System.IO.File.ReadAllBytesAsync(file);
                    var exists = await _db.Database.SqlQueryRaw<int>($"select Id from OrganizationFiles where FileName = '{fileInfo.Name}'").ToListAsync();
                    if (exists == null || exists.Count == 0)
                    {
                        await _db.Database.ExecuteSqlRawAsync("insert into OrganizationFiles(OrganizationId, FileType, FileName, ContentType, ContentLength, Blob, CreateDate, CreateBy, IsActive) values(@OrganizationId, 'Historical Training Plan', @FileName, 'application/pdf', @ContentLength, @Blob, getdate(), 'Import', 1)"
                            , new SqlParameter("OrganizationId", orgId)
                            , new SqlParameter("FileName", fileInfo.Name)
                            , new SqlParameter("ContentLength", bytes.Length)
                            , new SqlParameter("Blob", bytes)
                        );
                    }
                }
                else
                {
                    Console.WriteLine($"Can't find org {orgName}");
                }
            }

            Console.ReadLine();
        }
    }
}
