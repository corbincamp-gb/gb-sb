using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.Data;
using SkillBridge_System_Prototype.Models;
using SkillBridge_System_Prototype.Util.Global;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;
using SkillBridge_System_Prototype.Models.TrainingPlans;
using NuGet.Protocol.Core.Types;
using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Security.Policy;
using Microsoft.AspNetCore.Http.Extensions;
using Syncfusion.HtmlConverter;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize(Roles = "Admin, Analyst, Service, Organization, Program")]
    public class TrainingPlansController : Controller
    {
        private static IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TrainingPlansController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TrainingPlansController(ILogger<TrainingPlansController> logger, ApplicationDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            return await ListTrainingPlans();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> ListTrainingPlans()
        {
            var repository = new TrainingPlanRepository(_db);

            List<TrainingPlan> trainingPlans = null;

            if (User.IsInRole("Organization") || User.IsInRole("Program"))
            {
                var id = _userManager.GetUserId(User); // Get user id:

                trainingPlans = await repository.GetTrainingPlansByUserIdAsync(id);
            }
            else
            {
                trainingPlans = await repository.GetAllTrainingPlansAsync();
            }

            return View("~/views/trainingplans/listtrainingplans.cshtml", trainingPlans);
        }

        public async Task<IActionResult> Historical()
        {
            var _organizationFileRepository = new OrganizationFileRepository(_db);

            List<OrganizationFile> trainingPlans = null;

            if (User.IsInRole("Organization") || User.IsInRole("Program"))
            {
                var id = _userManager.GetUserId(User); // Get user id:
                var _authenticationRepository = new AuthenticationRepository(_db);

                var orgs = await _authenticationRepository.GetOrganizationsByUser(id);
                trainingPlans = await _organizationFileRepository.GetAllOrganizationFiles(orgs.Select(o => o.Id).ToList(), "Historical Training Plan");
            }
            else
            {
                trainingPlans = await _organizationFileRepository.GetAllOrganizationFiles("Historical Training Plan");
            }

            return View("~/views/trainingplans/historical.cshtml", trainingPlans);
        }

        public async Task<IActionResult> View(int id)
        {
            var repository = new TrainingPlanRepository(_db);
            var trainingPlan = await repository.GetTrainingPlanAsync(id);

            ViewBag.TrainingPlan = trainingPlan;
            ViewBag.InstructionalMethodsList = await _db.InstructionalMethods.OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.TrainingPlanLengthsList = await _db.TrainingPlanLengths.OrderBy(o => o.SortOrder).ToListAsync();
            return View("~/views/trainingplans/view.cshtml", trainingPlan);
        }

        public async Task<IActionResult> ViewHistorical(int id)
        {
            var repository = new OrganizationFileRepository(_db);
            var trainingPlan = await repository.GetFile(id);
            var memoryStream = new MemoryStream(trainingPlan.FileBlob.Blob);

            return new FileStreamResult(memoryStream, trainingPlan.ContentType) { FileDownloadName = trainingPlan.FileName };
        }

        public async Task<IActionResult> DownloadCSV()
        {
            try
            {
                return await ExportTrainingPlans();
            }
            catch
            {
                return Error();
            }
        }

        public async Task<FileStreamResult> ExportTrainingPlans()
        {
            var repository = new TrainingPlanRepository(_db);
            var tps = await repository.GetAllTrainingPlansAsync();

            var result = WriteCsvToMemory(tps);
            var memoryStream = new MemoryStream(result);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddSeconds(2);
            options.Path = "/TrainingPlans";
            _httpContextAccessor.HttpContext.Response.Cookies.Append("trainingPlansDownloadStarted", "1", options);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = "TrainingPlans-FULL-" + DateTime.Today.ToString("MM-dd-yy") + ".csv" };
        }


        public byte[] WriteCsvToMemory(IEnumerable<TrainingPlan> records)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(records);
                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        public string EscCommas(string data)
        {
            string QUOTE = "\"";
            string ESCAPED_QUOTE = "\"\"";
            char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };

            if (data != null)
            {
                if (data.Contains(","))
                {
                    data = String.Format("\"{0}\"", data);
                }

                if (data.Contains(QUOTE))
                    data = data.Replace(QUOTE, ESCAPED_QUOTE);

                if (data.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                    data = QUOTE + data + QUOTE;
            }

            return data;
        }
    }
}