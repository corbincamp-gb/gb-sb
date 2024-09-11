using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using SkillBridge.CMS.ViewModel;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using Taku.Core.Global;

namespace SkillBridge.CMS.Controllers
{
    [Authorize(Roles = "Admin, Analyst, Service")]
    public class OpportunitiesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrganizationsController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public OpportunitiesController(ILogger<OrganizationsController> logger, IConfiguration configuration, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _db = db;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ListOpportunities", "Opportunities");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /* All Opportunities */

        public class GroupDropdownItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public double Lat { get; set; }
            public double Long { get; set; }
        }

        public class ProgramDropdownItem
        {
            public int Id { get; set; }
            public string Program_Name { get; set; }
            public string Service { get; set; }
        }

        [HttpGet]
        public IActionResult CreateOpportunity()
        {
            // Get Id/Title combos for opportunity groups, showing only unique ones in the dropdown
            var list = _db.OpportunityGroups.ToList();

            var programList = _db.Programs.ToList();

            List<SelectListItem> dropdownList = new List<SelectListItem>();
            List<GroupDropdownItem> lookupList = new List<GroupDropdownItem>();

            List<ProgramDropdownItem> programLookupList = new List<ProgramDropdownItem>();

            // Populate Dropdown info for States Dropdown
            List<State> states = new List<State>();

            foreach (State s in _db.States)
            {
                states.Add(s);
            };

            states = states.OrderBy(o => o.Code).ToList();

            ViewBag.States = states;

            // Create select list and lookup list (for populating title/lat/long fields on selection)
            for (int i = 0; i < list.Count; i++)
            {
                if (!dropdownList.Any(o => o.Value == list[i].Id.ToString()))
                {
                    dropdownList.Add(new SelectListItem()
                    {
                        Value = list[i].Id.ToString(),
                        Text = list[i].Title == "" ? list[i].Id.ToString() : list[i].Id.ToString() + " - " + list[i].Title
                    });

                    lookupList.Add(new GroupDropdownItem()
                    {
                        Id = list[i].Id,
                        Title = list[i].Title,
                        Lat = list[i].Lat,
                        Long = list[i].Long
                    });
                }
            }

            for (int i = 0; i < programList.Count; i++)
            {
                programLookupList.Add(new ProgramDropdownItem()
                {
                    Id = programList[i].Id,
                    Program_Name = programList[i].Program_Name.Replace("'", "&apos;"),
                    Service = programList[i].Services_Supported
                });
            }

            ViewBag.GroupIds = dropdownList;
            ViewBag.GroupLookup = JsonConvert.SerializeObject(lookupList);
            ViewBag.ProgramLookup = JsonConvert.SerializeObject(programLookupList);

            //ViewBag.GroupIds = ViewBag.GroupIds.Sort();
            //ViewBag.GroupIdsWithTitles = new SelectList(_db.OpportunityGroups.Select(m => m.Group_Id).Distinct().ToList());

            //List<OrganizationModel> orgs = new List<OrganizationModel>();
            List <ProgramModel> progs = new List<ProgramModel>();

            // Populate org and program lists
            /*foreach (OrganizationModel org in _db.Organizations)
            {
                orgs.Add(org);
            };*/

            foreach (ProgramModel prog in _db.Programs)
            {
                progs.Add(prog);
            };

            // Order lists
            //ViewBag.Organizations = orgs.OrderBy(o => o.Name).ToList();
            ViewBag.Programs = progs.OrderBy(o => o.Program_Name).ToList();



            //SelectList groups = new SelectList(_db.OpportunityGroups.Select(l => l.Group_Id).Distinct(), "Group_Id", "Group_Id");
            //ViewBag.GroupIds = groups;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOpportunity(OpportunityModel model, string newGroupTitle)
        {
            //Console.WriteLine("Create Opportunity posted");
            //Console.WriteLine("newGroupTitle: " + newGroupTitle);
            string userName = HttpContext.User.Identity.Name;

            //Console.WriteLine("model.Program_Id: " + model.Program_Id);

            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == model.Program_Id);
            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == prog.Organization_Id);
            //var mou = _db.Mous.FirstOrDefault(e => e.Id == org.Mou_Id);

            //Console.WriteLine("prog: " + prog.Program_Name);
            // Console.WriteLine("org: " + org.Name);
            //Console.WriteLine("ModelState.IsValid: " + ModelState.IsValid);

            string newJobFamilies, newPartPop;

            newJobFamilies = GetJobFamiliesListForProg(prog);
            newPartPop = GetParticipationPopulationStringFromProgram(prog);

            Console.WriteLine("newJobFamilies: " + newJobFamilies);
            Console.WriteLine("newPartPop: " + newPartPop);

