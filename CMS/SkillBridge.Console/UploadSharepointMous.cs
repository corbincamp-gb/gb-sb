using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client; // *** Special library installed from NuGet: Microsoft.SharePointOnline.CSOM.dll
using SkillBridgeConsoleApp.Data;
using System.Security;

namespace SkillBridgeConsoleApp
{
    public class UploadSharepointMous
    {
        private static IConfiguration _configuration;
        private readonly ApplicationDbContext _db;

        public UploadSharepointMous(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _db = context;
        }

        // SHAREPOINT CODE FROM https://learn.microsoft.com/en-us/sharepoint/dev/sp-add-ins/using-csom-for-dotnet-standard

        public async Task Run()
        {
            var mous = await _db.Mous.FromSqlRaw($"select Id, Url from Mous where Id not in (select Id from MouFiles)").ToListAsync();

            try
            {
                Uri site = new Uri("https://solidllc.sharepoint.com/sites/OSD");
                string user = "TempSPuser@solidinfodesign.com";

                var password = new SecureString();
                "LUO#H63Oy2a?9BBRl!F#Q~`xpBK!Gf".ToList().ForEach(password.AppendChar);

                // Note: The PnP Sites Core AuthenticationManager class also supports this
                using (var authenticationManager = new SharepointAuthenticationManager())
                {
                    using (var context = authenticationManager.GetContext(site, user, password))
                    {
                        foreach (var mou in mous)
                        {
                            Console.Write($"{mou.Url}");

                            try
                            {
                                var file = context.Web.GetFileByUrl(mou.Url);
                                ClientResult<System.IO.Stream> data = file.OpenBinaryStream();
                                context.Load(file);
                                context.ExecuteQuery();
                                using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                                {
                                    if (data != null)
                                    {
                                        data.Value.CopyTo(mStream);

                                        var blob = mStream.ToArray();

                                        await _db.Database.ExecuteSqlRawAsync("insert into MouFiles(MouId, FileName, ContentType, ContentLength, Blob, CreateDate, CreateBy, IsActive) values(@MouId, @FileName, 'application/pdf', @ContentLength, @Blob, getdate(), 'Import', 1)"
                                            , new SqlParameter("MouId", mou.Id)
                                            , new SqlParameter("FileName", file.Name)
                                            , new SqlParameter("ContentLength", blob.Length)
                                            , new SqlParameter("Blob", blob)
                                        );
                                    }
                                }
                                Console.Write($" SUCCESS\n");
                            }
                            catch (Exception ex)
                            {
                                Console.Write($" FAILURE {ex.Message}\n");
                            }
                        }
                    }
                }
            }
            catch (Exception ex2)
            {
                Console.Write($" FAILURE {ex2}\n");
            }

            Console.ReadLine();
        }
    }

    sealed class SharepointFile
    {
        public string FileName { get; set; }
        public byte[] Blob { get; set; }
    }
}
