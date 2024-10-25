using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SkillBridge.CMS.ViewModel;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Repository;
using Taku.Core.Global;

namespace SkillBridge.CMS.Controllers
{
    [Authorize(Roles = "Organization, Program")]

    public class MyOpportunitiesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MyOpportunitiesController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;

        public MyOpportunitiesController(ILogger<MyOpportunitiesController> logger, IConfiguration configuration, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _db = db;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View("MyOpportunities");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOpportunity()
        {
            var _authenticationRepository = new AuthenticationRepository(_db);

            var id = _userManager.GetUserId(User); // Get user id:
            var user = await _userManager.FindByIdAsync(id);

            var programList = await _authenticationRepository.GetProgramsByUser(id);

            // Get Id/Title combos for opportunity groups, showing only unique ones in the dropdown
            var list = _db.OpportunityGroups.Select(m => new GroupDropdownItem { Id = m.Group_Id, Title = m.Title, Lat = m.Lat, Long = m.Long }).OrderBy(m => m.Id).ToList();

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

            for (int i = 0; i < programList.Count(); i++)
            {
                programLookupList.Add(new ProgramDropdownItem()
                {
                    Id = programList[i].Id,
                    Program_Name = programList[i].ProgramName.Replace("'", "&apos;"),
                    Service = programList[i].ServicesSupported
                });
            }

            ViewBag.GroupIds = dropdownList;
            ViewBag.GroupLookup = JsonConvert.SerializeObject(lookupList);
            ViewBag.ProgramLookup = JsonConvert.SerializeObject(programLookupList);

            ViewBag.Programs = programList.OrderBy(o => o.ProgramName).ToList();



            //SelectList groups = new SelectList(_db.OpportunityGroups.Select(l => l.Group_Id).Distinct(), "Group_Id", "Group_Id");
            //ViewBag.GroupIds = groups;

            return View(new OpportunityModel {});
        }

        [HttpPost]
        public async Task<IActionResult> CreateOpportunity(OpportunityModel model, string newGroupTitle)
        {
            //Console.WriteLine("Create Opportunity posted");
            //Console.WriteLine("newGroupTitle: " + newGroupTitle);
            string userName = HttpContext.User.Identity.Name;

            var id = _userManager.GetUserId(User); // Get user id:
            var user = await _userManager.FindByIdAsync(id);

            //Console.WriteLine("model.Program_Id: " + model.Program_Id);

            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == model.Program_Id);
            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == prog.OrganizationId);
            //OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == prog.Organization_Id);
            //var mou = _db.Mous.FirstOrDefault(e => e.Id == org.Mou_Id);

            //Console.WriteLine("prog: " + prog.Program_Name);
            // Console.WriteLine("org: " + org.Name);
            //Console.WriteLine("ModelState.IsValid: " + ModelState.IsValid);

            string newJobFamilies, newPartPop;

            newJobFamilies = GetJobFamiliesListForProg(prog);
            newPartPop = GetParticipationPopulationStringFromProgram(prog);

            Console.WriteLine("newJobFamilies: " + newJobFamilies);
            Console.WriteLine("newPartPop: " + newPartPop);

            //model.Service = prog.Services_Supported;
            //model.Program_Name = prog.Program_Name;

            //ModelState.MarkFieldValid("Service");
            //ModelState.MarkFieldValid("Program_Name");

