using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using SkillBridge_System_Prototype.Intake.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;
using IntakeForm.Models;
using Microsoft.AspNetCore.Authorization;
using IntakeForm.Models.Data.Templates;
using System.Collections.Generic;
using SkillBridge_System_Prototype.Areas.Intake.Controllers;
using Skillbridge.Business.Data;
using Skillbridge.Business.Model.Db;

namespace SkillBridge_System_Prototype.Intake.Controllers
{
    [Area("Intake")]
    [Authorize(Roles = "Analyst,Admin,OSD,OSD Reviewer,OSD Signatory")]
    public class AdminController : SkillBridge_System_Prototype.Controllers.CmsController
    {
        private readonly ILogger _logger;
        private readonly IFormRepository _formRepository;
        private readonly ITemplateRepository _templateRepository;

        public AdminController(ILogger<IntakeHomeController> logger, IFormRepository formRepository, ITemplateRepository templateRepository, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db) : base(roleManager, userManager, db)
        {
            _logger = logger;
            _formRepository = formRepository;
            _templateRepository = templateRepository;
        }

        public async Task<IActionResult> Index()
        {
            return await Index(new IntakeForm.Models.View.Admin.AdminSearchModel { LastUpdatedStartingOn = DateTime.Today.AddDays(-14), LastUpdatedEndingOn = DateTime.Today, SortBy = "UpdatedDate", SortOrder = "desc" });
        }

        [HttpPost]
        public async Task<IActionResult> Index(IntakeForm.Models.View.Admin.AdminSearchModel model, string action = "Filter")
        {
            if (action.ToLower() == "reset")
                return await Index();

            ViewBag.Results = await _formRepository.GetEntries(model);

            return View(model);
        }

        public async Task<IActionResult> Review(string zohoTicketId)
        {
            var entry = await _formRepository.GetEntryByZohoTicketId(zohoTicketId);

            if (entry == null) return await Index();

            ViewBag.Entry = entry;
            ViewBag.ProgressBar = await _formRepository.GetEntryResponses(entry.ID);
            ViewBag.FormTemplates = new List<DeserializedFormTemplate>
            {
                await _templateRepository.GetCurrentFormTemplate(Enumerations.TemplateType.MainApplication),
                await _templateRepository.GetCurrentFormTemplate(Enumerations.TemplateType.ProgramForm)
            };

            return View("~/Areas/Intake/Views/Admin/Review.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Analyst")]
        public async Task<IActionResult> SaveAnalystReview(int id, int entryStatusID, string notes)
        {
            var user = (ApplicationUser)ViewBag.User;
            var entry = await _formRepository.MarkAsReviewedByAnalyst(id, entryStatusID, notes ?? String.Empty, $"{user.FirstName} {user.LastName}");
    
            ViewBag.Messages = new List<string> { "This application has been successfully marked as reviewed by the analyst." };
            return await Review(entry.ZohoTicketId);

        }

        [HttpPost]
        [Authorize(Roles = "OSD Reviewer")]
        public async Task<IActionResult> SaveOsdReview(int id, string notes)
        {
            var user = (ApplicationUser)ViewBag.User;
            var entry = await _formRepository.MarkAsReviewedByOsd(id, notes ?? String.Empty, $"{user.FirstName} {user.LastName}");

            ViewBag.Messages = new List<string> { "This application has been successfully marked as reviewed by the OSD reviewer." };
            return await Review(entry.ZohoTicketId);

        }

        [HttpPost]
        [Authorize(Roles = "OSD Signatory")]
        public async Task<IActionResult> MakeDetermination(int id, int entryStatusID, string notes, string rejectionReason)
        {
            var user = (ApplicationUser)ViewBag.User;
            var entry = await _formRepository.MakeDetermination(id, entryStatusID, notes ?? String.Empty, rejectionReason, $"{user.FirstName} {user.LastName}");

            ViewBag.Messages = new List<string> { "This application has been successfully marked as reviewed by the OSD reviewer." };
            return await Review(entry.ZohoTicketId);

        }

        public async Task<IActionResult> StatusTrackingHistory(string zohoTicketId)
        {
            var entry = await _formRepository.GetEntryByZohoTicketId(zohoTicketId);

            if (entry == null) return await Index();

            ViewBag.Entry = entry;

            return View("~/Areas/Intake/Views/Admin/StatusTrackingHistory.cshtml");
        }

    }
}
