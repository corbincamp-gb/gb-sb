using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.ViewModel;
using Skillbridge.Business.Data;
using Skillbridge.Business.Model.Db;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize(Roles = "Admin, Analyst")]
    public class MousController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrganizationsController> _logger;
        private readonly IEmailSender _emailSender;

        public MousController(ILogger<OrganizationsController> logger, IConfiguration configuration, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ListMous", "Mous");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /* All Mous */

        [HttpGet]
        public IActionResult CreateMou()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMouAsync(MouModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save to DB
                    //model.Created_By = "Test";
                    //.Updated_By = "Test";
                    //model.Date_Created = DateTime.Now;
                    //model.Date_Updated = DateTime.Now;
                    _db.Mous.Add(model);
                    _db.SaveChanges();

                    return RedirectToAction("ListMous", "Mous");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " - " + ex.StackTrace);
                }
            }            

            return View();
        }

        [HttpGet]
        public IActionResult ListMous()
        {
            var mous = _db.Mous.ToList();
            var mouFiles = _db.MouFiles.ToList();

            // List of models
            List<MOUViewModel> models = new List<MOUViewModel>();
            
            // For each mou in the DB, look for its related orgs
            foreach(var mou in mous)
            {
                //List<string> orgs = new List<string>();

                // Get orgs related to this MOU Id
                //var relatedOrgs = _db.Organizations.Where(p => p.Name == mou.Organization_Name);

                // Add name of org to list
                //foreach(var o in relatedOrgs)
                //{
                //orgs.Add(o.Name);
                //}

                // Convert list to a string separated by commas
                //string orgList = string.Join(", ", orgs);

                var mouFile = mouFiles.Find(o => o.MouId == mou.Id);

                // Create the view model for this specific MOU
                MOUViewModel model = new MOUViewModel
                {
                    Id = mou.Id,
                    Creation_Date = mou.Creation_Date,
                    Expiration_Date = mou.Expiration_Date,
                    Url = (mouFile != null ? $"{_configuration["SiteUrl"]}/organizations/viewmoufile/{mouFile.Id}" : String.Empty),
                    Service = mou.Service,
                    Is_OSD = mou.Is_OSD,
                    Orgs = mou.Organization_Name
                };

                // Add the view model to the list of models
                models.Add(model);
            }
            
            return View(models);
        }
    }
}
