using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.ViewModel;
using Skillbridge.Business.Data;
using Skillbridge.Business.Model.Db;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize(Roles = "Admin, Analyst")]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrganizationsController> _logger;
        private readonly IEmailSender _emailSender;

        public GroupsController(ILogger<OrganizationsController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ListGroupedOpportunities", "Groups");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /* All Mous */

        [HttpGet]
        public IActionResult CreateGroup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroupAsync(OpportunityGroupModel model)
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
                    _db.OpportunityGroups.Add(model);
                    _db.SaveChanges();

                    return RedirectToAction("ListGroupedOpportunities", "Groups");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " - " + ex.StackTrace);
                }
            }            

            return View();
        }

        [HttpGet]
        public IActionResult ListGroupedOpportunities()
        {
            var groups = _db.OpportunityGroups;
            return View(groups);
        }

        [HttpGet]
        public IActionResult ListOpportunityGroups()
        {
            //var gs = (from og in _db.OpportunityGroups select new { og.Group_Id/*, og.Title*/ }).GroupBy(x =>  x.Group_Id/*, x.Title*/ ).ToList();

            //List<int> ids = _db.OpportunityGroups.Select(x => x.Group_Id).Distinct().ToList();
            List<ListGroupsViewModel> groups = new List<ListGroupsViewModel>();

            /*foreach (int id in ids)
            {
                OpportunityModelGroup group = _db.OpportunityGroups.FirstOrDefault(x => x.Group_Id == id);
            }*/

            //int currentId = -1;
            int lastId = -1;
            string lastTitle = "";                                  // MIGHT NEED THIS, COULD THE RECORDS BE USING THE PREVIOUS ITERATIONS VALUES????
            double lastLat = 0;
            double lastLong = 0;

            /*ListGroupsViewModel model = new ListGroupsViewModel
            {
                Group_Id = -1,
                Title = "",
                Lat = -1,
                Long = -1,
                Opportunities = new List<int>()
            };*/

            List<int> oppList = new List<int>();

            //List<int> oppList = _db.OpportunityGroups.Select(d => d.Opportunity_Id).Distinct().ToList();

            /*foreach (OpportunityModelGroup g in _db.OpportunityGroups.OrderBy(x => x.Group_Id))
            {
                if(lastId == -1)
                {
                    oppList.Clear();
                    oppList = new List<int>();
                    lastId = g.Group_Id;
                    lastTitle = g.Title;
                    lastLat = g.Lat;
                    lastLong = g.Long;
                }
                Console.WriteLine("g.Group_Id: " + g.Group_Id + " lastId: " + lastId);
                if(g.Group_Id != lastId)
                {
                    // Create the model to add
                    ListGroupsViewModel model = new ListGroupsViewModel
                    {
                        Group_Id = lastId,
                        Title = lastTitle,
                        Lat = lastLat,
                        Long = lastLong,
                        Opportunities = new List<int>(oppList)
                    };
                    groups.Add(model);
                    Console.WriteLine("group id " + model.Group_Id + " w title " + model.Title + " should be added to groups");

                    // Update lastId to new id
                    lastId = g.Group_Id;
                    lastTitle = g.Title;
                    lastLat = g.Lat;
                    lastLong = g.Long;

                    // Clear the list out to start over next time
                    oppList.Clear();
                    oppList = new List<int>();
                }
                
                oppList.Add(g.Opportunity_Id);
                Console.WriteLine("Should be adding opportunity");
            }*/

            var groupOpportuntiies = _db.OpportunityGroups.OrderBy(x => x.Group_Id).ToList();

            for (int i=0; i < groupOpportuntiies.Count + 1; i++)    // Go an extra iteration for the last record to show, since we are comparing each item with prev item
            {
                // If this isnt the last record
                if (i != groupOpportuntiies.Count)
                {
                    var g = groupOpportuntiies[i];
                    if (lastId == -1)
                    {
                        oppList.Clear();
                        oppList = new List<int>();
                        lastId = g.Group_Id;
                        lastTitle = g.Title;
                        lastLat = g.Lat;
                        lastLong = g.Long;
                    }

                    Console.WriteLine("g.Group_Id: " + g.Group_Id + " lastId: " + lastId);
                    if (g.Group_Id != lastId)
                    {
                        // Create the model to add
                        ListGroupsViewModel model = new ListGroupsViewModel
                        {
                            Group_Id = lastId,
                            Title = lastTitle,
                            Lat = lastLat,
                            Long = lastLong,
                            Opportunities = new List<int>(oppList)
                        };
                        groups.Add(model);
                        Console.WriteLine("group id " + model.Group_Id + " w title " + model.Title + " should be added to groups");

                        // Update lastId to new id
                        lastId = g.Group_Id;
                        lastTitle = g.Title;
                        lastLat = g.Lat;
                        lastLong = g.Long;

                        // Clear the list out to start over next time
                        oppList.Clear();
                        oppList = new List<int>();
                    }

                    oppList.Add(g.Opportunity_Id);
                }
                else// This is the last record
                {
                    var g = groupOpportuntiies[i - 1];  // get last record since we are over

                    // Create the model to add
                    ListGroupsViewModel model = new ListGroupsViewModel
                    {
                        Group_Id = g.Group_Id,
                        Title = g.Title,
                        Lat = g.Lat,
                        Long = g.Long,
                        Opportunities = new List<int>(oppList)
                    };
                    groups.Add(model);
                    Console.WriteLine("group id " + model.Group_Id + " w title " + model.Title + " should be added to groups");

                    // Clear the list out to start over next time
                    oppList.Clear();
                    oppList = new List<int>();

                    oppList.Add(g.Opportunity_Id);
                }

                Console.WriteLine("Should be adding opportunity");
            }

            /*public int Group_Id { get; set; }   // This is the number that will appear in the data on the site as groupid
            public string Title { get; set; }
            public double Lat { get; set; }
            public double Long { get; set; }
            public List<int> Opportunities { get; set; }*/

            //foreach(var g in gs)
            //{
            /*var count = _db.OpportunityGroups
            .Where(o => o.Group_Id == g.Group_Id)
            .SelectMany(o => o.OpportunityGroups)
            .Count();*/

            /*List<int> oppList = _db.OpportunityGroups.Select(d => d.Opportunity_Id).Distinct().ToList();

            var model = new ListGroupsViewModel
            {
                Group_Id = g.Group_Id,
                Title = "",
                Lat = g.Lat,
                Long = g.Long,                    
                Opportunities = oppList
            };

            groups.Add(model);
        }*/

            Console.WriteLine("======OUPUTTING GROUPS=======");
            foreach(ListGroupsViewModel c in groups)
            {
                Console.WriteLine("Group_Id: " + c.Group_Id);
                Console.WriteLine("Title: " + c.Title);
            }

            return View(groups);
        }

        [HttpGet]
        public IActionResult EditGroup(int id)
        {
            var group = _db.OpportunityGroups.FirstOrDefault(e => e.Group_Id == id);

            var model = new EditGroupViewModel
            {
                Group_Id = group.Group_Id,
                Title = group.Title,
                Lat = group.Lat,
                Long = group.Long
            };

            return View(model);
        }

        // Posting an update to the lat/long for a specific group, and updating its opps
        [HttpPost]
        public async Task<IActionResult> EditGroup(EditGroupViewModel model)
        {
            Console.WriteLine("Saving updated coords to groups with group id: " + model.Group_Id);
            // Update the Opportunity Group Table with the change
            var groupOpps = _db.OpportunityGroups.Where(e => e.Group_Id == model.Group_Id).ToList();

            foreach(var group in groupOpps)
            {
                group.Lat = Math.Round(model.Lat, 8);
                group.Long = Math.Round(model.Long, 8);
                group.Title = model.Title;
                //_db.OpportunityGroups.Update(group);
            }

            // Now, update the individual opportunities
            List<OpportunityModel> opps = _db.Opportunities.Where(e => e.Group_Id == model.Group_Id).ToList();

            foreach (OpportunityModel opp in opps)
            {
                opp.Lat = Math.Round(model.Lat, 8);
                opp.Long = Math.Round(model.Long, 8);
                //_db.Opportunities.Update(opp);
            }

            _db.SaveChanges();

            /*Console.WriteLine("Update result: " + result);

            if (result > 0)
            {
                Console.WriteLine("Saved updated coords");
            }*/

            var newGroup = _db.OpportunityGroups.FirstOrDefault(e => e.Group_Id == model.Group_Id);

            var model2 = new EditGroupViewModel
            {
                Group_Id = newGroup.Group_Id,
                Title = newGroup.Title,
                Lat = newGroup.Lat,
                Long = newGroup.Long
            };

            return View(model2);
        }

        [HttpGet]
        public IActionResult ListGroupOpportunities(int id)
        {
            // Find the opportunities with the specified group id
            var query = from p in _db.Opportunities
                        where p.Group_Id == id
                        orderby p.Group_Id
                        select p;
            var opps = query.ToList<OpportunityModel>();

            ViewBag.GroupId = id;
            if(opps.Count > 0)
            {
                ViewBag.Lat = opps[0].Lat;
                ViewBag.Long = opps[0].Long;
            }

            return View(opps);
        }
    }
}