            if (ModelState.IsValid)
            {
                //Console.WriteLine("MODEL STATE VALID, TRYING TO CREATE OPPORTUNITY");
                try
                {
                    //TODO: Convert to mapping
                    var opp = new PendingOpportunityAdditionModel()
                    {
                        // Save to DB
                        Created_By = userName,
                        Updated_By = userName,
                        Date_Created = DateTime.Now,
                        Date_Updated = DateTime.Now,
                        Organization_Id = org.Id,
                        Opportunity_Url = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Url),
                        //Organization_Name = org.Name,
                        Date_Program_Initiated = prog.DateCreated,
                        Program_Name = prog.ProgramName,
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

                    if (prog.LegacyProgramId != 0 && prog.LegacyProgramId != -1)
                    {
                    opp.Legacy_Program_Id = prog.LegacyProgramId;
                    }
                    else
                    {
                    opp.Legacy_Program_Id = 0;
                    }

                    if (prog.LegacyProviderId != 0 && prog.LegacyProviderId != -1)
                    {
                    opp.Legacy_Provider_Id = prog.LegacyProviderId;
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

                    if (result1 > 0)
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

                        //_db.Opportunities.Update(model);
                        //_db.OpportunityGroups.Add(group);
                        //var result2 = await _db.SaveChangesAsync();

                        //if (result2 > 0)
                        //{
                        // THESE NEED TO RUN AFTER APPROVAL
                        Console.WriteLine("Result2 returned > 0");
                        //List<OpportunityModel> opps = _db.Opportunities.Where(e => e.Program_Id == opp.Program_Id).ToList();
                        //ProgramModel prog2 = _db.Programs.FirstOrDefault(e => e.Id == model.Program_Id);
                        //OrganizationModel org2 = _db.Organizations.FirstOrDefault(e => e.Id == opp.Organization_Id);



                        //UpdateStatesOfProgramDelivery(prog2, opps);

                        //UpdateOrgStatesOfProgramDelivery(org2);

                        //return RedirectToAction("ListOpportunities", "Opportunities");
                        return RedirectToAction("MyOpportunityAdditionSuccess");
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

                foreach (var error in errors)
                {
                    Console.WriteLine("error: " + error);
                }
            }

            return RedirectToAction("MyOpportunityAdditionFailed");
            //return RedirectToAction("ListOpportunities", "Opportunities");
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

                if (count == 1)
                {
                    if (fam != null)
                    {
                        jfs += fam.Name;
                    }
                }
                else if (count == 2)
                {
                    if (i == 0)
                    {
                        if (fam != null)
                        {
                            jfs += fam.Name;
                        }
                    }
                    else
                    {
                        if (fam != null)
                        {
                            jfs += " and " + fam.Name;
                        }
                    }
                }
                else if (count >= 3)
                {
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
                            jfs += ", and " + fam.Name;
                        }
                    }
                    else
                    {
                        if (fam != null)
                        {
                            jfs += ", " + fam.Name;
                        }
                    }
                }

                /*if (i == 0)
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
                }*/

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

                if (count == 1)
                {
                    if (p != null)
                    {
                        pps += p.Name;
                    }
                }
                else if (count == 2)
                {
                    if (i == 0)
                    {
                        if (p != null)
                        {
                            pps += p.Name;
                        }
                    }
                    else
                    {
                        if (p != null)
                        {
                            //services += " and " + service.Name;
                            pps += ", " + p.Name;
                        }
                    }
                }
                else if (count >= 3)
                {
                    if (i == 0)
                    {
                        if (p != null)
                        {
                            pps += p.Name;
                        }
                    }
                    /*else if(i < count - 1)
                    {
                        if (service != null)
                        {
                            pps += ", and " + service.Name;
                        }
                    }*/
                    else
                    {
                        if (p != null)
                        {
                            pps += ", " + p.Name;
                        }
                    }
                }



                /*if (i == 0)
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
                }*/