            if (ModelState.IsValid)
            {
                //Console.WriteLine("MODEL STATE VALID, TRYING TO CREATE OPPORTUNITY");
                try
                {
                    //TODO: COnvert to mapping
                    var opp = new PendingOpportunityAdditionModel
                    {
                        // Save to DB
                        Created_By = userName,
                        Updated_By = userName,
                        Date_Created = DateTime.Now,
                        Date_Updated = DateTime.Now,
                        Organization_Id = org.Id,
                        Opportunity_Url = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Url),
                        //Organization_Name = org.Name,
                        Date_Program_Initiated = prog.Date_Created,
                        Program_Name = prog.Program_Name,
                        Program_Id = prog.Id,
                        Job_Families = newJobFamilies,    // This is just an optimized field
                        Participation_Populations = newPartPop,   // This is just an optimized field
                        Employer_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Employer_Poc_Email),
                        Employer_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(model.Employer_Poc_Name),
                        Training_Duration = GlobalFunctions.RemoveSpecialCharacters(model.Training_Duration),
                        Delivery_Method = GlobalFunctions.RemoveSpecialCharacters(model.Delivery_Method),
                        Multiple_Locations = model.Multiple_Locations,
                        Program_Type = GlobalFunctions.RemoveSpecialCharacters(model.Program_Type),
                        Support_Cohorts = model.Support_Cohorts,
                        Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(model.Enrollment_Dates),
                        Mous = model.Mous,
                        Num_Locations = model.Num_Locations,
                        Installation = GlobalFunctions.RemoveSpecialCharacters(model.Installation),
                        City = GlobalFunctions.RemoveSpecialCharacters(model.City),
                        State = GlobalFunctions.RemoveSpecialCharacters(model.State),
                        Zip = GlobalFunctions.RemoveSpecialCharacters(model.Zip),
                        Lat = model.Lat,
                        Long = model.Long,
                        Nationwide = prog.Nationwide,
                        Online = prog.Online,
                        Summary_Description = GlobalFunctions.RemoveSpecialCharacters(model.Summary_Description),
                        Jobs_Description = GlobalFunctions.RemoveSpecialCharacters(model.Jobs_Description),
                        Links_To_Prospective_Jobs = GlobalFunctions.RemoveSpecialCharacters(model.Links_To_Prospective_Jobs),
                        Prospective_Job_Labor_Demand = GlobalFunctions.RemoveSpecialCharacters(model.Prospective_Job_Labor_Demand),
                        Locations_Of_Prospective_Jobs_By_State = GlobalFunctions.RemoveSpecialCharacters(model.Locations_Of_Prospective_Jobs_By_State),
                        Salary = GlobalFunctions.RemoveSpecialCharacters(model.Salary),
                        Cost = GlobalFunctions.RemoveSpecialCharacters(model.Cost),
                        Target_Mocs = GlobalFunctions.RemoveSpecialCharacters(model.Target_Mocs),
                        Other_Eligibility_Factors = GlobalFunctions.RemoveSpecialCharacters(model.Other_Eligibility_Factors),
                        Other = GlobalFunctions.RemoveSpecialCharacters(model.Other),
                        Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes),
                        For_Spouses = model.For_Spouses,
                        Is_Active = model.Is_Active,
                        Service = GlobalFunctions.RemoveSpecialCharacters(model.Service),
                        Requires_OSD_Review = true
                        //Admin_Poc_Email = prog.Admin_Poc_Email,
                        //Admin_Poc_First_Name = prog.Admin_Poc_First_Name,
                        //Admin_Poc_Last_Name = prog.Admin_Poc_Last_Name
                    };

                    string newProgType = "";

                    /*
                    < option value = "0" data - select2 - id = "2320" > Department of Labor(DOL) Registered Apprenticeship Program </ option >
          < option value = "1" data - select2 - id = "2321" > DOL Registered Pre-Apprenticeship Program </ option >
                     < option value = "2" data - select2 - id = "2322" > Industry Recognized(Non - DOL - Registered) Pre - Apprenticeship Program </ option >
                                     < option value = "3" data - select2 - id = "2323" > Industry Recognized(Non - DOL - Registered) Apprenticeship Program(IRAP)</ option >
                                                  < option value = "4" data - select2 - id = "2324" > Internship Program </ option >
                                                             < option value = "5" data - select2 - id = "2325" > Employment Skills Training Program </ option >
                                                                        < option value = "6" data - select2 - id = "2326" > Job Training Program</ option >
                    */

                    switch (model.Program_Type)
                    {
                        case "0":
                            newProgType = "Department of Labor(DOL) Registered Apprenticeship Program";
                            break;
                        case "1":
                            newProgType = "DOL Registered Pre-Apprenticeship Program";
                            break;
                        case "2":
                            newProgType = "Industry Recognized(Non - DOL - Registered) Pre - Apprenticeship Program";
                            break;
                        case "3":
                            newProgType = "Industry Recognized(Non - DOL - Registered) Apprenticeship Program(IRAP)";
                            break;
                        case "4":
                            newProgType = "Internship Program";
                            break;
                        case "5":
                            newProgType = "Employment Skills Training Program";
                            break;
                        case "6":
                            newProgType = "Job Training Program";
                            break;
                        default:
                            newProgType = "Job Training Program";
                            break;
                    }

                    opp.Program_Type = newProgType;
                    //opp.Mou_Link = prog.Mou_Link;

                    if(prog.Legacy_Program_Id != 0 && prog.Legacy_Program_Id != -1)
                    {
                        opp.Legacy_Program_Id = prog.Legacy_Program_Id;
                    }
                    else
                    {
                        opp.Legacy_Program_Id = 0;
                    }

                    if (prog.Legacy_Provider_Id != 0 && prog.Legacy_Provider_Id != -1)
                    {
                        opp.Legacy_Provider_Id = prog.Legacy_Provider_Id;
                    }
                    else
                    {
                        opp.Legacy_Provider_Id = 0;
                    }

                    //model.Admin_Poc_Email = model.Admin_Poc_Email;
                    //model.Admin_Poc_Email = model.Admin_Poc_Email;
                    //model.Admin_Poc_Email = model.Admin_Poc_Email;
                    //model.Mou_Link = model.Mou_Link;
                    _db.PendingOpportunityAdditions.Add(opp);
                    var result1 = await _db.SaveChangesAsync();

                    if(result1 > 0)
                    {
                        Console.WriteLine("Result1 returned > 0");

                        /*int newGroupId = model.Group_Id;

                        int maxGroupId = _db.OpportunityGroups.Max(p => p.Group_Id);

                        double newLat = 0f;
                        double newLong = 0f;

                        string newTitle = "";

                        // If we are creating a new group id, find the highest one in the list and increase by one
                        if (model.Group_Id == -1)
                        {
                            newGroupId = maxGroupId + 1;
                            newLat = model.Lat;
                            newLong = model.Long;
                            newTitle = newGroupTitle;
                        }
                        else
                        {
                            //Console.WriteLine("model.Group_Id: " + model.Group_Id);
                            OpportunityModelGroup existingGroup = _db.OpportunityGroups.FirstOrDefault(p => p.Group_Id == model.Group_Id);
                            newLat = existingGroup.Lat;
                            newLong = existingGroup.Long;
                            newTitle = existingGroup.Title;
                        }

                        //Create the opportunity group or add it to an existing one
                        OpportunityModelGroup group = new OpportunityModelGroup
                        {
                            Group_Id = newGroupId,
                            Opportunity_Id = model.Id,
                            Title = newTitle,
                            Lat = newLat,
                            Long = newLong
                        };

                        model.Group_Id = group.Group_Id;*/

                        /*_db.Opportunities.Update(model);
                        //_db.OpportunityGroups.Add(group);
                        var result2 = await _db.SaveChangesAsync();

                        if (result2 > 0)
                        {
                            Console.WriteLine("Result2 returned > 0");
                            List<OpportunityModel> opps = _db.Opportunities.Where(e => e.Program_Id == model.Program_Id).ToList();
                            ProgramModel prog2 = _db.Programs.FirstOrDefault(e => e.Id == model.Program_Id);
                            OrganizationModel org2 = _db.Organizations.FirstOrDefault(e => e.Id == model.Organization_Id);

                            GlobalFunctions.UpdateStatesOfProgramDelivery(prog2, opps, _db);

                            GlobalFunctions.UpdateOrgStatesOfProgramDelivery(org2, _db);*/

                            return RedirectToAction("ListOpportunities", "Opportunities");
                        //}
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " - " + ex.StackTrace);
                }
            }
            else
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();

                foreach(var error in errors)
                {
                    Console.WriteLine("error: " + error);
                }
                
            }


            return RedirectToAction("ListOpportunities", "Opportunities");
        }

        private string GetJobFamiliesListForProg(ProgramModel prog)
        {
            string jfs = "";

            var pjf = _db.ProgramJobFamily.Where(x => x.Program_Id == prog.Id).ToList();

            //Console.WriteLine("==pjf.Count for " + prog.Program_Name + " = " + pjf.Count);

            int count = pjf.Count;
            int i = 0;

            foreach (var jf in pjf)
            {
                Console.WriteLine("jf: " + jf.Job_Family_Id);
                JobFamily fam = _db.JobFamilies.FirstOrDefault(x => x.Id == jf.Job_Family_Id);

                if (i == 0)
                {
                    if (fam != null)
                    {
                        jfs += fam.Name;
                    }
                }
                else if (i < count - 1)
                {
                    if (fam != null)
                    {
                        jfs += ", " + fam.Name;
                    }
                }
                else
                {
                    if (fam != null)
                    {
                        jfs += ", and " + fam.Name;
                    }
                }

                i++;
            }

            Console.WriteLine("GetJobFamiliesListForProg returning " + jfs);

            return jfs;
        }

        private string GetParticipationPopulationStringFromProgram(ProgramModel prog)
        {
            string pps = "";

            var ppps = _db.ProgramParticipationPopulation.Where(x => x.Program_Id == prog.Id).ToList();

            //Console.WriteLine("==pjf.Count for " + prog.Program_Name + " = " + pjf.Count);

            int count = ppps.Count;
            int i = 0;

            foreach (var pp in ppps)
            {
                Console.WriteLine("pp: " + pp.Participation_Population_Id);
                ParticipationPopulation p = _db.ParticipationPopulations.FirstOrDefault(x => x.Id == pp.Participation_Population_Id);

                if (i == 0)
                {
                    if (p != null)
                    {
                        pps += p.Name;
                    }
                }
                else if (i < count - 1)
                {
                    if (p != null)
                    {
                        pps += ", " + p.Name;
                    }
                }
                else
                {
                    if (p != null)
                    {
                        pps += ", and " + p.Name;
                    }
                }

                i++;
            }

            Console.WriteLine("GetParticipationPopulationStringFromProgram returning " + pps);

            return pps;
        }
        /*
        private void UpdateOrgStatesOfProgramDelivery(OrganizationModel org)
        {
            // Get all programs from org
            List<ProgramModel> progs = _db.Programs.Where(e => e.Organization_Id == org.Id).ToList();

            List<string> states = new List<string>();


            foreach (ProgramModel p in progs)
            {
                string progStates = "";
                progStates = p.States_Of_Program_Delivery;
                Console.WriteLine("progStates: " + progStates);

                // Split out each programs states of program delivery, and add them to the states array
                progStates = progStates.Replace(" ", "");
                string[] splitStates = progStates.Split(",");

                foreach (string s in splitStates)
                {
                    if (s != "" && s != " ")
                    {
                        Console.WriteLine("s in splitstates: " + s);
                        bool found = false;

                        foreach (string st in states)
                        {
                            if (s == st)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (found == false)
                        {
                            states.Add(s);
                        }
                    }
                }
            }

            // Sort states alphabetically
            states.Sort();

            // Go through and remove duplicate entries
            int count = 0;
            string orgStates = "";

            foreach (string s in states)
            {
                Console.WriteLine("Checking state s: " + s);

                if (count == 0)
                {
                    orgStates += s;
                }
                else
                {
                    orgStates += ", " + s;
                }

                count++;
            }

            org.States_Of_Program_Delivery = orgStates;

            _db.SaveChanges();
        }

        private void UpdateStatesOfProgramDelivery(ProgramModel prog, List<OpportunityModel> opps)
        {
            // Update Program
            string newStateList = "";
            int num = 0;
            int activeOppsCount = 0;

            List<string> states = new List<string>();

            // Make sure there aren't duplicate states in list
            foreach (OpportunityModel o in opps)
            {
                bool found = false;

                foreach (string s in states)
                {
                    if (s == o.State)
                    {
                        found = true;
                        continue;
                    }
                }

                if (found == false)
                {
                    if (o.State != "" && o.State != " ")
                    {
                        states.Add(o.State);
                    }
                }

                if(o.Is_Active == true)
                {
                    activeOppsCount++;
                }
            }

            // Sort states alphabetically
            states.Sort();

            // Format states in string
            foreach (string s in states)
            {
                if (num == 0)
                {
                    newStateList += s;
                }
                else
                {
                    newStateList += ", " + s;
                }
                num++;
            }

            prog.States_Of_Program_Delivery = newStateList;

            // If more than one active opportunity, or if more than 2 states overall in opps, prog has multiple locations
            if(activeOppsCount > 1 || num > 1)
            {
                prog.Has_Multiple_Locations = true;
            }
            else
            {
                prog.Has_Multiple_Locations = false;
            }

            _db.SaveChanges();
        }
        */

        [HttpGet]
        public IActionResult ListOpportunities()
        {
            var opps = _db.Opportunities;

            //Console.WriteLine("opps.Count: " + opps.Count());

            List<ListOpportunityModel> model = new List<ListOpportunityModel>();

            if(opps.Count() > 0)
            {
                foreach (OpportunityModel opp in opps)
                {
                    //OrganizationModel org = _db.Organizations.SingleOrDefault(x => x.Id == opp.Organization_Id);
                    //ProgramModel prog = _db.Programs.SingleOrDefault(x => x.Id == opp.Program_Id);
                    //var mou = _db.Mous.SingleOrDefault(x => x.Id == org.Mou_Id);

                    string mouLink = "";
                    DateTime expirationDate = new DateTime();
                    string pocFirstName = "";
                    string pocLastName = "";
                    string pocEmail = "";

                    string orgName = opp.Organization_Name;
                    //if(prog != null)
                    //{

                    mouLink = opp.Mou_Link;
                    expirationDate = opp.Mou_Expiration_Date;
                    pocFirstName = opp.Admin_Poc_First_Name;
                    pocLastName = opp.Admin_Poc_Last_Name;
                    pocEmail = opp.Admin_Poc_Email;
                    //}

                    //Console.WriteLine("mouLink: " + mouLink);

                    ListOpportunityModel newOpp = new ListOpportunityModel
                    {
                        Id = opp.Id.ToString(),
                        Is_Active = opp.Is_Active,
                        Program_Name = opp.Program_Name,
                        Organization_Name = orgName,
                        Date_Program_Initiated = opp.Date_Program_Initiated,
                        Mou_Link = mouLink,
                        Mou_Expiration_Date = expirationDate,
                        POC_First_Name = pocFirstName,
                        POC_Last_Name = pocLastName,
                        POC_Email = pocEmail,
                        Employer_Poc_Name = opp.Employer_Poc_Name,
                        Employer_Poc_Email = opp.Employer_Poc_Email,
                        City = opp.City,
                        State = opp.State
                    };

                    model.Add(newOpp);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListOpportunitiesServerSide()
        {
            //var opps = _db.Opportunities;

            //Console.WriteLine("opps.Count: " + opps.Count());

            List<ListOpportunityModel> model = new List<ListOpportunityModel>();

            //if (opps.Count() > 0)
           // {
                /*foreach (OpportunityModel opp in opps)
                {
                    //OrganizationModel org = _db.Organizations.SingleOrDefault(x => x.Id == opp.Organization_Id);
                    //ProgramModel prog = _db.Programs.SingleOrDefault(x => x.Id == opp.Program_Id);
                    //var mou = _db.Mous.SingleOrDefault(x => x.Id == org.Mou_Id);

                    string mouLink = "";
                    DateTime expirationDate = new DateTime();
                    string pocFirstName = "";
                    string pocLastName = "";
                    string pocEmail = "";

                    string orgName = opp.Organization_Name;
                    //if(prog != null)
                    //{

                    mouLink = opp.Mou_Link;
                    expirationDate = opp.Mou_Expiration_Date;
                    pocFirstName = opp.Admin_Poc_First_Name;
                    pocLastName = opp.Admin_Poc_Last_Name;
                    pocEmail = opp.Admin_Poc_Email;
                    //}

                    //Console.WriteLine("mouLink: " + mouLink);

                    ListOpportunityModel newOpp = new ListOpportunityModel
                    {
                        Id = opp.Id.ToString(),
                        Is_Active = opp.Is_Active,
                        Program_Name = opp.Program_Name,
                        Organization_Name = orgName,
                        Date_Program_Initiated = opp.Date_Program_Initiated,
                        Mou_Link = mouLink,
                        Mou_Expiration_Date = expirationDate,
                        POC_First_Name = pocFirstName,
                        POC_Last_Name = pocLastName,
                        POC_Email = pocEmail,
                        Employer_Poc_Name = opp.Employer_Poc_Name,
                        Employer_Poc_Email = opp.Employer_Poc_Email,
                        City = opp.City,
                        State = opp.State
                    };

                    model.Add(newOpp);
                }*/
            //}

            return View(model);
        }

        // Pull the data from the existing Opportunity record
        [HttpGet]
        public async Task<IActionResult> EditOpportunity(string id, bool edit)
        {
            // Find any pending changes for this opportunity
            var pendingChange = _db.PendingOpportunityChanges.FirstOrDefault(e => e.Opportunity_Id == int.Parse(id) && e.Pending_Change_Status == 0);

            // Check for pending change, if it exists, redirect analyst user to the pending change instead
            if (pendingChange != null)
            {
                //Redirect to the approval page instead so analyst can enter changes and approve same time
                return Redirect(Url.Action("ReviewPendingOpportunityChange", "Analyst") + "/" + pendingChange.Id + "?oppId=" + pendingChange.Opportunity_Id);
            }

            // Find the existing Opportunity in the current database
            OpportunityModel opp = _db.Opportunities.FirstOrDefault(e => e.Id == int.Parse(id));
           

            // Populate Dropdown info for States Dropdown
            List<State> states = new List<State>();

            foreach (State s in _db.States)
            {
                states.Add(s);
            };

            states = states.OrderBy(o => o.Code).ToList();

            ViewBag.States = states;

            if (opp == null)
            {
                ViewBag.ErrorMessage = $"Opportunity with id = {id} cannot be found";
                return View("NotFound");
            }

            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == opp.Organization_Id);
            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == opp.Program_Id);

            if (org.Is_Active == false || prog.Is_Active == false)
            {
                ViewBag.Should_Disable_Editing = 1;
            }
            else
            {
                ViewBag.Should_Disable_Editing = 0;
            }

            var model = new EditOpportunityModel
            {
                Id = opp.Id.ToString(),
                Opportunity_Id = opp.Organization_Id,
                Group_Id = opp.GroupId,
                Is_Active = opp.Is_Active,
                Program_Name = opp.Program_Name,
                Opportunity_Url = opp.Opportunity_Url,
                Date_Program_Initiated = opp.Date_Program_Initiated,
                Date_Created = opp.Date_Created, // Date opportunity was created in system
                Date_Updated = opp.Date_Updated, // Date opportunity was last edited/updated in the system
                Employer_Poc_Name = opp.Employer_Poc_Name,
                Employer_Poc_Email = opp.Employer_Poc_Email,
                Training_Duration = opp.Training_Duration,
                Service = opp.Service,
                Delivery_Method = opp.Delivery_Method,
                Multiple_Locations = opp.Multiple_Locations,
                Program_Type = opp.Program_Type,
                Job_Families = opp.Job_Families,
                Participation_Populations = opp.Participation_Populations,
                Support_Cohorts = opp.Support_Cohorts,
                Enrollment_Dates = opp.Enrollment_Dates,
                Mous = opp.Mous,
                Num_Locations = opp.Num_Locations,
                Installation = opp.Installation,
                City = opp.City,
                State = opp.State,
                Zip = opp.Zip,
                Lat = opp.Lat,
                Long = opp.Long,
                Nationwide = opp.Nationwide,
                Online = opp.Online,
                Summary_Description = opp.Summary_Description,
                Jobs_Description = opp.Jobs_Description,
                Links_To_Prospective_Jobs = opp.Links_To_Prospective_Jobs,
                Locations_Of_Prospective_Jobs_By_State = opp.Locations_Of_Prospective_Jobs_By_State,
                Salary = opp.Salary,
                Prospective_Job_Labor_Demand = opp.Prospective_Job_Labor_Demand,
                Target_Mocs = opp.Target_Mocs,
                Other_Eligibility_Factors = opp.Other_Eligibility_Factors,
                Cost = opp.Cost,
                Other = opp.Other,
                Notes = opp.Notes,
                Created_By = opp.Created_By,
                Updated_By = opp.Updated_By,
                For_Spouses = opp.For_Spouses,
                Legacy_Opportunity_Id = opp.Legacy_Opportunity_Id,
                Legacy_Program_Id = opp.Legacy_Program_Id,
                Legacy_Provider_Id = opp.Legacy_Provider_Id,
                Pending_Fields = new List<string>()
            };

            // If we have a pending change we want a way to notify the user
            if (pendingChange != null)
            {
                ViewBag.hasPendingChange = true;

                // Update the model with data from the pending change
                model = UpdateOppModelWithPendingChanges(model, pendingChange);
            }
            else
            {
                ViewBag.hasPendingChange = false;
            }

            return View("~/Views/Opportunities/EditOpportunity.cshtml", model);
        }

        public EditOpportunityModel UpdateOppModelWithPendingChanges(EditOpportunityModel model, PendingOpportunityChangeModel pendingChange)
        {
            if (model.Group_Id != pendingChange.Group_Id) { model.Group_Id = pendingChange.Group_Id; model.Pending_Fields.Add("Group_Id"); }

            if (model.Is_Active != pendingChange.Is_Active) { model.Is_Active = pendingChange.Is_Active; model.Pending_Fields.Add("Is_Active"); }

            if (model.Program_Name != pendingChange.Program_Name)
            {
                if (pendingChange.Program_Name != null && pendingChange.Program_Name != "")
                {
                    if ((String.IsNullOrEmpty(model.Program_Name) == true && String.IsNullOrEmpty(pendingChange.Program_Name) == true) == false)
                    { model.Program_Name = pendingChange.Program_Name; model.Pending_Fields.Add("Program_Name"); }
                }

            }

            if (model.Opportunity_Url != pendingChange.Opportunity_Url)
            {
                if ((String.IsNullOrEmpty(model.Opportunity_Url) == true && String.IsNullOrEmpty(pendingChange.Opportunity_Url) == true) == false)
                { model.Opportunity_Url = pendingChange.Opportunity_Url; model.Pending_Fields.Add("Opportunity_Url"); }
            }

            if (model.Date_Program_Initiated != pendingChange.Date_Program_Initiated) { model.Date_Program_Initiated = pendingChange.Date_Program_Initiated; model.Pending_Fields.Add("Date_Program_Initiated"); }

            if (model.Date_Created != pendingChange.Date_Created) { model.Date_Created = pendingChange.Date_Created; model.Pending_Fields.Add("Date_Created"); }

            if (model.Date_Updated != pendingChange.Date_Updated) { model.Date_Updated = pendingChange.Date_Updated; model.Pending_Fields.Add("Date_Updated"); }

            if (model.Employer_Poc_Name != pendingChange.Employer_Poc_Name)
            {
                if ((String.IsNullOrEmpty(model.Employer_Poc_Name) == true && String.IsNullOrEmpty(pendingChange.Employer_Poc_Name) == true) == false)
                { model.Employer_Poc_Name = pendingChange.Employer_Poc_Name; model.Pending_Fields.Add("Employer_Poc_Name"); }
            }

            if (model.Employer_Poc_Email != pendingChange.Employer_Poc_Email)
            {
                if ((String.IsNullOrEmpty(model.Employer_Poc_Email) == true && String.IsNullOrEmpty(pendingChange.Employer_Poc_Email) == true) == false)
                { model.Employer_Poc_Email = pendingChange.Employer_Poc_Email; model.Pending_Fields.Add("Employer_Poc_Email"); }
            }

            if (model.Training_Duration != pendingChange.Training_Duration)
            {
                if ((String.IsNullOrEmpty(model.Training_Duration) == true && String.IsNullOrEmpty(pendingChange.Training_Duration) == true) == false)
                { model.Training_Duration = pendingChange.Training_Duration; model.Pending_Fields.Add("Training_Duration"); }
            }

            if (model.Service != pendingChange.Service)
            {
                if ((String.IsNullOrEmpty(model.Service) == true && String.IsNullOrEmpty(pendingChange.Service) == true) == false)
                { model.Service = pendingChange.Service; model.Pending_Fields.Add("Service"); }
            }

            //Console.WriteLine("model.Delivery_Method: " + model.Delivery_Method);
            //Console.WriteLine("pendingChange.Delivery_Method: " + pendingChange.Delivery_Method);

            if (model.Delivery_Method != pendingChange.Delivery_Method)
            {
                if ((String.IsNullOrEmpty(model.Delivery_Method) == true && String.IsNullOrEmpty(pendingChange.Delivery_Method) == true) == false)
                { model.Delivery_Method = pendingChange.Delivery_Method; model.Pending_Fields.Add("Delivery_Method"); }
            }

            if (model.Multiple_Locations != pendingChange.Multiple_Locations) { model.Multiple_Locations = pendingChange.Multiple_Locations; model.Pending_Fields.Add("Multiple_Locations"); }

            if (model.Program_Type != pendingChange.Program_Type)
            {
                if ((String.IsNullOrEmpty(model.Program_Type) == true && String.IsNullOrEmpty(pendingChange.Program_Type) == true) == false)
                { model.Program_Type = pendingChange.Program_Type; model.Pending_Fields.Add("Program_Type"); }
            }

            if (model.Job_Families != pendingChange.Job_Families)
            {
                if ((String.IsNullOrEmpty(model.Job_Families) == true && String.IsNullOrEmpty(pendingChange.Job_Families) == true) == false)
                { model.Job_Families = pendingChange.Job_Families; model.Pending_Fields.Add("Job_Families"); }
            }

            if (model.Participation_Populations != pendingChange.Participation_Populations)
            {
                if ((String.IsNullOrEmpty(model.Participation_Populations) == true && String.IsNullOrEmpty(pendingChange.Participation_Populations) == true) == false)
                { model.Participation_Populations = pendingChange.Participation_Populations; model.Pending_Fields.Add("Participation_Populations"); }
            }

            if (model.Support_Cohorts != pendingChange.Support_Cohorts) { model.Support_Cohorts = pendingChange.Support_Cohorts; model.Pending_Fields.Add("Support_Cohorts"); }

            if (model.Enrollment_Dates != pendingChange.Enrollment_Dates)
            {
                if ((String.IsNullOrEmpty(model.Enrollment_Dates) == true && String.IsNullOrEmpty(pendingChange.Enrollment_Dates) == true) == false)
                { model.Enrollment_Dates = pendingChange.Enrollment_Dates; model.Pending_Fields.Add("Enrollment_Dates"); }
            }

            if (model.Mous != pendingChange.Mous) { model.Mous = pendingChange.Mous; model.Pending_Fields.Add("Mous"); }

            if (model.Num_Locations != pendingChange.Num_Locations) { model.Num_Locations = pendingChange.Num_Locations; model.Pending_Fields.Add("Num_Locations"); }

            if (model.Installation != pendingChange.Installation)
            {
                if ((String.IsNullOrEmpty(model.Installation) == true && String.IsNullOrEmpty(pendingChange.Installation) == true) == false)
                { model.Installation = pendingChange.Installation; model.Pending_Fields.Add("Installation"); }
            }

            if (model.City != pendingChange.City)
            {
                if ((String.IsNullOrEmpty(model.City) == true && String.IsNullOrEmpty(pendingChange.City) == true) == false)
                { model.City = pendingChange.City; model.Pending_Fields.Add("City"); }
            }

            if (model.State != pendingChange.State)
            {
                if ((String.IsNullOrEmpty(model.State) == true && String.IsNullOrEmpty(pendingChange.State) == true) == false)
                { model.State = pendingChange.State; model.Pending_Fields.Add("State"); }
            }

            if (model.Zip != pendingChange.Zip)
            {
                if ((String.IsNullOrEmpty(model.Zip) == true && String.IsNullOrEmpty(pendingChange.Zip) == true) == false)
                { model.Zip = pendingChange.Zip; model.Pending_Fields.Add("Zip"); }
            }

            if (model.Lat != pendingChange.Lat) { model.Lat = pendingChange.Lat; model.Pending_Fields.Add("Lat"); }

            if (model.Long != pendingChange.Long) { model.Long = pendingChange.Long; model.Pending_Fields.Add("Long"); }

            if (model.Nationwide != pendingChange.Nationwide) { model.Nationwide = pendingChange.Nationwide; model.Pending_Fields.Add("Nationwide"); }

            if (model.Online != pendingChange.Online) { model.Online = pendingChange.Online; model.Pending_Fields.Add("Online"); }

            if (model.Summary_Description != pendingChange.Summary_Description)
            {
                if ((String.IsNullOrEmpty(model.Summary_Description) == true && String.IsNullOrEmpty(pendingChange.Summary_Description) == true) == false)
                { model.Summary_Description = pendingChange.Summary_Description; model.Pending_Fields.Add("Summary_Description"); }
            }

            if (model.Jobs_Description != pendingChange.Jobs_Description)
            {
                if ((String.IsNullOrEmpty(model.Jobs_Description) == true && String.IsNullOrEmpty(pendingChange.Jobs_Description) == true) == false)
                { model.Jobs_Description = pendingChange.Jobs_Description; model.Pending_Fields.Add("Jobs_Description"); }
            }

            if (model.Links_To_Prospective_Jobs != pendingChange.Links_To_Prospective_Jobs)
            {
                if ((String.IsNullOrEmpty(model.Links_To_Prospective_Jobs) == true && String.IsNullOrEmpty(pendingChange.Links_To_Prospective_Jobs) == true) == false)
                { model.Links_To_Prospective_Jobs = pendingChange.Links_To_Prospective_Jobs; model.Pending_Fields.Add("Links_To_Prospective_Jobs"); }
            }

            if (model.Locations_Of_Prospective_Jobs_By_State != pendingChange.Locations_Of_Prospective_Jobs_By_State)
            {
                if ((String.IsNullOrEmpty(model.Locations_Of_Prospective_Jobs_By_State) == true && String.IsNullOrEmpty(pendingChange.Locations_Of_Prospective_Jobs_By_State) == true) == false)
                { model.Locations_Of_Prospective_Jobs_By_State = pendingChange.Locations_Of_Prospective_Jobs_By_State; model.Pending_Fields.Add("Locations_Of_Prospective_Jobs_By_State"); }
            }

            if (model.Salary != pendingChange.Salary)
            {
                if ((String.IsNullOrEmpty(model.Salary) == true && String.IsNullOrEmpty(pendingChange.Salary) == true) == false)
                { model.Salary = pendingChange.Salary; model.Pending_Fields.Add("Salary"); }
            }

            if (model.Prospective_Job_Labor_Demand != pendingChange.Prospective_Job_Labor_Demand)
            {
                if ((String.IsNullOrEmpty(model.Prospective_Job_Labor_Demand) == true && String.IsNullOrEmpty(pendingChange.Prospective_Job_Labor_Demand) == true) == false)
                { model.Prospective_Job_Labor_Demand = pendingChange.Prospective_Job_Labor_Demand; model.Pending_Fields.Add("Prospective_Job_Labor_Demand"); }
            }

            if (model.Target_Mocs != pendingChange.Target_Mocs)
            {
                if ((String.IsNullOrEmpty(model.Target_Mocs) == true && String.IsNullOrEmpty(pendingChange.Target_Mocs) == true) == false)
                { model.Target_Mocs = pendingChange.Target_Mocs; model.Pending_Fields.Add("Target_Mocs"); }
            }

            if (model.Other_Eligibility_Factors != pendingChange.Other_Eligibility_Factors)
            {
                if ((String.IsNullOrEmpty(model.Other_Eligibility_Factors) == true && String.IsNullOrEmpty(pendingChange.Other_Eligibility_Factors) == true) == false)
                { model.Other_Eligibility_Factors = pendingChange.Other_Eligibility_Factors; model.Pending_Fields.Add("Other_Eligibility_Factors"); }
            }

            if (model.Cost != pendingChange.Cost)
            {
                if ((String.IsNullOrEmpty(model.Cost) == true && String.IsNullOrEmpty(pendingChange.Cost) == true) == false)
                { model.Cost = pendingChange.Cost; model.Pending_Fields.Add("Cost"); }
            }

            if (model.Other != pendingChange.Other)
            {
                if ((String.IsNullOrEmpty(model.Other) == true && String.IsNullOrEmpty(pendingChange.Other) == true) == false)
                { model.Other = pendingChange.Other; model.Pending_Fields.Add("Other"); }
            }

            if (model.Notes != pendingChange.Notes)
            {
                if ((String.IsNullOrEmpty(model.Notes) == true && String.IsNullOrEmpty(pendingChange.Notes) == true) == false)
                { model.Notes = pendingChange.Notes; model.Pending_Fields.Add("Notes"); }
            }

            if (model.Created_By != pendingChange.Created_By)
            {
                if ((String.IsNullOrEmpty(model.Created_By) == true && String.IsNullOrEmpty(pendingChange.Created_By) == true) == false)
                { model.Created_By = pendingChange.Created_By; model.Pending_Fields.Add("Created_By"); }
            }

            if (model.Updated_By != pendingChange.Updated_By)
            {
                if ((String.IsNullOrEmpty(model.Updated_By) == true && String.IsNullOrEmpty(pendingChange.Updated_By) == true) == false)
                { model.Updated_By = pendingChange.Updated_By; model.Pending_Fields.Add("Updated_By"); }
            }

            if (model.For_Spouses != pendingChange.For_Spouses) { model.For_Spouses = pendingChange.For_Spouses; model.Pending_Fields.Add("For_Spouses"); }

            if (model.Legacy_Opportunity_Id != pendingChange.Legacy_Opportunity_Id) { model.Legacy_Opportunity_Id = pendingChange.Legacy_Opportunity_Id; model.Pending_Fields.Add("Legacy_Opportunity_Id"); }

            if (model.Legacy_Program_Id != pendingChange.Legacy_Program_Id) { model.Legacy_Program_Id = pendingChange.Legacy_Program_Id; model.Pending_Fields.Add("Legacy_Program_Id"); }

            if (model.Legacy_Provider_Id != pendingChange.Legacy_Provider_Id) { model.Legacy_Provider_Id = pendingChange.Legacy_Provider_Id; model.Pending_Fields.Add("Legacy_Provider_Id"); }

            return model;
        }

        [Authorize(Roles = "Admin, Analyst")]
        // Post the change to the pending opportunities change database table, ready for an analyst to review and approve
        [HttpPost]
        public async Task<IActionResult> EditOpportunity(EditOpportunityModel model)
        {
            if (CanPostEdit(model.Id) == false)
            {
                ViewBag.ErrorMessage = $"Opportunity with id = {model.Id} cannot be edited";
                return View("NotFound");
            }

            // Find any pending changes for this organization
            var pendingChange = _db.PendingOpportunityChanges.FirstOrDefault(e => e.Opportunity_Id == int.Parse(model.Id) && e.Pending_Change_Status == 0);

            OpportunityModel origOpp = _db.Opportunities.FirstOrDefault(e => e.Id == int.Parse(model.Id));

            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == origOpp.Program_Id);

            var opp = new PendingOpportunityChangeModel { };

            string userName = HttpContext.User.Identity.Name;

            bool sendOSDEmailNotification = false;

            if (model == null)
            {
                ViewBag.ErrorMessage = $"Opportunity with id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                // If theres already a unresolved pending change, update it
                if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                {
                    pendingChange.Group_Id = model.Group_Id;
                    pendingChange.Is_Active = model.Is_Active;
                    pendingChange.Program_Name = GlobalFunctions.RemoveSpecialCharacters(model.Program_Name);
                    pendingChange.Opportunity_Url = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Url);
                    pendingChange.Date_Program_Initiated = model.Date_Program_Initiated;
                    pendingChange.Date_Created = model.Date_Created; // Date opportunity was created in system
                    pendingChange.Date_Updated = DateTime.Now; // Date opportunity was last edited/updated in the system
                    pendingChange.Employer_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(model.Employer_Poc_Name);
                    pendingChange.Employer_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Employer_Poc_Email);
                    pendingChange.Training_Duration = GlobalFunctions.RemoveSpecialCharacters(model.Training_Duration);
                    pendingChange.Service = GlobalFunctions.RemoveSpecialCharacters(model.Service);
                    pendingChange.Delivery_Method = model.Delivery_Method;
                    pendingChange.Multiple_Locations = model.Multiple_Locations;
                    pendingChange.Program_Type = GlobalFunctions.RemoveSpecialCharacters(model.Program_Type);
                    pendingChange.Job_Families = GlobalFunctions.RemoveSpecialCharacters(model.Job_Families);
                    pendingChange.Participation_Populations = GlobalFunctions.RemoveSpecialCharacters(model.Participation_Populations);
                    pendingChange.Support_Cohorts = model.Support_Cohorts;
                    pendingChange.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(model.Enrollment_Dates);
                    pendingChange.Mous = model.Mous;
                    pendingChange.Num_Locations = model.Num_Locations;
                    pendingChange.Installation = GlobalFunctions.RemoveSpecialCharacters(model.Installation);
                    pendingChange.City = GlobalFunctions.RemoveSpecialCharacters(model.City);
                    pendingChange.State = GlobalFunctions.RemoveSpecialCharacters(model.State);
                    pendingChange.Zip = GlobalFunctions.RemoveSpecialCharacters(model.Zip);
                    pendingChange.Lat = model.Lat;
                    pendingChange.Long = model.Long;
                    pendingChange.Nationwide = prog.Nationwide;
                    pendingChange.Online = prog.Online;
                    pendingChange.Summary_Description = GlobalFunctions.RemoveSpecialCharacters(model.Summary_Description);
                    pendingChange.Jobs_Description = GlobalFunctions.RemoveSpecialCharacters(model.Jobs_Description);
                    pendingChange.Links_To_Prospective_Jobs = GlobalFunctions.RemoveSpecialCharacters(model.Links_To_Prospective_Jobs);
                    pendingChange.Locations_Of_Prospective_Jobs_By_State = GlobalFunctions.RemoveSpecialCharacters(model.Locations_Of_Prospective_Jobs_By_State);
                    pendingChange.Salary = GlobalFunctions.RemoveSpecialCharacters(model.Salary);
                    pendingChange.Prospective_Job_Labor_Demand = GlobalFunctions.RemoveSpecialCharacters(model.Prospective_Job_Labor_Demand);
                    pendingChange.Target_Mocs = GlobalFunctions.RemoveSpecialCharacters(model.Target_Mocs);
                    pendingChange.Other_Eligibility_Factors = GlobalFunctions.RemoveSpecialCharacters(model.Other_Eligibility_Factors);
                    pendingChange.Cost = GlobalFunctions.RemoveSpecialCharacters(model.Cost);
                    pendingChange.Other = GlobalFunctions.RemoveSpecialCharacters(model.Other);
                    pendingChange.Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes);
                    pendingChange.Created_By = model.Created_By;
                    pendingChange.Updated_By = userName;
                    pendingChange.For_Spouses = model.For_Spouses;
                    pendingChange.Legacy_Opportunity_Id = model.Legacy_Opportunity_Id;
                    pendingChange.Legacy_Program_Id = model.Legacy_Program_Id;
                    pendingChange.Legacy_Provider_Id = model.Legacy_Provider_Id;
                    //pendingChange.Pending_Change_Status = 1;
                    pendingChange.Pending_Change_Status = 0;
                    pendingChange.Requires_OSD_Review = CheckForOSDApprovalNecessary(model, origOpp);

                    sendOSDEmailNotification = pendingChange.Requires_OSD_Review;

                    // Status is still pending, so no need to update
                }
                else  // If not, create a new one
                {
                    // Create the pending change object to push to the database table
                    //TODO: Convert to Mapping
                    opp = new PendingOpportunityChangeModel
                    {
                        Group_Id = model.Group_Id,
                        Organization_Id = prog.Organization_Id,
                        Program_Id = prog.Id,
                        Opportunity_Id = int.Parse(model.Id),
                        Is_Active = model.Is_Active,
                        Program_Name = GlobalFunctions.RemoveSpecialCharacters(model.Program_Name),
                        Opportunity_Url = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Url),
                        Date_Program_Initiated = model.Date_Program_Initiated,
                        Date_Created = DateTime.Now, // Date opportunity was created in system
                        Date_Updated = DateTime.Now, // Date opportunity was last edited/updated in the system
                        Employer_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(model.Employer_Poc_Name),
                        Employer_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Employer_Poc_Email),
                        Training_Duration = GlobalFunctions.RemoveSpecialCharacters(model.Training_Duration),
                        Service = GlobalFunctions.RemoveSpecialCharacters(model.Service),
                        Delivery_Method = GlobalFunctions.RemoveSpecialCharacters(model.Delivery_Method),
                        Multiple_Locations = model.Multiple_Locations,
                        Program_Type = GlobalFunctions.RemoveSpecialCharacters(model.Program_Type),
                        Job_Families = GlobalFunctions.RemoveSpecialCharacters(model.Job_Families),
                        Participation_Populations = GlobalFunctions.RemoveSpecialCharacters(model.Participation_Populations),
                        Support_Cohorts = model.Support_Cohorts,
                        Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(model.Enrollment_Dates),
                        Mous = model.Mous,
                        Num_Locations = model.Num_Locations,
                        Installation = GlobalFunctions.RemoveSpecialCharacters(model.Installation),
                        City = GlobalFunctions.RemoveSpecialCharacters(model.City),
                        State = GlobalFunctions.RemoveSpecialCharacters(model.State),
                        Zip = GlobalFunctions.RemoveSpecialCharacters(model.Zip),
                        Lat = model.Lat,
                        Long = model.Long,
                        Nationwide = prog.Nationwide,
                        Online = prog.Online,
                        Summary_Description = GlobalFunctions.RemoveSpecialCharacters(model.Summary_Description),
                        Jobs_Description = GlobalFunctions.RemoveSpecialCharacters(model.Jobs_Description),
                        Links_To_Prospective_Jobs = GlobalFunctions.RemoveSpecialCharacters(model.Links_To_Prospective_Jobs),
                        Locations_Of_Prospective_Jobs_By_State = GlobalFunctions.RemoveSpecialCharacters(model.Locations_Of_Prospective_Jobs_By_State),
                        Salary = GlobalFunctions.RemoveSpecialCharacters(model.Salary),
                        Prospective_Job_Labor_Demand = GlobalFunctions.RemoveSpecialCharacters(model.Prospective_Job_Labor_Demand),
                        Target_Mocs = GlobalFunctions.RemoveSpecialCharacters(model.Target_Mocs),
                        Other_Eligibility_Factors = GlobalFunctions.RemoveSpecialCharacters(model.Other_Eligibility_Factors),
                        Cost = GlobalFunctions.RemoveSpecialCharacters(model.Cost),
                        Other = GlobalFunctions.RemoveSpecialCharacters(model.Other),
                        Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes),
                        Created_By = GlobalFunctions.RemoveSpecialCharacters(model.Created_By),
                        Updated_By = userName,
                        For_Spouses = model.For_Spouses,
                        Legacy_Opportunity_Id = model.Legacy_Opportunity_Id,
                        Legacy_Program_Id = model.Legacy_Program_Id,
                        Legacy_Provider_Id = model.Legacy_Provider_Id,
                        //Pending_Change_Status = 1   // 0 = Pending
                        Pending_Change_Status = 0,
                        Requires_OSD_Review = CheckForOSDApprovalNecessary(model, origOpp)
                };

                    sendOSDEmailNotification = opp.Requires_OSD_Review;

                    _db.PendingOpportunityChanges.Add(opp);
                }

                var result = await _db.SaveChangesAsync();

                if (result >= 1)    // RESULT IS ACTUALLY THE NUMBER OF RECORDS UPDATED -- MAY NEED TO CHANGE THIS EVERYWHERE
                {
                    int pendingId = pendingChange != null ? pendingChange.Id : opp.Id;

                    if (sendOSDEmailNotification)
                    {
                        string currentHost = $"{Request.Host}";
                        string notificationEmail = _configuration["OsdNotificationEmail"];
                        string currentURI = $"{Request.Scheme}://{Request.Host}";
                        // Send confirmation Email
                        await _emailSender.SendEmailAsync(notificationEmail, "Opportunity Change Requires OSD Approval", "A recent opportunity change requires OSD approval.<br/><a href='" + currentURI + "/Analyst/ReviewPendingOpportunityChange/" + pendingId + "?oppId=" + origOpp.Id + "'>Click here to review it</a>");
                    }

                    // Find this specific pending change
                    //SB_PendingOpportunityChange pendingChange = _db.PendingOpportunityChanges.FirstOrDefault(e => e.Opportunity_Id == int.Parse(oppId) && e.Pending_Change_Status == 0);
                    /*#region Auto Approve
                    // Update the organization with the data from the reviewed change
                    if (origOpp != null)
                    {
                        origOpp.Group_Id = model.Group_Id;
                        origOpp.Program_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Name));
                        origOpp.Is_Active = model.Is_Active;
                        origOpp.Opportunity_Url = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Opportunity_Url));
                        origOpp.Date_Program_Initiated = model.Date_Program_Initiated;
                        origOpp.Date_Created = model.Date_Created; // Date opportunity was created in system
                        origOpp.Date_Updated = DateTime.Now; // Date opportunity was last edited/updated in the system
                        origOpp.Employer_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Employer_Poc_Name));
                        origOpp.Employer_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Employer_Poc_Email));
                        origOpp.Training_Duration = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Training_Duration));
                        origOpp.Service = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Service));
                        origOpp.Delivery_Method = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Delivery_Method));
                        origOpp.Multiple_Locations = model.Multiple_Locations;
                        origOpp.Program_Type = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Type));
                        origOpp.Job_Families = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Job_Families));
                        origOpp.Participation_Populations = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Participation_Populations));
                        origOpp.Support_Cohorts = model.Support_Cohorts;
                        origOpp.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Enrollment_Dates));
                        origOpp.Mous = model.Mous;
                        origOpp.Num_Locations = model.Num_Locations;
                        origOpp.Installation = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Installation));
                        origOpp.City = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.City));
                        origOpp.State = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.State));
                        origOpp.Zip = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Zip));
                        origOpp.Lat = model.Lat;
                        origOpp.Long = model.Long;
                        origOpp.Nationwide = prog.Nationwide;
                        origOpp.Online = prog.Online;
                        origOpp.Summary_Description = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Summary_Description));
                        origOpp.Jobs_Description = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Jobs_Description));
                        origOpp.Links_To_Prospective_Jobs = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Links_To_Prospective_Jobs));
                        origOpp.Locations_Of_Prospective_Jobs_By_State = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Locations_Of_Prospective_Jobs_By_State));
                        origOpp.Salary = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Salary));
                        origOpp.Prospective_Job_Labor_Demand = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Prospective_Job_Labor_Demand));
                        origOpp.Target_Mocs = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Target_Mocs));
                        origOpp.Other_Eligibility_Factors = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Other_Eligibility_Factors));
                        origOpp.Cost = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Cost));
                        origOpp.Other = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Other));
                        origOpp.Notes = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Notes));
                        origOpp.Created_By = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Created_By));
                        origOpp.Updated_By = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Updated_By));
                        origOpp.For_Spouses = model.For_Spouses;
                        origOpp.Legacy_Opportunity_Id = model.Legacy_Opportunity_Id;
                        origOpp.Legacy_Program_Id = model.Legacy_Program_Id;
                        origOpp.Legacy_Provider_Id = model.Legacy_Provider_Id;
                    }

                    _db.Opportunities.Update(origOpp);

                    var result1 = await _db.SaveChangesAsync();

                    if (result1 > 0)
                    {
                        // Update pending change
                        //pendingChange.Pending_Change_Status = 1;
                        //_db.PendingOpportunityChanges.Update(pendingChange);

                        //var result2 = await _db.SaveChangesAsync();

                        //if (result2 > 0)
                        //{
                        List<OpportunityModel> opps = _db.Opportunities.Where(e => e.Program_Id == origOpp.Program_Id).ToList();
                        OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == origOpp.Organization_Id);

                        GlobalFunctions.UpdateStatesOfProgramDelivery(prog, opps, _db);

                        GlobalFunctions.UpdateOrgStatesOfProgramDelivery(org, _db);

                        //COMMENTED FOR STATES_OF_PROGRAM_DELIVERY CHANGE BEFORE DUAL ENTRY
                         
                         //UpdateStatesOfProgramDelivery(prog, opps);

                        //UpdateOrgStatesOfProgramDelivery(org);

                        return RedirectToAction("ListOpportunities");
                        //}
                    }
                    else
                    {

                    }

                    return RedirectToAction("ListOpportunities");
                    #endregion*/
                }
                else
                {

                }

                return RedirectToAction("ListOpportunities");
            }
        }

        private bool CheckForOSDApprovalNecessary(EditOpportunityModel model, OpportunityModel origOpp)
        {
            bool required = false;

            // Check if any of the fields that require OSD approval are changed
            if ((model.Training_Duration == null ? "" : model.Training_Duration) != origOpp.Training_Duration ||
                (model.Program_Type == null ? "" : model.Program_Type) != origOpp.Program_Type ||
                model.Num_Locations != origOpp.Num_Locations ||
                model.Summary_Description != origOpp.Summary_Description ||
                model.Jobs_Description != origOpp.Jobs_Description ||
                (model.Links_To_Prospective_Jobs == null ? "" : model.Links_To_Prospective_Jobs) != origOpp.Links_To_Prospective_Jobs ||//
                (model.Locations_Of_Prospective_Jobs_By_State == null ? "" : model.Locations_Of_Prospective_Jobs_By_State) != origOpp.Locations_Of_Prospective_Jobs_By_State ||//
                (model.Salary == null ? "" : model.Salary) != origOpp.Salary ||//
                (model.Installation == null ? "" : model.Installation) != origOpp.Installation ||//
                (model.City == null ? "" : model.City) != origOpp.City ||//
                model.State != origOpp.State ||
                (model.Zip == null ? "" : model.Zip) != origOpp.Zip ||//
                model.Long != origOpp.Long ||
                model.Lat != origOpp.Lat ||
                (model.Cost == null ? "" : model.Cost) != origOpp.Cost)//
            {
                required = true;
            }

            return required;
        }

    public string PreventNullString(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                s = "";
            }
            return s;
        }


        [HttpPost]
        public async Task<IActionResult> DeleteOpportunity(string id)
        {
            var opp = _db.PendingOpportunityChanges.FirstOrDefault(e => e.Id == int.Parse(id));

            if (opp == null)
            {
                ViewBag.ErrorMessage = $"Opportunity with id = { id } cannot be found";
                return View("NotFound");
            }
            else
            {
                _db.Remove(opp);
                var result = await _db.SaveChangesAsync();

                if (result == 1)
                {
                    return RedirectToAction("ListOpportunities");
                }
                else
                {

                }

                /*foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }*/

                return View("ListOpportunities");
            }
        }

        public bool CanPostEdit(string oppId)
        {
            OpportunityModel opp = _db.Opportunities.FirstOrDefault(e => e.Id == int.Parse(oppId));

            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == opp.Program_Id);

            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == opp.Organization_Id);

            if (org.Is_Active && prog.Is_Active)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet]
        public IActionResult DownloadCSV()
        {
            var opps = _db.Opportunities;

            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                /*Type objType = typeof(OpportunityModel);
                FieldInfo[] info = objType.GetFields(BindingFlags.Public | BindingFlags.Static);

                
                for(int i=0; i < info.Length; i++)
                {
                    // If not last row, add comma
                    string comma = "";
                    if(i != info.Length - 1)
                    {
                        comma = ",";
                    }

                    stringBuilder.AppendLine(info[i].ToString() + comma);
                }*/

                stringBuilder.AppendLine("Id,Group_Id,Program_Name,Opportunity_Url,Date_Program_Initiated,Date_Created,Date_Updated,Employer_Poc_Name,Employer_Poc_Email,Training_Duration,Service,Delivery_Method,Multiple_Locations,Program_Type,Job_Families,Participation_Populations,Support_Cohorts,Enrollment_Dates,Mous,Num_Locations,Installation,City,State,Zip,Lat,Long,Nationwide,Online,Summary_Description,Jobs_Description,Links_To_Prospective_Jobs,Locations_Of_Prospective_Jobs_By_State,Salary,Prospective_Job_Labor_Demand,Target_Mocs,Other_Eligibility_Factors,Cost,Other,Notes,Created_By,Updated_By,For_Spouses,Legacy_Opportunity_Id,Legacy_Program_Id,Legacy_Provider_Id");

                

                foreach (OpportunityModel opp in opps)
                {
                    //var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);
                    //string urlToDisplay = org != null ? org.Organization_Url : "";

                    string programName = EscCommas(opp.Program_Name.Replace(System.Environment.NewLine, ""));
                    string Employer_Poc_Name = EscCommas(opp.Employer_Poc_Name.Replace(System.Environment.NewLine, "")); //add a line terminating ;
                    string summaryDescription = EscCommas(opp.Summary_Description.Replace(System.Environment.NewLine, "")); //add a line terminating ;
                    string jobDescription = EscCommas(opp.Jobs_Description.Replace(System.Environment.NewLine, "")); //add a line terminating ;
                    string service = EscCommas(opp.Service.Replace(System.Environment.NewLine, ""));
                    string pp = EscCommas(opp.Participation_Populations.Replace(System.Environment.NewLine, ""));
                    string states = opp.Locations_Of_Prospective_Jobs_By_State != null ? EscCommas(opp.Locations_Of_Prospective_Jobs_By_State.Replace(System.Environment.NewLine, "")) : "";
                    string other = opp.Other != null ? EscCommas(opp.Other.Replace(System.Environment.NewLine, "")) : "";
                    string otherElig = opp.Other_Eligibility_Factors != null ? EscCommas(opp.Other_Eligibility_Factors.Replace(System.Environment.NewLine, "")) : "";
                    string targetMOCs = opp.Target_Mocs != null ? EscCommas(opp.Target_Mocs.Replace(System.Environment.NewLine, "")) : "";
                    string jobFamilies = EscCommas(opp.Job_Families.Replace(System.Environment.NewLine, ""));
                    string prospective = opp.Prospective_Job_Labor_Demand != null ? EscCommas(opp.Prospective_Job_Labor_Demand.Replace(System.Environment.NewLine, "")) : "";
                    string city = opp.City != null ? EscCommas(opp.City.Replace(System.Environment.NewLine, "")) : "";
                    string state = opp.State != null ? EscCommas(opp.State.Replace(System.Environment.NewLine, "")) : "";
                    string enrollmentDates = opp.Enrollment_Dates != null ? EscCommas(opp.Enrollment_Dates.Replace(System.Environment.NewLine, "")) : "";
                    string trainingDuration = opp.Training_Duration != null ? EscCommas(opp.Training_Duration.Replace(System.Environment.NewLine, "")) : "";
                    string deliveryMethod = EscCommas(opp.Delivery_Method.Replace(System.Environment.NewLine, ""));
                    string installation = opp.Installation != null ? EscCommas(opp.Installation.Replace(System.Environment.NewLine, "")) : "";
                    string linksTo = opp.Links_To_Prospective_Jobs != null ? EscCommas(opp.Links_To_Prospective_Jobs.Replace(System.Environment.NewLine, "")) : "";
                    string cost = opp.Cost != null ? EscCommas(opp.Cost.Replace(System.Environment.NewLine, "")) : "";
                    string salary = opp.Salary != null ? EscCommas(opp.Salary.Replace(System.Environment.NewLine, "")) : "";
                    string notes = opp.Notes == null ? "" : EscCommas(opp.Notes.Replace(System.Environment.NewLine, ""));
                    string programType = EscCommas(opp.Program_Type.Replace(System.Environment.NewLine, ""));
                    string url = opp.Opportunity_Url != null ? EscCommas(opp.Opportunity_Url.Replace(System.Environment.NewLine, "")) : "";
                    string Employer_Poc_Email = EscCommas(opp.Employer_Poc_Email.Replace(System.Environment.NewLine, ""));

                    stringBuilder.AppendLine($"{opp.Id},{opp.GroupId},{programName},{url},{opp.Date_Program_Initiated},{opp.Date_Created},{opp.Date_Updated},{Employer_Poc_Name},{Employer_Poc_Email},{trainingDuration},{service},{deliveryMethod},{opp.Multiple_Locations},{programType},{jobFamilies},{pp},{opp.Support_Cohorts},{enrollmentDates},{opp.Mous},{opp.Num_Locations},{installation},{city},{state},{opp.Zip},{opp.Lat},{opp.Long},{opp.Nationwide},{opp.Online},{summaryDescription},{jobDescription},{linksTo},{states},{salary},{prospective},{targetMOCs},{otherElig},{cost},{other},{notes},{opp.Created_By},{opp.Updated_By},{opp.For_Spouses},{opp.Legacy_Opportunity_Id},{opp.Legacy_Program_Id},{opp.Legacy_Provider_Id}");
                }

                //return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", "Opportunities-FULL-" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
                return ExportOpportunities();
            }
            catch
            {
                return Error();
            }
        }

        public FileStreamResult ExportOpportunities()
        {
            var result = WriteCsvToMemory(_db.Opportunities);
            var memoryStream = new MemoryStream(result);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddSeconds(2);
            options.Path = "/Opportunities";
            _httpContextAccessor.HttpContext.Response.Cookies.Append("opportunitiesDownloadStarted", "1", options);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = "Opportunities-FULL-" + DateTime.Today.ToString("MM-dd-yy") + ".csv" };
        }


        public byte[] WriteCsvToMemory(IEnumerable<OpportunityModel> records)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.Context.RegisterClassMap<OpportunityMap>();
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

            if(data != null)
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

public sealed class OpportunityMap : ClassMap<OpportunityModel>
{
    public OpportunityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Legacy_Opportunity_Id).Ignore();
        Map(m => m.Legacy_Program_Id).Ignore();
        Map(m => m.Legacy_Provider_Id).Ignore();
    }
}