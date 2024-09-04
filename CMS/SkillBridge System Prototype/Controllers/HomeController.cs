using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.Data;
using SkillBridge_System_Prototype.Models;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize]
    public class HomeController : CmsController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db, IHostEnvironment hostEnvironment) : base(roleManager, userManager, db)
        {
            _logger = logger;
            _env = hostEnvironment;
        }

        [Authorize]
        public IActionResult Index()
        {
            var config = _db.SiteConfiguration.FirstOrDefault(m => m.Id == 1);
            // Identity / Account / Login
            // return RedirectToAction("ListOpportunities", "Opportunities");
            ViewBag.NotificationType = config.NotificationType;
            ViewBag.NotificationHTML = config.NotificationHTML;

            ViewBag.Env = _env.IsDevelopment() ? "Test" : "Prod";

            return View();
        }

        public IActionResult NewDesign()
        {
            return View();
        }

        public IActionResult Privacy()  // Privacy
        {
            return View();
        }

        public IActionResult Legal()  // Privacy
        {
            return View();
        }

        public IActionResult UserGuide()  // User Guide
        {
            return View();
        }

        public IActionResult Logo()  // Logo
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