                i++;
            }

            Console.WriteLine("GetParticipationPopulationStringFromProgram returning " + pps);

            return pps;
        }



        private void UpdateOrgStatesOfProgramDelivery(OrganizationModel org)
        {
            // Get all programs from org
            List<ProgramModel> progs = _db.Programs.Where(e => e.OrganizationId == org.Id).ToList();

            List<string> states = new List<string>();


            foreach (ProgramModel p in progs)
            {
                string progStates = "";
                progStates = p.StatesOfProgramDelivery;
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

            prog.StatesOfProgramDelivery = newStateList;

            _db.SaveChanges();
        }


        [HttpGet]
        public async Task<IActionResult> MyOpportunities()
        {
            var _authenticationRepository = new AuthenticationRepository(_db);

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            bool IsAdmin = currentUser.IsInRole("Admin");
            var id = _userManager.GetUserId(User); // Get user id:
            var user = await _userManager.FindByIdAsync(id);

            List<OpportunityModel> filteredOpps = await _authenticationRepository.GetOpportunitiesByUser(id);

            List<ListOpportunityModel> model = new List<ListOpportunityModel>();

            foreach (OpportunityModel opp in filteredOpps)
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

            /*Organization_Name
            Mou_Link
            Mou_Expiration_Date
            POC_First_Name
            POC_Last_Name
            POC_Email*/
            return View(model);
        }

        // Pull the data from the existing Opportunity record
        [HttpGet]
        public async Task<IActionResult> EditOpportunity(string id, bool edit)
        {
            Console.WriteLine("EditOpportunity GET called");
            Console.WriteLine("id: " + id);
            // Find the existing Opportunity in the current database
            OpportunityModel opp = await _db.Opportunities.FirstOrDefaultAsync(e => e.Id == int.Parse(id));

            // Find any pending changes for this organization
            var pendingChange = await _db.PendingOpportunityChanges.FirstOrDefaultAsync(e => e.Opportunity_Id == int.Parse(id) && e.Pending_Change_Status == 0);

            // Populate Dropdown info for States Dropdown
            List<State> states = new List<State>();

            foreach (State s in await _db.States.ToListAsync())
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

            if (org.Is_Active == false || prog.IsActive == false)
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

            return View(model);
        }

        public void CheckStringForChanges(string s1, string s2, EditOpportunityModel model, string fieldName)
        {
            Console.WriteLine("checking strings for changes: " + s1 + " =AND= " + s2);
            // If the strings are different
            if (!s1.Equals(s2))
            {
                // If the string it will be changed to isnt null and it isnt blank
                if (!String.IsNullOrEmpty(s2))
                {
                    if ((String.IsNullOrEmpty(s1) == true && String.IsNullOrEmpty(s2) == true) == false)
                    { 
                        s1 = s2; 
                        model.Pending_Fields.Add(fieldName); 
                    }
                }
            }
        }

        public EditOpportunityModel UpdateOppModelWithPendingChanges(EditOpportunityModel model, PendingOpportunityChangeModel pendingChange)
        {
            if (model.Group_Id != pendingChange.Group_Id) { model.Group_Id = pendingChange.Group_Id; model.Pending_Fields.Add("Group_Id"); }

            /*if (model.Program_Name != pendingChange.Program_Name)
            {
                if(pendingChange.Program_Name != null && pendingChange.Program_Name != "")
                {
                    if ((String.IsNullOrEmpty(model.Program_Name) == true && String.IsNullOrEmpty(pendingChange.Program_Name) == true) == false)
                    { model.Program_Name = pendingChange.Program_Name; model.Pending_Fields.Add("Program_Name"); }
                }
            }*/

            if (model.Is_Active != pendingChange.Is_Active) { model.Is_Active = pendingChange.Is_Active; model.Pending_Fields.Add("Is_Active"); }

            if (model.Opportunity_Url != pendingChange.Opportunity_Url)
            {
                if ((String.IsNullOrEmpty(model.Opportunity_Url) == true && String.IsNullOrEmpty(pendingChange.Opportunity_Url) == true) == false)
                { model.Opportunity_Url = pendingChange.Opportunity_Url; model.Pending_Fields.Add("Opportunity_Url"); }
            }

            //CheckStringForChanges(model.Opportunity_Url, pendingChange.Opportunity_Url, model, "Opportunity_Url");

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

            /*if (model.Installation != pendingChange.Installation)
            {
                if ((String.IsNullOrEmpty(model.Installation) == true && String.IsNullOrEmpty(pendingChange.Installation) == true) == false)
                { model.Installation = pendingChange.Installation; model.Pending_Fields.Add("Installation"); }
            }*/
            CheckStringForChanges(model.Installation, pendingChange.Installation, model, "Installation");

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

        // Post the change to the pending opportunities change database table, ready for an analyst to review and approve
        [HttpPost]
        public async Task<IActionResult> EditOpportunity(EditOpportunityModel model)
        {
            //Console.WriteLine("EditOpportunity POST called");
            //Console.WriteLine("PendingFields.Count: " + model.Pending_Fields.Count);
            if (CanPostEdit(model.Id) == false)
            {
                ViewBag.ErrorMessage = $"Opportunity with id = {model.Id} cannot be edited";
                return View("NotFound");
            }

            // Find the existing Opportunity in the current database
            OpportunityModel originalOpp = _db.Opportunities.FirstOrDefault(e => e.Id == int.Parse(model.Id));
            // Find any pending changes for this organization
            var pendingChange = _db.PendingOpportunityChanges.FirstOrDefault(e => e.Opportunity_Id == int.Parse(model.Id) && e.Pending_Change_Status == 0);

            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == originalOpp.Program_Id);

            //TODO: Convert to mapping
            var opp = new PendingOpportunityChangeModel { };

            string userName = HttpContext.User.Identity.Name;

            bool sendOSDEmailNotification = false;

            if (model == null)
            {
                //Console.WriteLine("EditOpportunity POST model is NULL");
                ViewBag.ErrorMessage = $"Opportunity with id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Console.WriteLine("EditOpportunity POST model is NOT NULL");
                // If theres already a unresolved pending change, update it
                if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                {
                    //Console.WriteLine("pendingChange IS NOT NULL, pending_change_status IS 0...");
                    //Console.WriteLine("UPDATING PENDING CHANGE INFORMATION");
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
                    pendingChange.Delivery_Method = GlobalFunctions.RemoveSpecialCharacters(model.Delivery_Method);
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
                    pendingChange.Requires_OSD_Review = CheckForOSDApprovalNecessary(model, originalOpp);

                    sendOSDEmailNotification = pendingChange.Requires_OSD_Review;

                    // Status is still pending, so no need to update
                }
                else  // If not, create a new one
                {
                    //Console.WriteLine("CREATING A NEW PENDING CHANGE");
                    // Create the pending change object to push to the database tabler
                    // TODO: Convert to Mapping
                    opp = new PendingOpportunityChangeModel()
                    {
                        Group_Id = model.Group_Id,
                        Opportunity_Id = int.Parse(model.Id),
                        Is_Active = model.Is_Active,
                        Program_Name = model.Program_Name,
                        Opportunity_Url = model.Opportunity_Url,
                        Date_Program_Initiated = model.Date_Program_Initiated,
                        Date_Created = DateTime.Now, // Date opportunity was created in system
                        Date_Updated = DateTime.Now, // Date opportunity was last edited/updated in the system
                        Employer_Poc_Name = model.Employer_Poc_Name,
                        Employer_Poc_Email = model.Employer_Poc_Email,
                        Training_Duration = model.Training_Duration,
                        Service = model.Service,
                        Delivery_Method = model.Delivery_Method,
                        Multiple_Locations = model.Multiple_Locations,
                        Program_Type = model.Program_Type,
                        Job_Families = model.Job_Families,
                        Participation_Populations = model.Participation_Populations,
                        Support_Cohorts = model.Support_Cohorts,
                        Enrollment_Dates = model.Enrollment_Dates,
                        Mous = model.Mous,
                        Num_Locations = model.Num_Locations,
                        Installation = model.Installation,
                        City = model.City,
                        State = model.State,
                        Zip = model.Zip,
                        Lat = model.Lat,
                        Long = model.Long,
                        Nationwide = prog.Nationwide,
                        Online = prog.Online,
                        Summary_Description = model.Summary_Description,
                        Jobs_Description = model.Jobs_Description,
                        Links_To_Prospective_Jobs = model.Links_To_Prospective_Jobs,
                        Locations_Of_Prospective_Jobs_By_State = model.Locations_Of_Prospective_Jobs_By_State,
                        Salary = model.Salary,
                        Prospective_Job_Labor_Demand = model.Prospective_Job_Labor_Demand,
                        Target_Mocs = model.Target_Mocs,
                        Other_Eligibility_Factors = model.Other_Eligibility_Factors,
                        Cost = model.Cost,
                        Other = model.Other,
                        Notes = model.Notes,
                        Created_By = model.Created_By,
                        Updated_By = userName,
                        For_Spouses = model.For_Spouses,
                        Legacy_Opportunity_Id = model.Legacy_Opportunity_Id,
                        Legacy_Program_Id = model.Legacy_Program_Id,
                        Legacy_Provider_Id = model.Legacy_Provider_Id,
                        Pending_Change_Status = 0,   // 0 = Pending
                        Requires_OSD_Review = CheckForOSDApprovalNecessary(model, originalOpp)
                };

                    sendOSDEmailNotification = opp.Requires_OSD_Review;

                    _db.PendingOpportunityChanges.Add(opp);
                }
                //Console.WriteLine("PendingFields.Count: " + model.Pending_Fields.Count);
                //Console.WriteLine("TRYING TO SAVE THE UPDATED PENDING CHANGE OR THE NEW CHANGE");
                var result = await _db.SaveChangesAsync();

                // RESET THE MODEL TO THE ORIGINAL VIEW MODEL SO WE CAN COMPARE OLD TO NEW VALS APPROPRIATELY
                model = new EditOpportunityModel
                {
                    Id = originalOpp.Id.ToString(),
                    Group_Id = originalOpp.GroupId,
                    Is_Active = originalOpp.Is_Active,
                    Program_Name = originalOpp.Program_Name,
                    Opportunity_Url = originalOpp.Opportunity_Url,
                    Date_Program_Initiated = originalOpp.Date_Program_Initiated,
                    Date_Created = originalOpp.Date_Created, // Date opportunity was created in system
                    Date_Updated = originalOpp.Date_Updated, // Date opportunity was last edited/updated in the system
                    Employer_Poc_Name = originalOpp.Employer_Poc_Name,
                    Employer_Poc_Email = originalOpp.Employer_Poc_Email,
                    Training_Duration = originalOpp.Training_Duration,
                    Service = originalOpp.Service,
                    Delivery_Method = originalOpp.Delivery_Method,
                    Multiple_Locations = originalOpp.Multiple_Locations,
                    Program_Type = originalOpp.Program_Type,
                    Job_Families = originalOpp.Job_Families,
                    Participation_Populations = originalOpp.Participation_Populations,
                    Support_Cohorts = originalOpp.Support_Cohorts,
                    Enrollment_Dates = originalOpp.Enrollment_Dates,
                    Mous = originalOpp.Mous,
                    Num_Locations = originalOpp.Num_Locations,
                    Installation = originalOpp.Installation,
                    City = originalOpp.City,
                    State = originalOpp.State,
                    Zip = originalOpp.Zip,
                    Lat = originalOpp.Lat,
                    Long = originalOpp.Long,
                    Nationwide = prog.Nationwide,
                    Online = prog.Online,
                    Summary_Description = originalOpp.Summary_Description,
                    Jobs_Description = originalOpp.Jobs_Description,
                    Links_To_Prospective_Jobs = originalOpp.Links_To_Prospective_Jobs,
                    Locations_Of_Prospective_Jobs_By_State = originalOpp.Locations_Of_Prospective_Jobs_By_State,
                    Salary = originalOpp.Salary,
                    Prospective_Job_Labor_Demand = originalOpp.Prospective_Job_Labor_Demand,
                    Target_Mocs = originalOpp.Target_Mocs,
                    Other_Eligibility_Factors = originalOpp.Other_Eligibility_Factors,
                    Cost = originalOpp.Cost,
                    Other = originalOpp.Other,
                    Notes = originalOpp.Notes,
                    Created_By = originalOpp.Created_By,
                    Updated_By = originalOpp.Updated_By,
                    For_Spouses = originalOpp.For_Spouses,
                    Legacy_Opportunity_Id = originalOpp.Legacy_Opportunity_Id,
                    Legacy_Program_Id = originalOpp.Legacy_Program_Id,
                    Legacy_Provider_Id = originalOpp.Legacy_Provider_Id,
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

                Console.WriteLine("PendingFields.Count: " + model.Pending_Fields.Count);

                if (result >= 1)    // RESULT IS ACTUALLY THE NUMBER OF RECORDS UPDATED -- MAY NEED TO CHANGE THIS EVERYWHERE
                {
                    int pendingId = pendingChange != null ? pendingChange.Id : opp.Id;

                    if (sendOSDEmailNotification)
                    {
                        string currentHost = $"{Request.Host}";
                        string notificationEmail = _configuration["OsdNotificationEmail"];
                        string currentURI = $"{Request.Scheme}://{Request.Host}";
                        // Send confirmation Email
                        await _emailSender.SendEmailAsync(notificationEmail, "Opportunity Change Requires OSD Approval", "A recent opportunity change requires OSD approval.<br/><a href='" + currentURI + "/Analyst/ReviewPendingOpportunityChange/" + pendingId + "?oppId=" + originalOpp.Id + "'>Click here to review it</a>");
                    }
                }
                else
                {
                    //Console.WriteLine("SAVE RESULT WAS 0");
                }

                //return RedirectToAction("MyOpportunities");
                return RedirectToAction("MyOpportunityUpdateSuccess");
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

        public bool CanPostEdit(string oppId)
        {
            OpportunityModel opp = _db.Opportunities.FirstOrDefault(e => e.Id == int.Parse(oppId));

            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == opp.Program_Id);

            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == opp.Organization_Id);

            if (org.Is_Active && prog.IsActive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Update success/fail views
        public IActionResult MyOpportunityUpdateSuccess()
        {
            return View();
        }

        public IActionResult MyOpportunityAdditionSuccess()
        {
            return View();
        }

        public IActionResult MyOpportunityAdditionFailed()
        {
            return View();
        }

        



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
    }
}
