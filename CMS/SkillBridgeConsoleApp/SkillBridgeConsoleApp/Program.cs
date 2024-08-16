using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SkillBridgeConsoleApp.Data;

namespace SkillBridgeConsoleApp
{
    public class Program
    {
        private static IConfiguration _configuration;

        public async static Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                                 .SetBasePath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
                                 .AddJsonFile("./appsettings.json", optional: false, reloadOnChange: true);


            _configuration = builder.Build();

            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")));

            var serviceProvider = services.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<ApplicationDbContext>();

            var action = String.Empty;

            if (args.Length > 0)
            {
                action = args[0];
            }
            else
            {
                Console.WriteLine("*****************");
                Console.WriteLine("*** FUNCTIONS ***");
                Console.WriteLine("*****************");
                Console.WriteLine("- Upload \"historical\" training plans");
                Console.WriteLine("- Import \"SharePoint\" MOUs");
                Console.Write("\nPlease enter the function you want to run: ");
                action = Console.ReadLine();
            }

            switch (action.ToLower())
            {
                case "historical":
                    var historical = new UploadHistoricalTrainingPlans(_configuration, dbContext);
                    await historical.Run();
                    break;
                case "sharepoint":
                    var uploadSharepointMous = new UploadSharepointMous(_configuration, dbContext);
                    await uploadSharepointMous.Run();
                    break;
            }

            Console.WriteLine("Process is complete...");
        }
    }
}

