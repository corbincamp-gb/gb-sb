using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkillBridge.CMS.Intake.Data;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;

namespace SkillBridge.CMS.Areas.Intake.Controllers
{
    [Area("Intake")]
    public class IntakeHomeController : SkillBridge.CMS.Controllers.CmsController
    {
        private readonly ILogger _logger;
        private readonly IFormRepository _formRepository;

        public IntakeHomeController(ILogger<IntakeHomeController> logger, IFormRepository formRepository, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db) : base(roleManager, userManager, db)
        {
            _logger = logger;
            _formRepository = formRepository;
        }

        public async Task<IActionResult> Index(string zohoTicketId)
        {
            var model = await _formRepository.GetEntryByZohoTicketId(zohoTicketId);
            
            if (model == null)
            { 
                model = new IntakeForm.Models.Data.Forms.Entry { ZohoTicketId = zohoTicketId }; 
            }

            ViewBag.States = await _formRepository.GetStates();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Initialize(IntakeForm.Models.Data.Forms.Entry model)
        {
            var phoneRegex = new Regex(@"\(\d\d\d\) \d\d\d\-\d\d\d\d(.*)");
            var emailRegex = new Regex(@"(.*)@(.*)\.(.*)");

            if (String.IsNullOrWhiteSpace(model.ZohoTicketId))
            {
                ModelState.AddModelError("ZohoTicketId", "Your submission is missing the Ticket ID. Please contact...");
            }

            if (!phoneRegex.IsMatch(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", $"Your submission for phone number must be in the format (###) ###-####. You entered {model.PhoneNumber}.");
            }

            if (!phoneRegex.IsMatch(model.PocPhoneNumber))
            {
                ModelState.AddModelError("PocPhoneNumber", $"Your submission for point of contact phone number must be in the format (###) ###-####. You entered {model.PocPhoneNumber}.");
            }

            if (!emailRegex.IsMatch(model.PocEmail))
            {
                ModelState.AddModelError("PocEmail", $"Your submission for point of contact email must be in the format myname@mycompany.com. You entered {model.PocEmail}.");
            }

            if (ModelState.IsValid)
            {
                model = await _formRepository.SaveEntry(model);

                return RedirectToAction("Index", "Form", new { model.ZohoTicketId });
            }

            ViewBag.States = await _formRepository.GetStates();

            return View("~/Areas/Intake/Views/IntakeHome/Index.cshtml", model);
        }
    }
}
