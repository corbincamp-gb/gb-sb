using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.ViewModel;
using Skillbridge.Business.Data;
using Skillbridge.Business.Model.Db;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize(Roles="Administrator, Analyst, Service")]
    public class UtilityController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UtilityController> _logger;
        private readonly IEmailSender _emailSender;

        public UtilityController(ILogger<UtilityController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult GetTicketData()
        {
            return View();
        }

        [HttpGet]
        public IActionResult UpdateZohoTicket(string ticketId)
        {
            Console.WriteLine("tickerId received from powerautomate system... ID is: " + ticketId);

            return null;
        }

        public IActionResult QuestionProPdfs()
        {
            return QuestionProPdfs(DateTime.Today.AddDays(-2), DateTime.Today.AddDays(1), String.Empty, String.Empty);
        }

        [HttpPost]
        public IActionResult QuestionProPdfs(DateTime startDate, DateTime endDate, string ZohoTicketId, string FileName)
        {
            if (endDate == DateTime.MinValue) endDate = DateTime.Today;

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.ZohoTicketId = ZohoTicketId;
            ViewBag.FileName = FileName;

            var pdfs = _db.QuestionProPdfModels.FromSqlRaw($"select p.Id, p.ZohoTicketId, p.CreateDate, p.FileName, r.TimeStamp from QPPdfs p join QPResponses r on p.ResponseId = r.ResponseId where p.ZohoTicketId like @ZohoTicketId and p.FileName like @FileName", 
                new SqlParameter("ZohoTicketId", $"%{ZohoTicketId}%"),
                new SqlParameter("FileName", $"%{FileName}%"))
                .Where(o => o.CreateDate >= startDate && o.CreateDate <= endDate.AddDays(1)).OrderByDescending(o => o.CreateDate).ToList();

            return View(pdfs);
        }

        public IActionResult DownloadQuestionProPdf(int id)
        {
            var pdf = _db.QPPdfs.FirstOrDefault(o => o.Id == id);

            if (pdf != null)
            {
                return File(pdf.Pdf, "application/pdf");
            }

            return QuestionProPdfs();
        }

    }
}
