using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkillBridge.CMS.ViewModel;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;

namespace SkillBridge.CMS.Controllers
{
    [Authorize(Roles="Administrator, Analyst")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ReportsController> _logger;
        private readonly IEmailSender _emailSender;

        public ReportsController(ILogger<ReportsController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
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

        public IActionResult ListExpiringMous(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue)
            {
                startDate = DateTime.Today.AddDays(-30);
            }

            if (!endDate.HasValue)
            {
                endDate = DateTime.Today;
            }

            List<MouModel> mous = _db.Mous.Where(s => s.Expiration_Date >= startDate.Value && s.Expiration_Date <= endDate.Value.AddDays(1).AddSeconds(-1)).ToList();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            // List of models
            List<MOUViewModel> models = new List<MOUViewModel>();

            // For each mou in the DB, look for its related orgs
            foreach (var mou in mous)
            {
                List<string> orgs = new List<string>();

                // Get orgs related to this MOU Id
                var relatedOrgs = _db.Organizations.Where(p => p.Mou_Id == mou.Id);

                // Add name of org to list
                foreach (var o in relatedOrgs)
                {
                    orgs.Add(o.Name);
                }

                // Convert list to a string separated by commas
                string orgList = string.Join(", ", orgs);

                // Create the view model for this specific MOU
                MOUViewModel model = new MOUViewModel
                {
                    Id = mou.Id,
                    Creation_Date = mou.Creation_Date,
                    Expiration_Date = mou.Expiration_Date,
                    Url = mou.Url,
                    Service = mou.Service,
                    Is_OSD = mou.Is_OSD,
                    Orgs = orgList
                };

                // Add the view model to the list of models
                models.Add(model);
            }

            return View(models);
        }


        [HttpGet]
        public IActionResult ListRecentOrganizationChanges()
        {
            int numDays = 30;   // Search this many days in the future for an expiration

            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            List<OrganizationModel> orgs = _db.Organizations.Where(s => s.Date_Updated > pastDate).ToList();

            ViewBag.numDays = 30;

            return View(orgs);
        }

        [HttpPost]
        public IActionResult ListRecentOrganizationChanges(int numDays)
        {
            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            List<OrganizationModel> orgs = _db.Organizations.Where(s => s.Date_Updated > pastDate).ToList();

            ViewBag.numDays = numDays;

            return View(orgs);
        }

        [HttpGet]
        public IActionResult ListRecentProgramChanges()
        {
            int numDays = 30;   // Search this many days in the future for an expiration

            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);
            var progs = _db.Programs.Where(s => s.Date_Updated > pastDate).ToList();

            ViewBag.numDays = 30;

            return View(progs);
        }

        [HttpPost]
        public IActionResult ListRecentProgramChanges(int numDays)
        {
            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            var progs = _db.Programs.Where(s => s.Date_Updated > pastDate).ToList();

            ViewBag.numDays = numDays;

            return View(progs);
        }

        [HttpGet]
        public IActionResult ListRecentOpportunityChanges()
        {
            int numDays = 30;   // Search this many days in the future for an expiration

            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            var opps = _db.Opportunities.Where(s => s.Date_Updated > pastDate).ToList();

            ViewBag.numDays = 30;

            return View(opps);
        }

        [HttpPost]
        public IActionResult ListRecentOpportunityChanges(int numDays)
        {
            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            var opps = _db.Opportunities.Where(s => s.Date_Updated > pastDate).ToList();

            ViewBag.numDays = numDays;

            return View(opps);
        }

        [HttpGet]
        public IActionResult ListRecentOSDOrganizationChanges()
        {
            int numDays = 30;   // Search this many days in the future for an expiration

            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            var orgs = _db.PendingOrganizationChanges.Where(s => s.Requires_OSD_Review == true && s.Last_Admin_Action_Time > pastDate).ToList();

            ViewBag.numDays = 30;

            return View(orgs);
        }

        [HttpPost]
        public IActionResult ListRecentOSDOrganizationChanges(int numDays)
        {
            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            var orgs = _db.PendingOrganizationChanges.Where(s => s.Requires_OSD_Review == true && s.Last_Admin_Action_Time > pastDate).ToList();

            ViewBag.numDays = numDays;

            return View(orgs);
        }

        [HttpGet]
        public IActionResult ListRecentOSDProgramChanges()
        {
            int numDays = 30;   // Search this many days in the future for an expiration

            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            List<PendingProgramChangeModel> progs = _db.PendingProgramChanges.Where(s => s.Requires_OSD_Review == true && s.Last_Admin_Action_Time > pastDate).ToList();

            ViewBag.numDays = 30;

            return View(progs);
        }

        [HttpPost]
        public IActionResult ListRecentOSDProgramChanges(int numDays)
        {
            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            List<PendingProgramChangeModel> progs = _db.PendingProgramChanges.Where(s => s.Requires_OSD_Review == true && s.Last_Admin_Action_Time > pastDate).ToList();

            ViewBag.numDays = numDays;

            return View(progs);
        }

        [HttpGet]
        public IActionResult ListRecentOSDOpportunityChanges()
        {
            int numDays = 30;   // Search this many days in the future for an expiration

            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            var opps = _db.PendingOpportunityChanges.Where(s => s.Requires_OSD_Review == true && s.Last_Admin_Action_Time > pastDate).ToList();

            ViewBag.numDays = 30;

            return View(opps);
        }

        [HttpPost]
        public IActionResult ListRecentOSDOpportunityChanges(int numDays)
        {
            DateTime startDate = DateTime.Now;
            DateTime pastDate = startDate.AddDays(-numDays);

            var opps = _db.PendingOpportunityChanges.Where(s => s.Requires_OSD_Review == true && s.Last_Admin_Action_Time > pastDate).ToList();

            ViewBag.numDays = numDays;

            return View(opps);
        }
    }
}
