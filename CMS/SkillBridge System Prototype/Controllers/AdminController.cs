using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.IO;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Text;
using SkillBridge_System_Prototype.ViewModel;
using Skillbridge.Business.Command;
using Z.EntityFramework.Plus;
using Skillbridge.Business.Data;
using Skillbridge.Business.Model.Db;
using Skillbridge.Business.Util.Ingest;
using Taku.Core.Global;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AdminController(ILogger<AdminController> logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db, IEmailSender emailSender)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            /*if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return View("MustLoginView");
            }*/
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /* Roles */
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleModel model)
        {
            if(ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                IdentityResult result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Admin");
                }

                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult ListAuditEntries()
        {
            var entries = _db.Audits;
            return View(entries);
        }

        [HttpGet]
        public IActionResult DeleteAll()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DeleteAllData()
        {
            DeleteAllParticipationPopulations();
            DeleteAllPendingParticipationPopulations();
            DeleteAllJobFamilies();
            DeleteAllPendingJobFamilies();
            DeleteAllOpportunityGroups();
            DeleteAllOpportunities();
            DeleteAllPrograms();
            DeleteAllOrganizations();
            DeleteAllMOUs();


            return View("DeleteAll");
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if(role == null)
            {
                ViewBag.ErrorMessage = $"Role with id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach(var user in _userManager.Users)
            {
                if(await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await _roleManager.UpdateAsync(role);

                if(result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        /* Generate Data */

        [HttpGet]
        public IActionResult GenerateOrgData()  // Generates the list of JSON data that will be used for the live site organizations page
        {
            var progs = _db.Programs;

            // Generate the string of JSON
            string newJson = "var orgs = { data: [";

            int i = 0;

            try
            {
                foreach (ProgramModel prog in progs)
                {
                    newJson += "{";

                    var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);

                    string urlToDisplay = org != null ? org.Organization_Url : "";

                    newJson += "\"PROGRAM\": " + prog.Program_Name + ",";
                    newJson += "\"URL\": " + urlToDisplay + ",";
                    newJson += "\"OPPORTUNITY_TYPE\": " + prog.Opportunity_Type + ",";
                    newJson += "\"DELIVERY_METHOD\": " + prog.Delivery_Method + ",";
                    newJson += "\"PROGRAM_DURATION\": " + prog.Program_Duration + ",";
                    newJson += "\"STATES\": " + prog.States_Of_Program_Delivery + ",";
                    newJson += "\"NATIONWIDE\": " + prog.Nationwide + ",";
                    newJson += "\"ONLINE\": " + prog.Online + ",";
                    newJson += "\"COHORTS\": " + prog.Support_Cohorts + ",";
                    newJson += "\"JOB_FAMILY\": " + prog.Job_Family + ",";
                    newJson += "\"LOCATION_DETAILS_AVAILABLE\": " + prog.Location_Details_Available;

                    newJson += "}";

                    // Add comma if this isn't the last object
                    if (i < progs.Count() - 1)
                    {
                        newJson += ",";
                        i++;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }
            

            newJson += "]};";
            //$("#json-output-container").html(newJson);

            ViewBag.GeneratedJSON = newJson;

            return View(progs);
        }

        [HttpGet]
        public IActionResult GenerateLocData()  // Generates the list of JSON data that will be used for the live site locations page
        {
            var opps = _db.Opportunities;

            // Generate the string of JSON
            string newJson = "var locations = { data: [";

            int i = 0;

            foreach(OpportunityModel opp in opps)
            {
                newJson += "{";

                newJson += "\"ID\": " + opp.Id + ",";
                newJson += "\"GROUPID\": " + opp.Group_Id + ",";
                newJson += "\"SERVICE\": " + opp.Service + ",";
                newJson += "\"PROGRAM\": " + opp.Program_Name + ",";
                newJson += "\"INSTALLATION\": " + opp.Installation + ",";
                newJson += "\"CITY\": " + opp.City + ",";
                newJson += "\"STATE\": " + opp.State + ",";
                newJson += "\"ZIP\": " + opp.Zip + ",";
                //newJson += "\"POC\": " + modelJson. + ",";
                //newJson += "\"POCEMAIL\": " + modelJson.id + ",";
                //newJson += "\"POCPHONE\": " + modelJson.id + ",";
                newJson += "\"EMPLOYERPOC\": " + opp.Employer_Poc_Name + ",";
                newJson += "\"EMPLOYERPOCEMAIL\": " + opp.Employer_Poc_Email + ",";
                //newJson += "\"EMPLOYERPOCPHONE\": " + modelJson.id + ",";
                newJson += "\"DATEPROGRAMINITIATED\": " + opp.Date_Program_Initiated + ",";
                newJson += "\"DURATIONOFTRAINING\": " + opp.Training_Duration + ",";
                newJson += "\"SUMMARYDESCRIPTION\": " + opp.Summary_Description + ",";
                newJson += "\"JOBSDESCRIPTION\": " + opp.Jobs_Description + ",";
                newJson += "\"LOCATIONSOFPROSPECTIVEJOBSBYSTATE\": " + opp.Locations_Of_Prospective_Jobs_By_State + ",";
                //newJson += "\"NUMBEROFPERSONNELEMPLOYED\": " + modelJson.id + ",";
                newJson += "\"TARGETMOCs\": " + opp.Target_Mocs + ",";
                //newJson += "\"OTHERELIGIBILITYFACTORS\": " + modelJson.id + ",";
                //newJson += "\"NUMBEROFGRADUATESTODATE\": " + modelJson.id + ",";
                newJson += "\"OTHER\": " + opp.Other + ",";
                newJson += "\"MOUs\": " + opp.Mous + ",";
                newJson += "\"LAT\": " + opp.Lat + ",";
                newJson += "\"LONG\": " + opp.Long +",";
                newJson += "\"COST\": " + opp.Cost + ",";
                newJson += "\"SALARY\": " + opp.Salary + ",";
                newJson += "\"NATIONWIDE\": " + opp.Nationwide;

                newJson += "}";

                // Add comma if this isn't the last object
                if (i < opps.Count() - 1)
                {
                    newJson += ",";
                    i++;
                }
            }

            newJson += "]};";
            //$("#json-output-container").html(newJson);

            ViewBag.GeneratedJSON = newJson;

            return View(opps);
        }

        [HttpGet]
        public IActionResult GenerateSpouseData()  // Generates the list of JSON data that will be used for the live site organizations page
        {
            var progs = _db.Programs;

            // Generate the string of JSON
            string newJson = "var spouses = { data: [";

            int i = 0;

            foreach (ProgramModel prog in progs)
            {
                // We need to check this differently than the others since we don't know the state of every programs spouse offerings...
                // Add comma if this isn't the first object... if we did this in the end like the other generation code, we could end up with commas at the end that shouldnt be there
                if (i != 0)
                {
                    newJson += ",";
                    i++;
                }

                newJson += "{";

                var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);

                string urlToDisplay = org != null ? org.Organization_Url : "";
                /*                 * 
                "PROGRAM": "The Park Clinic for Plastic Surgery - Healthcare Admin Internship",
                "URL": "https://www.theparkplasticsurgery.com/",
                "NATIONWIDE": 0,
                "ONLINE": 0,
                "DELIVERY_METHOD": "In-person",
                "STATES": "AL"
                */

                newJson += "\"PROGRAM\": " + prog.Program_Name + ",";
                newJson += "\"URL\": " + urlToDisplay + ",";
                newJson += "\"NATIONWIDE\": " + prog.Nationwide + ",";
                newJson += "\"ONLINE\": " + prog.Online + ",";
                newJson += "\"DELIVERY_METHOD\": " + prog.Delivery_Method + ",";
                newJson += "\"STATES\": " + prog.States_Of_Program_Delivery;

                newJson += "}";
            }

            newJson += "]};";
            //$("#json-output-container").html(newJson);

            ViewBag.GeneratedJSON = newJson;

            return View(progs);
        }

        [HttpGet]
        public IActionResult GenerateAFData()
        {
            return View();
        }

        private byte[] Serialize(object value, JsonSerializerSettings jsonSerializerSettings)
        {
            var result = JsonConvert.SerializeObject(value, jsonSerializerSettings);

            return Encoding.UTF8.GetBytes(result);
        }

        public void UpdateOrganizationStates()
        {
            List<OrganizationModel> orgs = _db.Organizations.ToList();

            foreach(OrganizationModel org in orgs)
            {
                UpdateOrgStatesOfProgramDeliveryNonAsync(org);
            }
        }

        private void UpdateOrgStatesOfProgramDeliveryNonAsync(OrganizationModel org)
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

        public IActionResult GenerateOrganizationJSON()
        {

            //org page should only pull active orgs with active programs

            /*var download = Serialize(_db.Organizations, new JsonSerializerSettings());

            return File(download, "application/json", "organizations-" + DateTime.Today.ToString("MM-dd-yy") + ".json");*/

            var orgs = _db.Organizations.AsNoTracking();
            var progs = _db.Programs.AsNoTracking();

            // Generate the string of JSON
            //string newJson = "[";
            StringBuilder newJson = new StringBuilder("[");

            int i = 0;

            int progCount = progs.ToList().Count;

            //List<int> usedOrgIds = new List<int>();
            //usedOrgIds.Clear();

            try
            {
                foreach (var org in orgs)
                {
                    // check if org is active
                    //if (org.Is_Active)
                    //{
                        //var org = _db.Organizations.AsNoTracking().SingleOrDefault(x => x.Id == prog.Organization_Id);
                        List<ProgramModel> relatedProgs = progs.Where(m => m.Organization_Id == org.Id).ToList();

                        string newProgramIds = "";
                        int progInc = 0;

                        // checking for prog active status underneath org is how its set up right now, but maybe shouldnt be in the future

                        foreach (ProgramModel prog in relatedProgs)
                        {
                            //string add = progInc == 0 ? prog.Legacy_Program_Id.ToString() : " " + prog.Legacy_Program_Id;
                            string add = progInc == 0 ? prog.Id.ToString() : " " + prog.Id;
                            newProgramIds += add;
                            progInc++;
                        }

                        newProgramIds.Trim();

                        if (relatedProgs.Count > 0)
                        {
                            //string newDeliveryMethod = GetDeliveryMethodForProg(relatedProgs[0]);   // captured as a input on the create program page
                            //string newProgramDuration = GetProgramDurationForProg(relatedProgs[0]); // ^^
                            //string newCohorts = GetCohortsForProg(relatedProgs[0]);     // supports

                            string newDeliveryMethod = GetDeliveryMethodListForOrg(relatedProgs);   // captured as a input on the create program page
                            string newProgramDuration = GetProgramDurationListForOrg(relatedProgs); // ^^
                            //string newCohorts = GetCohortsAggregateForOrg(relatedProgs);     // supports
                            string opportunityTypes = GetOpportunityTypesListForOrg(relatedProgs);
                            string jobFamilies = GetJobFamiliesListForOrg(relatedProgs);


                            string newCohorts = "No";
                            foreach (ProgramModel p in relatedProgs)
                            {
                                if (p.Nationwide != false)
                                {
                                    newCohorts = "Yes";
                                }
                            }

                            bool nationwide = false;
                            foreach (ProgramModel p in relatedProgs)
                            {
                                if (p.Nationwide != false)
                                {
                                    nationwide = true;
                                }
                            }

                            bool online = false;
                            foreach (ProgramModel p in relatedProgs)
                            {
                                if (p.Online != false)
                                {
                                    online = true;
                                }
                            }

                            bool locationDetailsAvailable = false;
                            foreach (ProgramModel p in relatedProgs)
                            {
                                if (p.Location_Details_Available != false)
                                {
                                    locationDetailsAvailable = true;
                                }
                            }

                            if (i != 0)
                            {
                                newJson.Append(",");
                            }

                            // USE AGGREGATES OF ALL PROGRAMS

                            newJson.Append("{");

                            newJson.Append("\"PROGRAM STATUS\": \"" + (org.Is_Active ? "Active\"," : "Closed\","));
                            //newJson.Append("\"PROVIDER UNIQUE ID\": " + org.Legacy_Provider_Id + ",");
                            newJson.Append("\"PROVIDER UNIQUE ID\": " + org.Id + ",");
                            newJson.Append("\"PROGRAM UNIQUE ID\": \"" + newProgramIds + "\",");
                            newJson.Append("\"PROGRAM\": \"" + org.Name + "\",");
                            newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                            newJson.Append("\"OPPORTUNITY_TYPE\": \"" + opportunityTypes + "\",");
                            newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                            newJson.Append("\"PROGRAM_DURATION\": \"" + newProgramDuration + "\",");
                            newJson.Append("\"STATES\": \"" + org.States_Of_Program_Delivery + "\",");
                            newJson.Append("\"NATIONWIDE\":" + (nationwide == true ? 1 : 0) + ",");
                            newJson.Append("\"ONLINE\":" + (online == true ? 1 : 0) + ",");
                            newJson.Append("\"COHORTS\":\"" + newCohorts + "\",");
                            newJson.Append("\"JOB_FAMILY\":\"" + jobFamilies + "\",");
                            newJson.Append("\"LOCATION_DETAILS_AVAILABLE\":" + (locationDetailsAvailable == true ? 1 : 0));

                            newJson.Append("}");

                            i++;

                            /*string newDeliveryMethod = GetDeliveryMethodForProg(relatedProgs[0]);   // captured as a input on the create program page
                            string newProgramDuration = GetProgramDurationForProg(relatedProgs[0]); // ^^
                            string newCohorts = GetCohortsForProg(relatedProgs[0]);     // supports

                            if (i != 0)
                            {
                                newJson.Append(",");
                            }

                            // USE AGGREGATES OF ALL PROGRAMS

                            newJson.Append("{");

                            newJson.Append("\"PROGRAM STATUS\": \"" + (relatedProgs[0].Program_Status ? "Active" : "Closed (" + relatedProgs[0].Date_Deactivated + ")") + "\",");
                            newJson.Append("\"PROVIDER UNIQUE ID\": " + org.Legacy_Provider_Id + ",");
                            newJson.Append("\"PROGRAM UNIQUE ID\": \"" + newProgramIds + "\",");
                            newJson.Append("\"PROGRAM\": \"" + org.Name + "\",");
                            newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                            newJson.Append("\"OPPORTUNITY_TYPE\": \"" + relatedProgs[0].Opportunity_Type + "\",");
                            newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                            newJson.Append("\"PROGRAM_DURATION\": \"" + newProgramDuration + "\",");
                            newJson.Append("\"STATES\": \"" + relatedProgs[0].States_Of_Program_Delivery + "\",");
                            newJson.Append("\"NATIONWIDE\":" + (relatedProgs[0].Nationwide == true ? 1 : 0) + ",");
                            newJson.Append("\"ONLINE\":" + (relatedProgs[0].Online == true ? 1 : 0) + ",");
                            newJson.Append("\"COHORTS\":\"" + newCohorts + "\",");
                            newJson.Append("\"JOB_FAMILY\":\"" + relatedProgs[0].Job_Family + "\",");
                            newJson.Append("\"LOCATION_DETAILS_AVAILABLE\":" + (relatedProgs[0].Location_Details_Available == true ? 1 : 0));

                            newJson.Append("}");

                            i++;*/

                            // Add comma if this isn't the last object
                            //if (i < progCount - 1)
                            //{
                            //newJson.Append(",");
                            //i++;
                            //}
                        }
                    //}                                
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }



            // OLD CODE

            /*try
            {
                foreach (var prog in progs)
                {
                    foreach (var org in orgs)
                    {
                        //var org = _db.Organizations.AsNoTracking().SingleOrDefault(x => x.Id == prog.Organization_Id);

                        if (org.Id == prog.Organization_Id)
                        {
                            //if (usedOrgIds.Contains(org.Id) == false)
                            //{
                                newJson.Append("{");

                                //string urlToDisplay = org != null ? org.Organization_Url : "";
                                //int nat = prog.Nationwide == true ? 1 : 0;
                                //int online = prog.Online == true ? 1 : 0;
                                //int details = prog.Location_Details_Available == true ? 1 : 0;

                                

                                string newProgramIds = "";
                                int progInc = 0;
                                foreach (var p in progs)
                                {
                                    if (p.Organization_Id == org.Id)
                                    {
                                        string add = progInc == 0 ? p.Legacy_Program_Id.ToString() : " " + p.Legacy_Program_Id;
                                        newProgramIds += add;
                                        progInc++;
                                    }
                                }

                                newProgramIds.Trim();

                                string newDeliveryMethod = GetDeliveryMethodForProg(prog);
                                string newProgramDuration = GetProgramDurationForProg(prog);
                                string newCohorts = GetCohortsForProg(prog);

                                newJson.Append("\"PROGRAM STATUS\": \"" + (prog.Program_Status ? "Active" : "Closed (" + prog.Date_Deactivated + ")") + "\",");
                                newJson.Append("\"PROVIDER UNIQUE ID\": " + org.Legacy_Provider_Id + ",");
                                newJson.Append("\"PROGRAM UNIQUE ID\": \"" + newProgramIds + "\",");
                                newJson.Append("\"PROGRAM\": \"" + org.Name + "\",");
                                newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                                newJson.Append("\"OPPORTUNITY_TYPE\": \"" + prog.Opportunity_Type + "\",");
                                newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                                newJson.Append("\"PROGRAM_DURATION\": \"" + newProgramDuration + "\",");
                                newJson.Append("\"STATES\": \"" + prog.States_Of_Program_Delivery + "\",");
                                newJson.Append("\"NATIONWIDE\":" + (prog.Nationwide == true ? 1 : 0) + ",");
                                newJson.Append("\"ONLINE\":" + (prog.Online == true ? 1 : 0) + ",");
                                newJson.Append("\"COHORTS\":\"" + newCohorts + "\",");
                                newJson.Append("\"JOB_FAMILY\":\"" + prog.Job_Family + "\",");
                                newJson.Append("\"LOCATION_DETAILS_AVAILABLE\":" + (prog.Location_Details_Available == true ? 1 : 0));

                                // Pull delivery method data out of database

                                newJson.Append("}");

                                //usedOrgIds.Add(org.Id);

                                // Add comma if this isn't the last object
                                if (i < progCount - 1)
                                {
                                    newJson.Append(",");
                                    i++;
                                }
                            //}
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }*/


            newJson.Append("]");
            //$("#json-output-container").html(newJson);

            /*List<ProgramModel> programs = _db.Programs.ToList();
            string newJson = "var orgs = { data: [" + JsonConvert.SerializeObject(programs, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) + "]};";*/

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "application/json", "AF-Org-" + DateTime.Today.ToString("MM-dd-yy") + ".json");
        }

        public string GetDeliveryMethodListForOrg(List<ProgramModel> relatedProgs)
        {
            string returnString = "";
            int i = 0;

            List<string> dms = new List<string>();
            List<int> dmids = new List<int>();

            //bool isScottsdale = false;


            foreach (ProgramModel p in relatedProgs)
            {
                List<ProgramDeliveryMethod> pdm = _db.ProgramDeliveryMethod.AsNoTracking().Where(x => x.Program_Id == p.Id).ToList();

                foreach(ProgramDeliveryMethod d in pdm)
                {
                    dmids.Add(d.Delivery_Method_Id);
                }

                /*if(p.Organization_Id == 121)
                {
                    isScottsdale = true;
                }*/
            }

            // Get unique values
            List<int> uniqueDmids = dmids.Distinct().ToList();

            /*if(isScottsdale)
            {
                foreach(int j in uniqueDmids)
                {
                    Console.WriteLine("uniqueDmids j: " + j);
                }
            }*/
            

            // Combine values into a comma separated string
            foreach (int dm in uniqueDmids)
            {
                string val = "";
                if (dm == 1)
                {
                    val = "In-person";
                }
                else if (dm == 2)
                {
                    val = "Online";
                }
                else if (dm == 3)
                {
                    val = "Hybrid (In-Person and Online)";
                }

                if (i == 0)
                {
                    returnString += val;
                }
                else if (i < uniqueDmids.Count - 1)
                {
                    returnString += ", " + val;
                }
                else
                {
                    returnString += ", and " + val;
                }

                i++;
            }

            return returnString;
        }

        public string GetProgramDurationListForOrg(List<ProgramModel> relatedProgs)
        {
            string returnString = "";
            int i = 0;

            List<string> pds = new List<string>();

            // Get a list of items from the programs
            foreach (ProgramModel p in relatedProgs)
            {
                pds.Add(p.Program_Duration.ToString());
            }

            // Get unique values
            List<string> uniquePds = pds.Distinct().ToList();

            // Combine values into a comma separated string
            foreach (string pd in uniquePds)
            {
                string val = "";

                switch(pd)
                {
                case "0":
                    {
                            val = "1 - 30 days";
                        break;
                    }
                case "1":
                    {
                            val = "31 - 60 days";
                        break;
                    }
                case "2":
                    {
                            val = "61 - 90 days";
                        break;
                    }
                case "3":
                    {
                            val = "91 - 120 days";
                        break;
                    }
                case "4":
                    {
                            val = "121 - 150 days";
                        break;
                    }
                case "5":
                    {
                            val = "151 - 180 days";
                        break;
                    }
                case "6":
                    {
                            val = "Individually Developed – not to exceed 40 hours";
                        break;
                    }
                case "7":
                    {
                            val = "Self-paced";
                        break;
                    }
                }

                if (i == 0)
                {
                    returnString += val;
                }
                else if (i < uniquePds.Count - 1)
                {
                    returnString += ", " + val;
                }
                else
                {
                    returnString += ", and " + val;
                }

                i++;
            }

            return returnString;
        }

        /*public string GetCohortsAggregateForOrg(List<ProgramModel> relatedProgs)
        {
            string returnString = "";
            int i = 0;

            List<string> dms = new List<string>();

            // Get a list of items from the programs
            foreach (ProgramModel p in relatedProgs)
            {
                dms.Add(p.Delivery_Method);
            }

            // Get unique values
            List<string> uniqueDms = dms.Distinct().ToList();

            // Combine values into a comma separated string
            foreach (string dm in uniqueDms)
            {
                if (i == 0)
                {
                    returnString += dm;
                }
                else
                {
                    returnString += ", " + dm;
                }

                i++;
            }

            return returnString;
        }
        */
        public string GetOpportunityTypesListForOrg(List<ProgramModel> relatedProgs)
        {
            string returnString = "";
            int i = 0;

            List<string> ots = new List<string>();

            // Get a list of items from the programs
            foreach (ProgramModel p in relatedProgs)
            {
                ots.Add(p.Opportunity_Type);
            }

            // Get unique values
            List<string> uniqueOts = ots.Distinct().ToList();

            // Combine values into a comma separated string
            foreach (string ot in uniqueOts)
            {
                if (i == 0)
                {
                    returnString += ot;
                }
                else if (i < uniqueOts.Count - 1)
                {
                    returnString += ", " + ot;
                }
                else
                {
                    returnString += ", and " + ot;
                }

                i++;
            }

            return returnString;
        }

        public string GetJobFamiliesListForOrg(List<ProgramModel> relatedProgs)
        {
            string returnString = "";
            int i = 0;

            List<string> jfs = new List<string>();

            foreach (ProgramModel p in relatedProgs)
            {
                List<ProgramJobFamily> pjfs = _db.ProgramJobFamily.Where(x => x.Program_Id == p.Id).ToList();

                foreach(ProgramJobFamily jf in pjfs)
                {
                    JobFamily fam = _db.JobFamilies.FirstOrDefault(x => x.Id == jf.Job_Family_Id);

                    jfs.Add(fam.Name);
                }
            }

            // Get unique values
            List<string> uniqueJfs = jfs.Distinct().ToList();

            // Combine values into a comma separated string
            foreach (string j in uniqueJfs)
            {
                if (i == 0)
                {
                    returnString += j;
                }
                else if (i < uniqueJfs.Count - 1)
                {
                    returnString += ", " + j;
                }
                else
                {
                    returnString += ", and " + j;
                }

                i++;
            }

            return returnString;
        }

        public IActionResult GenerateProgramJSON()
        {
            /*
             *  {
                   "Provider Unique ID": 1,
                   "Program Unique ID": 1,
                   "Program Status": "Active ",
                   "Program & Organization": "2 Circle Consulting Inc",
                   "Program Duration": "151 - 180 days"
                 },
             * 
             */

            var progs = _db.Programs.AsNoTracking();
            var orgs = _db.Organizations.AsNoTracking();

            // Generate the string of JSON
            //string newJson = "[";
            StringBuilder newJson = new StringBuilder("[");

            int i = 0;

            int progCount = progs.ToList().Count;

            try
            {
                foreach (var prog in progs)
                {
                    //var org = _db.Organizations.AsNoTracking().FromCache().SingleOrDefault(x => x.Id == prog.Organization_Id);

                    string newProgramDuration = GetProgramDurationForProg(prog);

                    if (prog.Is_Active)
                    {
                        var org = orgs.FromCache().SingleOrDefault(x => x.Id == prog.Organization_Id);

                        if (org.Is_Active)
                        {
                            // Add comma if this isn't the last object
                            if (i != 0)
                            {
                                newJson.Append(",");
                            }

                            newJson.Append("{");

                            //newJson.Append("\"Provider Unique ID\": " + prog.Legacy_Provider_Id + ",");
                            //newJson.Append("\"Program Unique ID\": " + prog.Legacy_Program_Id + ",");
                            newJson.Append("\"Provider Unique ID\": " + prog.Organization_Id + ",");
                            newJson.Append("\"Program Unique ID\": " + prog.Id + ",");
                            newJson.Append("\"Program Status\": \"" + (prog.Is_Active ? "Active" : "Closed (" + prog.Date_Deactivated + ")") + "\",");
                            newJson.Append("\"Program & Organization\": \"" + prog.Organization_Name + "\",");
                            newJson.Append("\"Program Duration\": \"" + newProgramDuration + "\"");

                            newJson.Append("}");

                            i++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }

            newJson.Append("]");

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "application/json", "AF-Prog-" + DateTime.Today.ToString("MM-dd-yy") + ".json");


            //var download = Serialize(_db.Programs, new JsonSerializerSettings());

            //return File(download, "application/json", "programs-" + DateTime.Today.ToString("MM-dd-yy") + ".json");
        }

        public IActionResult GenerateOpportunityJSON()
        { 
            var orgs = _db.Organizations.AsNoTracking();
            var progs = _db.Programs.AsNoTracking();
            var opps = _db.Opportunities.AsNoTracking();

            // Generate the string of JSON
            //string newJson = "[";
            StringBuilder newJson = new StringBuilder("[");

            int i = 0;

            int oppCount = opps.ToList().Count;
             
            try
            {
                foreach (var opp in opps)
                {

                    var prog = progs.AsNoTracking().SingleOrDefault(x => x.Id == opp.Program_Id);
                    var org = orgs.AsNoTracking().SingleOrDefault(x => x.Id == opp.Organization_Id);

                    string newProgramDuration = GetProgramDurationForProg(prog);

                    if(org.Is_Active)
                    {
                        if(prog.Is_Active)
                        {
                            if (opp.Is_Active)
                            {
                                // Add comma if this isn't the last object
                                if (i != 0)
                                {
                                    newJson.Append(",");
                                }

                                newJson.Append("{");

                                newJson.Append("\"PROGRAM STATUS\":\"" + (prog.Is_Active ? "Active" : "Closed (" + prog.Date_Deactivated.ToString("MM/dd/yyyy") + ")") + "\",");
                                //newJson.Append("\"PROVIDER UNIQUE ID\":" + opp.Legacy_Provider_Id + ",");
                                //newJson.Append("\"PROGRAM UNIQUE ID\":" + opp.Legacy_Program_Id + ",");
                                newJson.Append("\"PROVIDER UNIQUE ID\":" + opp.Organization_Id + ",");
                                newJson.Append("\"PROGRAM UNIQUE ID\":" + opp.Program_Id + ",");
                                newJson.Append("\"ID\":" + opp.Id + ",");
                                newJson.Append("\"GROUPID\":" + opp.Group_Id + ",");
                                newJson.Append("\"SERVICE\":\"" + opp.Service + "\",");
                                newJson.Append("\"PROGRAM\":\"" + org.Name + " - " + opp.Program_Name + "\",");
                                newJson.Append("\"INSTALLATION\":\"" + opp.Installation + "\",");
                                newJson.Append("\"CITY\":\"" + opp.City + "\",");
                                newJson.Append("\"STATE\":\"" + opp.State + "\",");
                                newJson.Append("\"ZIP\":\"" + opp.Zip + "\",");
                                newJson.Append("\"POC\":\"" + org.Poc_First_Name + " " + org.Poc_Last_Name + "\",");
                                newJson.Append("\"POCEMAIL\":\"" + org.Poc_Email + "\",");
                                newJson.Append("\"POCPHONE\":\"" + org.Poc_Phone + "\",");
                                newJson.Append("\"EMPLOYERPOC\":\"" + org.Poc_First_Name + " " + org.Poc_Last_Name + "\",");
                                newJson.Append("\"EMPLOYERPOCEMAIL\":\"" + org.Poc_Email + "\",");
                                newJson.Append("\"EMPLOYERPOCPHONE\":\"" + org.Poc_Phone + "\",");
                                newJson.Append("\"DATEPROGRAMINITIATED\":\"" + opp.Date_Program_Initiated.ToString("MM/dd/yyyy") + "\",");
                                newJson.Append("\"DURATIONOFTRAINING\":\"" + newProgramDuration + "\",");
                                newJson.Append("\"SUMMARYDESCRIPTION\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Summary_Description)) + "\",");
                                newJson.Append("\"JOBSDESCRIPTION\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Jobs_Description)) + "\",");
                                newJson.Append("\"LOCATIONSOFPROSPECTIVEJOBSBYSTATE\":\"" + opp.Locations_Of_Prospective_Jobs_By_State + "\",");
                                newJson.Append("\"NUMBEROFPERSONNELEMPLOYED\":\"\",");   // SHOULD WE JUST DROP THIS FIELD? ITS ALWAYS BLANK
                                newJson.Append("\"TARGETMOCs\":\"" + opp.Target_Mocs + "\",");
                                newJson.Append("\"OTHERELIGIBILITYFACTORS\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Other_Eligibility_Factors)) + "\",");
                                newJson.Append("\"NUMBEROFGRADUATESTODATE\":\"\",");   // SHOULD WE JUST DROP THIS FIELD? ITS ALWAYS BLANK
                                newJson.Append("\"OTHER\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Other)) + "\",");
                                newJson.Append("\"MOUs\":\"" + (opp.Mous ? "Y" : "N") + "\",");
                                newJson.Append("\"LAT\":" + opp.Lat + ",");
                                newJson.Append("\"LONG\":" + opp.Long + ",");
                                newJson.Append("\"COST\":\"" + opp.Cost + "\",");
                                newJson.Append("\"SALARY\":\"" + opp.Salary + "\",");
                                newJson.Append("\"NATIONWIDE\":" + (opp.Nationwide ? 1 : 0));

                                newJson.Append("}");

                                i++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }


            newJson.Append("]");
            //$("#json-output-container").html(newJson);

            /*List<ProgramModel> programs = _db.Programs.ToList();
            string newJson = "var orgs = { data: [" + JsonConvert.SerializeObject(programs, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) + "]};";*/

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "application/json", "AF-Locs-" + DateTime.Today.ToString("MM-dd-yy") + ".json"); ;


            //var download = Serialize(_db.Opportunities, new JsonSerializerSettings());

            //return File(download, "application/json", "opportunities-" + DateTime.Today.ToString("MM-dd-yy") + ".json");
        }

        public IActionResult GenerateSpouseJSON()
        {
            var orgs = _db.Organizations.AsNoTracking();
            var progs = _db.Programs.AsNoTracking();
            var opps = _db.Opportunities.AsNoTracking();

            // Generate the string of JSON
            //string newJson = "[";
            StringBuilder newJson = new StringBuilder("[");

            int i = 0;

            List<int> spouseOrgs = new List<int>();

            try
            {
                foreach (ProgramModel prog in progs)
                {
                    if (prog.For_Spouses && prog.Is_Active)
                    {
                        var org = orgs.AsNoTracking().SingleOrDefault(x => x.Id == prog.Organization_Id);

                        /*bool wasFound = false;

                        // Make sure we arent duplicating orgs/progs in the data
                        foreach (int id in spouseOrgs)
                        {
                            if (id == prog.Organization_Id)
                            {
                                wasFound = true;
                            }
                        }

                        if (!wasFound)
                        {*/
                        //var prog = _db.Programs.AsNoTracking().FromCache().SingleOrDefault(x => x.Id == opp.Program_Id);
                        //var org = _db.Organizations.AsNoTracking().SingleOrDefault(x => x.Id == prog.Organization_Id);

                        if(org.Is_Active)
                        {
                            string newProgramDuration = GetProgramDurationForProg(prog);

                            if (i != 0)
                            {
                                newJson.Append(",");
                            }

                            newJson.Append("{");

                            string newDeliveryMethod = GetDeliveryMethodForProg(prog);

                            newJson.Append("\"PROGRAM STATUS\":\"" + (prog.Is_Active ? "Active" : "Closed (" + prog.Date_Deactivated.ToString("MM/dd/yyyy") + ")") + "\",");
                            //newJson.Append("\"PROVIDER UNIQUE ID\":" + prog.Legacy_Provider_Id + ",");
                            //newJson.Append("\"PROGRAM UNIQUE ID\":" + prog.Legacy_Program_Id + ",");
                            newJson.Append("\"PROVIDER UNIQUE ID\":" + prog.Organization_Id + ",");
                            newJson.Append("\"PROGRAM UNIQUE ID\":" + prog.Id + ",");
                            newJson.Append("\"PROGRAM\": \"" + org.Name + "\",");
                            newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                            newJson.Append("\"NATIONWIDE\":" + (prog.Nationwide ? 1 : 0) + ",");
                            newJson.Append("\"ONLINE\":" + (prog.Online ? 1 : 0) + ",");
                            newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                            newJson.Append("\"STATES\": \"" + prog.States_Of_Program_Delivery + "\"");

                            i++;

                            newJson.Append("}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }


            newJson.Append("]");

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "application/json", "AF-Spouses-" + DateTime.Today.ToString("MM-dd-yy") + ".json");
        }

        [HttpGet]
        public IActionResult GenerateUpdateData()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DownloadOrgData()  // Generates the list of JSON data that will be used for the live site organizations page
        {
            var orgs = _db.Organizations.AsNoTracking();
            var progs = _db.Programs.AsNoTracking();

            // Generate the string of JSON
            //string newJson = "var orgs = { data: [";
            StringBuilder newJson = new StringBuilder("var orgs = { data: [");

            int i = 0;

            int progCount = progs.ToList().Count;

            try
            {
                foreach (var org in orgs)
                {
                    if(org.Is_Active)
                    {
                        bool hasActiveProgram = false;

                        //foreach(var org in orgs)
                        //{
                        var subProgs = progs.Where(x => x.Organization_Id == org.Id).ToList();
                        List<string> deliveryMethods = new List<string>();
                        List<string> opportunityTypes = new List<string>();
                        List<int> durations = new List<int>();

                        List<string> jobFamilies = new List<string>();
                        List<string> states = new List<string>();

                        // Pull delivery method data out of database
                        string newDeliveryMethod = "";
                        string newOpportunityType = "";
                        string newProgramDuration = "";
                        string newCohorts = "";
                        string newJobFamilies = "";
                        string newStatesOfProgramDelivery = "";

                        bool nationwide = false;
                        bool online = false;
                        bool location = false;

                        string newName = "";
                        string progName = "";

                        foreach (ProgramModel prog in subProgs)
                        {
                            if (prog.Is_Active)
                            {
                                // If has singular active program at least, we include it in the export.
                                hasActiveProgram = true;

                                // Delivery Methods
                                // Check for duplicates
                                bool deliveryMethodExists = false;
                                foreach (string dm in deliveryMethods)
                                {
                                    if (dm == prog.Delivery_Method)
                                    {
                                        deliveryMethodExists = true;
                                    }
                                }

                                // If duration doesnt exist, add it to list
                                if (!deliveryMethodExists)
                                {
                                    deliveryMethods.Add(prog.Delivery_Method);
                                }

                                // Opportunity Type
                                // Check for duplicates
                                bool opportunityTypeExists = false;
                                foreach (string ot in opportunityTypes)
                                {
                                    if (ot == prog.Opportunity_Type)
                                    {
                                        opportunityTypeExists = true;
                                    }
                                }

                                // If duration doesnt exist, add it to list
                                if (!opportunityTypeExists)
                                {
                                    opportunityTypes.Add(prog.Opportunity_Type);
                                }

                                // Durations
                                // Check for duplicates
                                bool durationExists = false;
                                foreach (int d in durations)
                                {
                                    if (d == prog.Program_Duration)
                                    {
                                        durationExists = true;
                                    }
                                }

                                // If duration doesnt exist, add it to list
                                if (!durationExists)
                                {
                                    durations.Add(prog.Program_Duration);
                                }

                                // Job Families
                                // Check for duplicates
                                /*bool jobFamilyExists = false;
                                string jobFamilyString = GetJobFamiliesListForProg(prog);
                                foreach (string j in jobFamilies)
                                {
                                    if (jobFamilyString.Contains(j))
                                    {
                                        jobFamilyExists = true;
                                    }
                                }

                                // If duration doesnt exist, add it to list
                                if (!jobFamilyExists)
                                {
                                    jobFamilies.Add(prog.Delivery_Method);
                                }*/

                                // States
                                // Check for duplicates

                                Console.WriteLine("checking program states for program: " + prog.Program_Name);
                                string[] stateSplit = prog.States_Of_Program_Delivery.Split(", ");
                                //foreach (string s in states)
                                //{
                                //Console.WriteLine("-s: " + s);
                                foreach (string ss in stateSplit)
                                {
                                    Console.WriteLine("-ss: " + ss);
                                    bool stateExists = false;

                                    foreach (string s in states)
                                    {
                                        if (ss == s)
                                        {
                                            stateExists = true;
                                        }
                                    }

                                    // If duration doesnt exist, add it to list
                                    if (!stateExists)
                                    {
                                        Console.WriteLine("adding state: " + ss);
                                        states.Add(ss);
                                    }
                                }
                                //}

                                newDeliveryMethod = GetDeliveryMethodForProg(prog);

                                newCohorts = GetCohortsForProg(prog);
                                newJobFamilies = GetJobFamiliesListForProg(prog);

                                if (prog.Nationwide == true)
                                {
                                    nationwide = true;
                                }

                                if (prog.Online == true)
                                {
                                    online = true;
                                }

                                if (prog.Location_Details_Available == true)
                                {
                                    location = true;
                                }

                                progName = prog.Program_Name;
                            }
                        }

                        newOpportunityType = GetOpportunityTypeForTypeList(opportunityTypes);
                        newProgramDuration = GetProgramDurationForDurationList(durations);//GetProgramDurationForProg(prog);
                        newStatesOfProgramDelivery = String.Join(", ", states);

                        //string urlToDisplay = org != null ? org.Organization_Url : "";
                        //int nat = prog.Nationwide == true ? 1 : 0;
                        //int online = prog.Online == true ? 1 : 0;
                        //int details = prog.Location_Details_Available == true ? 1 : 0;

                        if (hasActiveProgram)
                        {
                            if (org.Name.Equals(progName))
                            {
                                newName = org.Name;
                            }
                            else
                            {
                                newName = org.Name + " - " + progName;
                            }

                            // Add comma if this isn't the last object
                            if (i > 0)
                            {
                                newJson.Append(",");
                            }

                            newJson.Append("{");

                            newJson.Append("\"PROGRAM\": \"" + newName + "\",");
                            newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                            newJson.Append("\"OPPORTUNITY_TYPE\": \"" + newOpportunityType + "\",");
                            newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                            newJson.Append("\"PROGRAM_DURATION\": \"" + newProgramDuration + "\",");
                            newJson.Append("\"STATES\": \"" + newStatesOfProgramDelivery/*prog.States_Of_Program_Delivery*/ + "\",");
                            newJson.Append("\"NATIONWIDE\": " + (nationwide == true ? 1 : 0) + ",");
                            newJson.Append("\"ONLINE\": " + (online == true ? 1 : 0) + ",");
                            newJson.Append("\"COHORTS\": \"" + newCohorts + "\",");
                            newJson.Append("\"JOB_FAMILY\": \"" + newJobFamilies + "\",");
                            newJson.Append("\"LOCATION_DETAILS_AVAILABLE\": " + (location == true ? 1 : 0));

                            newJson.Append("}");

                            i++;
                        }
                    }                    
                }    
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }

            newJson.Append("]};");
            //$("#json-output-container").html(newJson);

            /*List<ProgramModel> programs = _db.Programs.ToList();
            string newJson = "var orgs = { data: [" + JsonConvert.SerializeObject(programs, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) + "]};";*/

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "text/plain", "organizationsData.txt");
        }

        private string GetJobFamiliesListForProg(ProgramModel prog)
        {
            string jfs = "";

            var pjf = _db.ProgramJobFamily.Where(x => x.Program_Id == prog.Id).ToList();

            Console.WriteLine("==pjf.Count for " + prog.Program_Name + " = " + pjf.Count);

            int count = pjf.Count;
            int i = 0;

            foreach (var jf in pjf)
            {
                Console.WriteLine("jf: " + jf.Job_Family_Id);
                JobFamily fam = _db.JobFamilies.FirstOrDefault(x => x.Id == jf.Job_Family_Id);

                if(i == 0)
                {
                    if(fam != null)
                    {
                        jfs += fam.Name;
                    }                    
                }
                else if(i < count - 1)
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

            return jfs;
        }

        private string GetDeliveryMethodForProg(ProgramModel prog)
        {
            string dm = "";

            Console.WriteLine("GetDeliveryMethodForProg prog: " + prog.Program_Name + " - #" + prog.Id);

            List<ProgramDeliveryMethod> pdm = _db.ProgramDeliveryMethod.AsNoTracking().Where(x => x.Program_Id == prog.Id).ToList();

            int count = 0;

            foreach(ProgramDeliveryMethod p in pdm)
            {
                if(count != 0)
                {
                    dm += " and ";
                }

                if (p != null)
                {
                    if (p.Delivery_Method_Id == 1)
                    {
                        dm += "In-person";
                    }
                    else if (p.Delivery_Method_Id == 2)
                    {
                        dm += "Online";
                    }
                    else if (p.Delivery_Method_Id == 3)
                    {
                        dm += "Hybrid (In-Person and Online)";
                    }

                    count++;
                }                
            }

            return dm;
        }

        private string GetProgramDurationForProg(ProgramModel prog)
        {
            //if(prog.Delivery_Method == null) { return "Individually Developed – not to exceed 40 hours"; }


            // Program Duration
            string pd = "";

            Console.WriteLine("Program Name: " + prog.Program_Name + " -- Duration: " + prog.Program_Duration);
            switch (prog.Program_Duration)
            {
                case 0:
                    {
                        pd = "1 - 30 days";
                        break;
                    }
                case 1:
                    {
                        pd = "31 - 60 days";
                        break;
                    }
                case 2:
                    {
                        pd = "61 - 90 days";
                        break;
                    }
                case 3:
                    {
                        pd = "91 - 120 days";
                        break;
                    }
                case 4:
                    {
                        pd = "121 - 150 days";
                        break;
                    }
                case 5:
                    {
                        pd = "151 - 180 days";
                        break;
                    }
                case 6:
                    {
                        pd = "Individually Developed – not to exceed 40 hours";
                        break;
                    }
                case 7:
                    {
                        pd = "Self-paced";
                        break;
                    }
                default:
                    {
                        pd = "Individually Developed – not to exceed 40 hours";
                        break;
                    }
            }
            Console.WriteLine("returning: " + pd);

            return pd;
        }

        private string GetOpportunityTypeForTypeList(List<string> list)
        {
            // Opporunity Types
            string ot = "";
            int i = 0;
            int length = list.Count;

            foreach (string o in list)
            {
                ot += o;

                if (i < length - 1)
                {
                    ot += ", ";
                }

                i++;
            }


            //Console.WriteLine("returning list of opportunity types as: " + ot);

            return ot;
        }

        private string GetProgramDurationForDurationList(List<int> list)
        {
            // Program Duration
            string pd = "";
            int i = 0;
            int length = list.Count;

            //Console.WriteLine("Program Name: " + prog.Program_Name + " -- Duration: " + prog.Program_Duration);
            foreach(int d in list)
            {
                switch (d)
                {
                    case 0:
                        {
                            pd += "1 - 30 days";
                            break;
                        }
                    case 1:
                        {
                            pd += "31 - 60 days";
                            break;
                        }
                    case 2:
                        {
                            pd += "61 - 90 days";
                            break;
                        }
                    case 3:
                        {
                            pd += "91 - 120 days";
                            break;
                        }
                    case 4:
                        {
                            pd += "121 - 150 days";
                            break;
                        }
                    case 5:
                        {
                            pd += "151 - 180 days";
                            break;
                        }
                    case 6:
                        {
                            pd += "Individually Developed – not to exceed 40 hours";
                            break;
                        }
                    case 7:
                        {
                            pd += "Self-paced";
                            break;
                        }
                    default:
                        {
                            pd += "Individually Developed – not to exceed 40 hours";
                            break;
                        }
                }

                if(i < length - 1)
                {
                    pd += ", ";
                }

                i++;
            }

            
            //Console.WriteLine("returning list of durations as: " + pd);

            return pd;
        }

        private string GetCohortsForProg(ProgramModel prog)
        {
            string c = "";

            if(prog.Support_Cohorts == true)
            {
                c = "Yes";
            }
            else
            {
                c = "No";
            }

            return c;
        }

        [HttpGet]
        public IActionResult DownloadOrganizationsCSV()
        {
            var progs = _db.Programs;

            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("PROGRAM|URL|OPPORTUNITY_TYPE|DELIVERY_METHOD|PROGRAM_DURATION|STATES|NATIONWIDE|ONLINE|COHORTS|JOB_FAMILY|LOCATION_DETAILS_AVAILABLE");

                foreach (ProgramModel prog in progs)
                {
                    var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);
                    string urlToDisplay = org != null ? org.Organization_Url : "";

                    stringBuilder.AppendLine($"" + $"{prog.Program_Name}|{urlToDisplay}|{prog.Opportunity_Type}|{ prog.Delivery_Method}|{ prog.Program_Duration}|{ prog.States_Of_Program_Delivery}|{ prog.Nationwide}|{ prog.Online}|{ prog.Support_Cohorts}|{ prog.Job_Family}|{ prog.Location_Details_Available}");
                }

                return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", "orgs-" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
            }
            catch
            {
                return Error();
            }
        }

        [HttpGet]
        public IActionResult DownloadLocData()  // Generates the list of JSON data that will be used for the live site locations page
        {
            /*var opps = _db.Opportunities.AsNoTracking().Select(o => new
            { 
                o.Id, 
                o.Group_Id, 
                o.Service,
                o.Program_Name,
                o.Installation,
                o.City,
                o.State,
                o.Zip,
                o.Employer_Poc_Name,
                o.Employer_Poc_Email,
                o.Date_Program_Initiated,
                o.Training_Duration,
                o.Summary_Description,
                o.Jobs_Description,
                o.Locations_Of_Prospective_Jobs_By_State,
                o.Target_Mocs,
                o.Other,
                o.Mous,
                o.Lat,
                o.Long,
                o.Cost,
                o.Salary,
                o.Nationwide
            });

            // Generate the string of JSON
            string newJson = "var locations = { data: [";

            int i = 0;

            int oppsCount = opps.ToList().Count;
            

            foreach (var opp in opps)
            {
                //int nat = opp.Nationwide == true ? 1 : 0;

                newJson += "{";

                newJson += "\"ID\": " + opp.Id + ",";
                newJson += "\"GROUPID\": " + opp.Group_Id + ",";
                newJson += "\"SERVICE\": \"" + opp.Service + "\",";
                newJson += "\"PROGRAM\": \"" + opp.Program_Name + "\",";
                newJson += "\"INSTALLATION\": \"" + opp.Installation + "\",";
                newJson += "\"CITY\": \"" + opp.City + "\",";
                newJson += "\"STATE\": \"" + opp.State + "\",";
                newJson += "\"ZIP\": " + opp.Zip + ",";
                //newJson += "\"POC\": " + modelJson. + ",";
                //newJson += "\"POCEMAIL\": " + modelJson.id + ",";
                //newJson += "\"POCPHONE\": " + modelJson.id + ",";
                newJson += "\"EMPLOYERPOC\": \"" + opp.Employer_Poc_Name + "\",";
                newJson += "\"EMPLOYERPOCEMAIL\": \"" + opp.Employer_Poc_Email + "\",";
                //newJson += "\"EMPLOYERPOCPHONE\": " + modelJson.id + ",";
                newJson += "\"DATEPROGRAMINITIATED\": \"" + opp.Date_Program_Initiated + "\",";
                newJson += "\"DURATIONOFTRAINING\": \"" + opp.Training_Duration + "\",";
                newJson += "\"SUMMARYDESCRIPTION\": \"" + opp.Summary_Description + "\",";
                newJson += "\"JOBSDESCRIPTION\": \"" + opp.Jobs_Description + "\",";
                newJson += "\"LOCATIONSOFPROSPECTIVEJOBSBYSTATE\": \"" + opp.Locations_Of_Prospective_Jobs_By_State + "\",";
                //newJson += "\"NUMBEROFPERSONNELEMPLOYED\": " + modelJson.id + ",";
                newJson += "\"TARGETMOCs\": \"" + opp.Target_Mocs + "\",";
                //newJson += "\"OTHERELIGIBILITYFACTORS\": " + modelJson.id + ",";
                //newJson += "\"NUMBEROFGRADUATESTODATE\": " + modelJson.id + ",";
                newJson += "\"OTHER\": \"" + opp.Other + "\",";
                newJson += "\"MOUs\": \"" + opp.Mous + "\",";
                newJson += "\"LAT\": " + opp.Lat + ",";
                newJson += "\"LONG\": " + opp.Long + ",";
                newJson += "\"COST\": \"" + opp.Cost + "\",";
                newJson += "\"SALARY\": \"" + opp.Salary + "\",";
                newJson += "\"NATIONWIDE\": " + (opp.Nationwide == true ? 1 : 0);

                newJson += "}";

                // Add comma if this isn't the last object
                if (i < oppsCount - 1)
                {
                    newJson += ",";
                    i++;
                }
            }

            newJson += "]};";*/
            //$("#json-output-container").html(newJson);

            /*var opportunities = _db.Opportunities.Select(o => new
            {
                o.Id,
                o.Group_Id,
                o.Service,
                o.Program_Name,
                o.Installation,
                o.City,
                o.State,
                o.Zip,
                o.Employer_Poc_Name,
                o.Employer_Poc_Email,
                o.Date_Program_Initiated,
                o.Training_Duration,
                o.Summary_Description,
                o.Jobs_Description,
                o.Locations_Of_Prospective_Jobs_By_State,
                o.Target_Mocs,
                o.Other,
                o.Mous,
                o.Lat,
                o.Long,
                o.Cost,
                o.Salary,
                o.Nationwide
            }).ToList();*/



            ////////////////////////////////

            /*List<OpportunityModel> opportunities = _db.Opportunities.Where(x => x.Is_Active == true).ToList<OpportunityModel>();
            string newJson = "var locations = { data: [" + JsonConvert.SerializeObject(opportunities) + "]};";

            newJson = newJson.Replace("\"NATIONWIDE\":true", "\"NATIONWIDE\":1");
            newJson = newJson.Replace("\"NATIONWIDE\":false", "\"NATIONWIDE\":0");

            newJson = newJson.Replace("[[", "[");
            newJson = newJson.Replace("]]", "]");

            newJson = newJson.Replace("\"DELIVERY_METHOD\":\"0\"", "\"DELIVERY_METHOD\":\"In-person\"");
            newJson = newJson.Replace("\"DELIVERY_METHOD\":\"1\"", "\"DELIVERY_METHOD\":\"Online\"");
            newJson = newJson.Replace("\"DELIVERY_METHOD\":\"2\"", "\"DELIVERY_METHOD\":\"Hybrid (In-Person and Online)\"");

            newJson = newJson.Replace("\"MOUs\":true", "\"MOUs\":\"Y\"");
            newJson = newJson.Replace("\"MOUs\":false", "\"MOUs\":\"N\"");

            newJson = newJson.Replace("\"OTHER\"", "\"NUMBEROFPERSONNELEMPLOYED\":\"\",\"NUMBEROFGRADUATESTODATE\":\"\",\"OTHER\"");

            return File(Encoding.UTF8.GetBytes(newJson), "text/plain", "locationData.txt");*/




            ////////////////////////
            ///

            var orgs = _db.Organizations.AsNoTracking().FromCache();
            var progs = _db.Programs.AsNoTracking().FromCache();
            var opps = _db.Opportunities.AsNoTracking().FromCache();

            // Generate the string of JSON
            //string newJson = "var locations = { data: [";
            StringBuilder newJson = new StringBuilder("var locations = { data: [");

            int i = 0;

            int oppCount = opps.ToList().Count;
            

            try
            {
                foreach (var opp in opps)
                {
                    if(opp.Is_Active)
                    {
                        // Add comma if this isn't the last object
                        if (i > 0)
                        {
                            newJson.Append(",");
                        }

                        var prog = progs.SingleOrDefault(x => x.Id == opp.Program_Id);
                        var org = orgs.SingleOrDefault(x => x.Id == opp.Organization_Id);

                        string newProgramDuration = GetProgramDurationForProg(prog);
                        string newDeliveryMethod = "";

                        if(opp.Delivery_Method == "0")
                        {
                            newDeliveryMethod = "In-person";
                        }
                        else if(opp.Delivery_Method == "1")
                        {
                            newDeliveryMethod = "Online";
                        }
                        else if (opp.Delivery_Method == "2")
                        {
                            newDeliveryMethod = "Hybrid (In-Person and Online)";
                        }

                        string newName;

                        if (!org.Name.Equals(prog.Program_Name, StringComparison.OrdinalIgnoreCase))
                        {
                            newName = org.Name + " - " + opp.Program_Name;
                        }
                        else
                        {
                            newName = org.Name;
                        }

                        newJson.Append("{");

                        newJson.Append("\"ID\":" + opp.Id + ",");
                        newJson.Append("\"GROUPID\":" + opp.Group_Id + ",");
                        newJson.Append("\"SERVICE\":\"" + opp.Service + "\",");
                        newJson.Append("\"PROGRAM\":\"" + newName + "\",");
                        newJson.Append("\"INSTALLATION\":\"" + opp.Installation + "\",");
                        newJson.Append("\"CITY\":\"" + opp.City + "\",");
                        newJson.Append("\"STATE\":\"" + opp.State + "\",");
                        newJson.Append("\"ZIP\":\"" + opp.Zip + "\",");
                        newJson.Append("\"POC\":\"" + org.Poc_First_Name + " " + org.Poc_Last_Name + "\",");
                        newJson.Append("\"POCEMAIL\":\"" + org.Poc_Email + "\",");
                        newJson.Append("\"POCPHONE\":\"" + org.Poc_Phone + "\",");
                        newJson.Append("\"EMPLOYERPOC\":\"" + org.Poc_First_Name + " " + org.Poc_Last_Name + "\",");
                        newJson.Append("\"EMPLOYERPOCEMAIL\":\"" + org.Poc_Email + "\",");
                        newJson.Append("\"EMPLOYERPOCPHONE\":\"" + org.Poc_Phone + "\",");
                        newJson.Append("\"DATEPROGRAMINITIATED\":\"" + opp.Date_Program_Initiated.ToString("MM/dd/yyyy") + "\",");
                        newJson.Append("\"DURATIONOFTRAINING\":\"" + newProgramDuration + "\",");
                        newJson.Append("\"SUMMARYDESCRIPTION\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Summary_Description)) + "\",");
                        newJson.Append("\"JOBSDESCRIPTION\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Jobs_Description)) + "\",");
                        newJson.Append("\"LOCATIONSOFPROSPECTIVEJOBSBYSTATE\":\"" + opp.Locations_Of_Prospective_Jobs_By_State + "\",");
                        newJson.Append("\"NUMBEROFPERSONNELEMPLOYED\":\"\",");   // SHOULD WE JUST DROP THIS FIELD? ITS ALWAYS BLANK
                        newJson.Append("\"TARGETMOCs\":\"" + opp.Target_Mocs + "\",");
                        newJson.Append("\"OTHERELIGIBILITYFACTORS\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Other_Eligibility_Factors)) + "\",");
                        newJson.Append("\"NUMBEROFGRADUATESTODATE\":\"\",");   // SHOULD WE JUST DROP THIS FIELD? ITS ALWAYS BLANK
                        newJson.Append("\"OTHER\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Other)) + "\",");
                        newJson.Append("\"MOUs\":\"" + (opp.Mous ? "Y" : "N") + "\",");
                        newJson.Append("\"LAT\":" + opp.Lat + ",");
                        newJson.Append("\"LONG\":" + opp.Long + ",");
                        newJson.Append("\"COST\":\"" + opp.Cost + "\",");
                        newJson.Append("\"SALARY\":\"" + opp.Salary + "\",");
                        newJson.Append("\"NATIONWIDE\":" + (opp.Nationwide ? 1 : 0) + ",");
                        newJson.Append("\"DELIVERY_METHOD\":\"" + newDeliveryMethod + "\"");

                        newJson.Append("}");

                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }

            newJson.Append("]};");

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "application/json", "locationData.txt");
        }

        [HttpGet]
        public IActionResult DownloadNewLocData()  // Generates the list of JSON data that will be used for the live site locations page
        {
            var orgs = _db.Organizations.AsNoTracking();
            var progs = _db.Programs.AsNoTracking();
            var opps = _db.Opportunities.AsNoTracking();

            // Generate the string of JSON
            //string newJson = "var locations = { data: [";
            StringBuilder newJson = new StringBuilder("{  \"locations\":[");

            int i = 0;

            int oppCount = opps.ToList().Count;
            Console.WriteLine("oppCount: " + oppCount);

            try
            {
                foreach (var opp in opps)
                {
                    if (opp.Is_Active)
                    {
                        //Console.WriteLine("Checking opp from program : " + opp.Program_Name + " under organization: " + opp.Organization_Name);
                        var prog = progs.SingleOrDefault(x => x.Id == opp.Program_Id);
                        var org = orgs.SingleOrDefault(x => x.Id == opp.Organization_Id);

                        if(org.Is_Active && prog.Is_Active)
                        {
                            string newProgramDuration = GetProgramDurationForProg(prog);
                            string newDeliveryMethod = "";

                            if (opp.Delivery_Method == "0")
                            {
                                newDeliveryMethod = "In-person";
                            }
                            else if (opp.Delivery_Method == "1")
                            {
                                newDeliveryMethod = "Online";
                            }
                            else if (opp.Delivery_Method == "2")
                            {
                                newDeliveryMethod = "Hybrid (In-Person and Online)";
                            }

                            string newName;

                            if (!org.Name.Equals(prog.Program_Name, StringComparison.OrdinalIgnoreCase))
                            {
                                newName = org.Name + " - " + opp.Program_Name;
                            }
                            else
                            {
                                newName = org.Name;
                            }

                            // Add comma if this isn't the last object
                            if (i > 0)
                            {
                                newJson.Append(",");
                            }

                            newJson.Append("{");

                            newJson.Append("\"COST\":\"" + opp.Cost + "\",");
                            newJson.Append("\"SERVICE\":\"" + opp.Service + "\",");
                            newJson.Append("\"PARENTORGANIZATION\":\"" + org.Parent_Organization_Name + "\",");
                            newJson.Append("\"ORGANIZATION\":\"" + org.Name + "\",");
                            newJson.Append("\"PROGRAM\":\"" + newName + "\",");
                            newJson.Append("\"INSTALLATION\":\"" + opp.Installation + "\",");
                            newJson.Append("\"CITY\":\"" + opp.City + "\",");
                            newJson.Append("\"STATE\":\"" + opp.State + "\",");
                            newJson.Append("\"ZIP\":\"" + opp.Zip + "\",");
                            newJson.Append("\"EMPLOYERPOC\":\"" + opp.Employer_Poc_Name + "\",");
                            newJson.Append("\"EMPLOYERPOCEMAIL\":\"" + opp.Employer_Poc_Email + "\",");
                            newJson.Append("\"DURATIONOFTRAINING\":\"" + newProgramDuration + "\",");
                            newJson.Append("\"SUMMARYDESCRIPTION\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Summary_Description)) + "\",");
                            newJson.Append("\"JOBSDESCRIPTION\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Jobs_Description)) + "\",");
                            newJson.Append("\"LOCATIONSOFPROSPECTIVEJOBSBYSTATE\":\"" + opp.Locations_Of_Prospective_Jobs_By_State + "\",");
                            newJson.Append("\"TARGETMOCs\":\"" + opp.Target_Mocs + "\",");
                            newJson.Append("\"OTHERELIGIBILITYFACTORS\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Other_Eligibility_Factors)) + "\",");
                            newJson.Append("\"OTHER\":\"" + GlobalFunctions.RemoveSpecialCharacters(GlobalFunctions.EscapeCharacters(opp.Other)) + "\",");
                            newJson.Append("\"MOUs\":\"" + (opp.Mous ? "Y" : "N") + "\",");
                            newJson.Append("\"LAT\":" + opp.Lat + ",");
                            newJson.Append("\"LONG\":" + opp.Long + ",");
                            newJson.Append("\"NATIONWIDE\":" + (opp.Nationwide ? 1 : 0) + ",");
                            newJson.Append("\"DELIVERY_METHOD\":\"" + newDeliveryMethod + "\",");
                            newJson.Append("\"JOBFAMILIES\":\"" + opp.Job_Families + "\"");
                            newJson.Append("}");

                            i++;
                        }                        
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }

            newJson.Append("]}");

            Console.WriteLine("newJson: " + newJson);

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "application/json", "locations.json");
        }

        [HttpGet]        
        public IActionResult DownloadDropdownData()  // Generates the list of JSON data that will be used for the live site locations page
        {
            var orgs = _db.Organizations.AsNoTracking();
            var progs = _db.Programs.AsNoTracking();
            var opps = _db.Opportunities.AsNoTracking();
            

            // Generate the string of JSON
            //string newJson = "var locations = { data: [";
            StringBuilder newJson = new StringBuilder("");

            try
            {
                /* PROGRAMS/PROVIDERS */
                // Get Unique Programs
                var uniquePrograms = progs.FromCache().OrderBy(a => a.Program_Name).ToList();
                // Sort Alphebetically
                //uniquePrograms.Sort();

                string uniqueProgramsForExport = "const programDropdown = new Array(";

                int numOutput = 0;

                for (var i = 0; i < uniquePrograms.Count; i++)
                {
                    var progList = progs.FromCache().Where(m => m.Organization_Id == uniquePrograms[i].Organization_Id).ToList();
                    var oppList = opps.FromCache().Where(m => m.Program_Id == uniquePrograms[i].Id).ToList();
                    Console.WriteLine("Program '" + uniquePrograms[i].Program_Name + "' has " + oppList.Count + " Opportunities attached to it");

                    bool soloProgramUnderOrg = true;

                    if(progList.Count > 1)
                    {
                        soloProgramUnderOrg = false;
                    }
                    
                    bool hasActiveOpp = false;

                    for (var j = 0; j < oppList.Count; j++)
                    {
                        if(oppList[j].Is_Active == true)
                        {
                            hasActiveOpp = true;
                        }
                    }

                    //check to see how many programs in each org, if only one then dont output the org name with hyphen

                    if (oppList.Count > 0 && hasActiveOpp == true && uniquePrograms[i].Is_Active == true)
                    {
                        var orgName = uniquePrograms[i].Organization_Name;//.Replace("'", @"\'");
                        var progName = uniquePrograms[i].Program_Name;//.Replace("'", @"\'");

                        if (numOutput == 0)
                        {
                            if (soloProgramUnderOrg == false || uniquePrograms[i].Program_Name != uniquePrograms[i].Organization_Name)
                            {
                                uniqueProgramsForExport += "\"" + orgName + " - " + progName + "\"";
                            }
                            else
                            {
                                uniqueProgramsForExport += "\"" + progName + "\"";
                            }
                        }
                        else
                        {
                            if (soloProgramUnderOrg == false || uniquePrograms[i].Program_Name != uniquePrograms[i].Organization_Name)
                            {
                                uniqueProgramsForExport += ", \"" + orgName + " - " + progName + "\"";
                            }
                            else
                            {
                                uniqueProgramsForExport += ", \"" + progName + "\"";
                            }
                        }
                        numOutput++;
                    }
                }

                uniqueProgramsForExport += ");";

                Console.WriteLine("numOutput: " + numOutput);

                // Add to main body of data to export
                newJson.Append(uniqueProgramsForExport + "\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }

            /* SERVICES */
            newJson.Append("const serviceDropdown = new Array(\"Air Force\", \"Army\", \"Coast Guard\", \"Marine Corps\", \"Navy\");");

            /* DURATION OF TRAINING */
            // Get Unique Durations
            var uniqueDurations = _db.Opportunities.Select(m => m.Training_Duration).Distinct().ToList();
            List<string> durationsList = new List<string>();
            List<string> uniqueDurationsList = new List<string>();

            for (var i = 0; i < uniqueDurations.Count; i++)
            {
                var splits = uniqueDurations[i].Split(",");

                for (var x = 0; x < splits.Length; x++)
                {
                    var item = splits[x].Trim();
                    if (item != "" && item != " ")
                    {
                        durationsList.Add(item);
                    }
                }
            }

            // Get Unique Values from extracted values
            uniqueDurationsList.AddRange(durationsList.Distinct());

            // Sort new list alphabetically using custom comparer that will sort on both alpha and numbers
            //var myComparer = new NumberComparer();
            //uniqueDurationsList.Sort(myComparer);

            // Sort Alphebetically
            //var collator = new Intl.Collator(undefined, { numeric: true, sensitivity: 'base'});
            //uniqueDurations.sort(collator.compare);

            // sort can happen on the JS side since it's easier

            var uniqueDurationsForExport = "const durationDropdown = new Array(";

            for(var i=0; i< uniqueDurationsList.Count; i++)
            {
                if(i==0)
                {
                    uniqueDurationsForExport += "'" + uniqueDurationsList[i].Replace("'", @"\'") + "'";
                }
                else
                {
                    uniqueDurationsForExport += ", '" + uniqueDurationsList[i].Replace("'", @"\'") + "'";
                }
            }

            uniqueDurationsForExport += ");";
            //console.log("uniqueDurationsForExport: " + uniqueDurationsForExport);
            newJson.Append(uniqueDurationsForExport + "\n");

            /* DELIVERY METHOD */
            // Get Unique Programs
            List<string> uniqueDeliveryMethods = _db.Opportunities.Select(m => m.Delivery_Method).Distinct().ToList();
            // Sort Alphebetically
            uniqueDeliveryMethods.Sort();

            var uniqueDeliveryMethodsForExport = "const deliveryDropdown = new Array(";

            for (var i = 0; i < uniqueDeliveryMethods.Count; i++)
            {
                string newDM = "";

                if (uniqueDeliveryMethods[i] == "0")
                {
                    newDM = "In-person";
                }
                else if (uniqueDeliveryMethods[i] == "1")
                {
                    newDM = "Online";
                }
                else if (uniqueDeliveryMethods[i] == "2")
                {
                    newDM = "Hybrid (In-Person and Online)";
                }


                if (i == 0)
                {
                    uniqueDeliveryMethodsForExport += "'" + newDM + "'";
                }
                else
                {
                    uniqueDeliveryMethodsForExport += ", '" + newDM + "'";
                }
            }

            uniqueDeliveryMethodsForExport += ");";

            newJson.Append(uniqueDeliveryMethodsForExport + "\n");




            // Get Unique Locations
            List<string> uniqueLocationItems = _db.Opportunities.Select(m => m.Locations_Of_Prospective_Jobs_By_State).Distinct().ToList();
            // Sort Alphebetically
            uniqueLocationItems.Sort();

            List<string> locationsList = new List<string>();
            List<string> uniqueLocations = new List<string>();

            for (var i = 0; i < uniqueLocationItems.Count; i++)
            {
                if(uniqueLocationItems[i] != null)
                {
                    //Console.WriteLine("uniqueLocationItems[i]: " + uniqueLocationItems[i]);
                    var splits = uniqueLocationItems[i].Split(",");

                    for (var x = 0; x < splits.Length; x++)
                    {
                        var item = splits[x].Trim();
                        if (item != "" && item != " " && item != "All Services")
                        {
                            locationsList.Add(item);
                        }

                    }
                }
            }

            // Get Unique Values from extracted values
            uniqueLocations.AddRange(locationsList.Distinct());

            // Sort Alphebetically
            uniqueLocations.Sort();

            var uniqueLocationsForExport = "const locationDropdown = new Array(";

            for (var i = 0; i < uniqueLocations.Count; i++)
            {
                if (i == 0)
                {
                    uniqueLocationsForExport += "'" + uniqueLocations[i] + "'";
                }
                else
                {
                    uniqueLocationsForExport += ", '" + uniqueLocations[i] + "'";
                }

            }

            uniqueLocationsForExport += ");";

            newJson.Append(uniqueLocationsForExport + "\n");


            // Get Unique Job Families
            List<string> uniqueJobFamilyItems = _db.Opportunities.Select(m => m.Job_Families).Distinct().ToList();
            // Sort Alphebetically
            uniqueJobFamilyItems.Sort();

            List<string> jobFamilyList = new List<string>();
            List<string> uniqueJobFamilies = new List<string>();

            for (var i = 0; i < uniqueJobFamilyItems.Count; i++)
            {
                var splits = uniqueJobFamilyItems[i].Split(";");

                for (var x = 0; x < splits.Length; x++)
                {
                    var item = splits[x].Trim();
                    if (item != "" && item != " " && item != "All Services")
                    {
                        jobFamilyList.Add(item);
                    }

                }
            }

            // Get Unique Values from extracted values
            uniqueJobFamilies.AddRange(jobFamilyList.Distinct());

            // Sort Alphebetically
            uniqueJobFamilies.Sort();

            var uniqueJobFamiliesForExport = "const familyDropdown = new Array(";

            for (var i = 0; i < uniqueJobFamilies.Count; i++)
            {
                if (i == 0)
                {
                    uniqueJobFamiliesForExport += "'" + uniqueJobFamilies[i] + "'";
                }
                else
                {
                    uniqueJobFamiliesForExport += ", '" + uniqueJobFamilies[i] + "'";
                }

            }

            uniqueJobFamiliesForExport += ");";

            newJson.Append(uniqueJobFamiliesForExport + "\n");









            /* COMPANY */
            // Get Unique Companies
            List<string> uniqueParentOrgItems = _db.Opportunities.Select(m => m.Organization_Name).Distinct().ToList();
            List<int> uniqueParentOrgIds = _db.Opportunities.Select(m => m.Organization_Id).Distinct().ToList();
            //console.log("uniqueParentOrgItems.length: " + uniqueParentOrgItems.length);
            List<string> parentOrgList = new List<string>();
            List<string> uniqueParentOrgs = new List<string>();

            // Sort Alphebetically
            uniqueParentOrgItems.Sort();

            for (var i = 0; i < uniqueParentOrgItems.Count; i++)
            {
                //console.log("uniqueParentOrgItems[i].ORGANIZATION: " + uniqueParentOrgItems[i].PARENTORGANIZATION);
                parentOrgList.Add(uniqueParentOrgItems[i]);
            }

            // Get Unique Values from extracted values
            uniqueParentOrgs.AddRange(parentOrgList.Distinct());

            // Sort Alphebetically
            uniqueParentOrgs.Sort();

            var uniqueParentOrgsForExport = "const parentOrgDropdown = new Array(";
        
            for (var i = 0; i < uniqueParentOrgs.Count; i++)
            {
                if (i == 0)
                {
                    uniqueParentOrgsForExport += "'" + uniqueParentOrgs[i].Replace("'", @"\'") + "'";
                }
                else
                {
                    uniqueParentOrgsForExport += ", '" + uniqueParentOrgs[i].Replace("'", @"\'") + "'";
                }

            }

            uniqueParentOrgsForExport += ");";
            //console.log("uniqueParentOrgsForExport: " + uniqueParentOrgsForExport);
            newJson.Append(uniqueParentOrgsForExport + "\n");


            var relatedOrgsForExport = "var relatedOrgs = { data: [";

            // Find all Orgs under each parent org
            for (var i = 0; i < uniqueParentOrgs.Count; i++)
            {
                //var orgItems = _.where(data, { PARENTORGANIZATION: uniqueParentOrgs[i]});
                List<string> orgItems = _db.Organizations.Where(m => m.Parent_Organization_Name == uniqueParentOrgs[i]).Select(m => m.Name).ToList();

                // Get Unique Values from extracted values
                List<string> uniqueOrgItems = new List<string>();
                uniqueOrgItems.AddRange(orgItems.Distinct());
                // Sort Alphebetically
                uniqueOrgItems.Sort();

                // If multiple orgs under parent org, use opt group
                if (uniqueOrgItems.Count > 1)
                {
                    if (i > 0)
                    {
                        relatedOrgsForExport += ",";
                    }
                    relatedOrgsForExport += "{ 'parentOrg': '" + uniqueParentOrgs[i].Replace("'", @"\'") + "','orgs':[";

                    for (var j = 0; j < uniqueOrgItems.Count; j++)
                    {
                        if (j > 0)
                        {
                            relatedOrgsForExport += ",";
                        }
                        relatedOrgsForExport += "'" + uniqueOrgItems[j].Replace("'", @"\'") + "'";
                    }

                    relatedOrgsForExport += "]}";
                }
                else
                {
                    if (i > 0)
                    {
                        relatedOrgsForExport += ",";
                    }
                    relatedOrgsForExport += "{ 'parentOrg': '" + uniqueParentOrgs[i].Replace("'", @"\'") + "','orgs':[";
                    for (var j = 0; j < uniqueOrgItems.Count; j++)
                    {
                        if (j > 0)
                        {
                            relatedOrgsForExport += ",";
                        }
                        relatedOrgsForExport += "'" + uniqueOrgItems[j].Replace("'", @"\'") + "'";
                    }
                    relatedOrgsForExport += "]}";
                }
            }

            relatedOrgsForExport += "]}";
    
            newJson.Append(relatedOrgsForExport + "\n");


            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "application/json", "dropdown-data.js");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProgramServiceValues()
        {
            // Find programs missing services values
            var flaggedProgs = _db.Programs.Where(m => m.Services_Supported == "" || m.Services_Supported == null).ToList();

            Console.WriteLine("flaggedProgs.Count: " + flaggedProgs.Count); // Should be 1058

            int numWithServicesAlready = 0;
            int numWithNoService = 0;

            foreach (ProgramModel prog in flaggedProgs)
            {
                // Try to find a value for this program in the programsservice table
                var progServices = _db.ProgramService.Where(m => m.Program_Id == prog.Id).ToList();

                // If we have services defined, pull the optimized string to the program
                if(progServices.Count > 0)
                {
                    string optServices = GetServiceListForProg(prog);
                    prog.Services_Supported = optServices;
                    Console.WriteLine("Service defined for program: " + prog.Program_Name + ", setting optimized value to " + optServices);
                    _db.Programs.Update(prog);
                    numWithServicesAlready++;
                }
                else   // Create the appropriate service entries for this program
                {
                    ProgramService newService = new ProgramService
                    {
                        Program_Id = prog.Id,
                        Service_Id = 1
                    };

                    _db.ProgramService.Add(newService);
                    
                    string optServices = "All Services";
                    prog.Services_Supported = optServices;
                    Console.WriteLine("No services defined for program: " + prog.Program_Name + ", setting optimized value to All Services");
                    _db.Programs.Update(prog);
                    numWithNoService++;
                }
            }

            var result = await _db.SaveChangesAsync();

            if (result > 0)
            {
                Console.WriteLine("Update Successful: " + result + " changes made");
            }
            else
            {
                Console.WriteLine("Update FAILED");
            }

            Console.WriteLine("numWithServicesAlready: " + numWithServicesAlready);
            Console.WriteLine("numWithNoService: " + numWithNoService);

            return View("GenerateUpdateData");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateOpportunityServiceValues()
        {
            // Find programs missing services values
            var flaggedOpps = _db.Opportunities.Where(m => m.Service == "" || m.Service == null).ToList();

            Console.WriteLine("flaggedOpps.Count: " + flaggedOpps.Count);

            int changes = 0;

            foreach (OpportunityModel opp in flaggedOpps)
            {
                // Try to find a value for this program in the programsservice table
                var progServices = _db.ProgramService.Where(m => m.Program_Id == opp.Program_Id).ToList();
                ProgramModel prog = _db.Programs.SingleOrDefault(m => m.Id == opp.Program_Id);

                // If we have services defined, pull the optimized string to the program
                if (progServices.Count > 0)
                {
                    string optServices = prog.Services_Supported;
                    opp.Service = optServices;
                    Console.WriteLine("Service defined for program: " + prog.Program_Name + ", setting opp (" + opp.Id + ") optimized value to " + optServices);
                    _db.Opportunities.Update(opp);
                    changes++;
                }
                /*else   // Create the appropriate service entries for this program
                {
                    ProgramService newService = new ProgramService
                    {
                        Program_Id = prog.Id,
                        Service_Id = 1
                    };

                    _db.ProgramService.Add(newService);

                    string optServices = "All Services";
                    prog.Services_Supported = optServices;
                    Console.WriteLine("No services defined for program: " + prog.Program_Name + ", setting optimized value to All Services");
                    _db.Programs.Update(prog);
                    numWithNoService++;
                }*/
            }

            var result = await _db.SaveChangesAsync();

            if (result > 0)
            {
                Console.WriteLine("Update Successful: " + result + " changes made");
            }
            else
            {
                Console.WriteLine("Update FAILED");
            }

            Console.WriteLine("changes: " + changes);
            //Console.WriteLine("numWithNoService: " + numWithNoService);

            return View("GenerateUpdateData");
        }

        private string GetServiceListForProg(ProgramModel prog)
        {
            string services = "";

            var ps = _db.ProgramService.Where(x => x.Program_Id == prog.Id).ToList();

            Console.WriteLine("==ps.Count for " + prog.Program_Name + " = " + ps.Count);

            int count = ps.Count;
            int i = 0;

            foreach (var s in ps)
            {
                Console.WriteLine("s: " + s.Service_Id);
                Service service = _db.Services.FirstOrDefault(x => x.Id == s.Service_Id);

                if (count == 1)
                {
                    if (service != null)
                    {
                        services += service.Name;
                    }
                }
                else if (count == 2)
                {
                    if (i == 0)
                    {
                        if (service != null)
                        {
                            services += service.Name;
                        }
                    }
                    else
                    {
                        if (service != null)
                        {
                            //services += " and " + service.Name;
                            services += ", " + service.Name;
                        }
                    }
                }
                else if (count >= 3)
                {
                    if (i == 0)
                    {
                        if (service != null)
                        {
                            services += service.Name;
                        }
                    }
                    /*else if(i < count - 1)
                    {
                        if (service != null)
                        {
                            services += ", and " + service.Name;
                        }
                    }*/
                    else
                    {
                        if (service != null)
                        {
                            services += ", " + service.Name;
                        }
                    }
                }

                /*if (i == 0)
                {
                    if (service != null)
                    {
                        services += service.Name;
                    }
                }
                else if (i < count - 1)
                {
                    if(count > 2)
                    {
                        if (service != null)
                        {
                            services += ", " + service.Name;
                        }
                    }
                    else if(count > 1)
                    {
                        if (service != null)
                        {
                            services += service.Name;
                        }
                    }
                }
                else
                {
                    if (count > 2)
                    {
                        if (service != null)
                        {
                            services += ", and " + service.Name;
                        }
                    }
                    else if(count > 1)
                    {
                        if (service != null)
                        {
                            services += " and " + service.Name;
                        }
                    }
                }*/

                i++;
            }

            Console.WriteLine("GetServiceListForProg returning " + services);

            return services;
        }

        [HttpGet]
        public IActionResult DownloadLocationsCSV()
        {
            var opps = _db.Opportunities;

            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("ID|GROUPID|SERVICE|PROGRAM|INSTALLATION|CITY|STATE|ZIP|EMPLOYERPOC|EMPLOYERPOCEMAIL|DATEPROGRAMINITIATED|DURATIONOFTRAINING|SUMMARYDESCRIPTION|JOBSDESCRIPTION|LOCATIONSOFPROSPECTIVEJOBSBYSTATE|TARGETMOCs|OTHER|MOUs|LAT|LONG|COST|SALARY|NATIONWIDE");

                foreach (OpportunityModel opp in opps)
                {
                    //var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);
                    //string urlToDisplay = org != null ? org.Organization_Url : "";

                    stringBuilder.AppendLine($"" + $"{ opp.Id}|{ opp.Group_Id}|{ opp.Service}|{ opp.Program_Name}|{ opp.Installation}|{ opp.City}|{ opp.State}|{ opp.Zip}|{ opp.Employer_Poc_Name}|{ opp.Employer_Poc_Email}|{ opp.Date_Program_Initiated}|{ opp.Training_Duration}|{ opp.Summary_Description}|{ opp.Jobs_Description}|{ opp.Locations_Of_Prospective_Jobs_By_State}|{ opp.Target_Mocs}|{ opp.Other}|{ opp.Mous}|{ opp.Lat}|{ opp.Long}|{ opp.Cost}|{ opp.Salary}|{ opp.Nationwide}");
                }

                return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", "locs-" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
            }
            catch
            {
                return Error();
            }
        }

        [HttpGet]
        public IActionResult DownloadSpouseData()  // Generates the list of JSON data that will be used for the live site organizations page
        {
            /*var progs = _db.Programs.AsNoTracking().Where(x => x.For_Spouses == true).OrderBy(x => x.Program_Name);
            var orgs = _db.Organizations.AsNoTracking();

            List<int> spouseOrgs = new List<int>();

            // Generate the string of JSON
            //string newJson = "var spouses = { data:
            StringBuilder newJson = new StringBuilder("var spouses = { data: [");

            int i = 0;
            //int progCount = progs.ToList().Count;   // 690

            //Console.WriteLine("About to export spouse data in foreach loop");
            foreach (ProgramModel prog in progs)
            {
                //Console.WriteLine("-=-=-=-=checking program: " + prog.Program_Name + " for spouse export, i = " + i);
                // We need to check this differently than the others since we don't know the state of every programs spouse offerings...
                //if(prog.For_Spouses)
                //{
                bool wasFound = false;

                    // Make sure we arent duplicating orgs/progs in the data
                    foreach(int id in spouseOrgs)
                    {
                        if(id == prog.Organization_Id)
                        {
                            //Console.WriteLine("==Skipping program: " + prog.Program_Name + " for spouse export, similar prog already found");
                            wasFound = true;
                        }
                    }

                    if(!wasFound)
                    {
                        if(prog.Is_Active)
                        {
                            if (i != 0)
                            {
                                newJson.Append(",");
                            }

                            newJson.Append("{");

                            var org = _db.Organizations.AsNoTracking().SingleOrDefault(x => x.Id == prog.Organization_Id);

                            string newDeliveryMethod = GetDeliveryMethodForProg(prog);

                            newJson.Append("\"PROGRAM\": \"" + org.Name + "\",");
                            newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                            newJson.Append("\"NATIONWIDE\": " + (prog.Nationwide == true ? 1 : 0) + ",");
                            newJson.Append("\"ONLINE\": " + (prog.Online == true ? 1 : 0) + ",");
                            newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                            newJson.Append("\"STATES\": \"" + prog.States_Of_Program_Delivery + "\"");

                            newJson.Append("}");

                            i++;

                            spouseOrgs.Add(prog.Organization_Id);
                        }                        
                    }
                //}
            }*/

            /*foreach(OrganizationModel org in orgs)
            {
                bool isForSpouses = false;
                foreach(ProgramModel prog in progs)
                {
                    if(prog.For_Spouses == true)
                    {
                        isForSpouses = true;
                    }
                }

                if(isForSpouses)
                {
                    newJson += "{";

                    //var org = _db.Organizations.AsNoTracking().SingleOrDefault(x => x.Id == prog.Organization_Id);

                    //string urlToDisplay = org != null ? org.Organization_Url : "";


                    //int nat = prog.Nationwide == true ? 1 : 0;
                    //int online = prog.Online == true ? 1 : 0;

                    string newDeliveryMethod = GetDeliveryMethodForProg(prog);

                    newJson += "\"PROGRAM\": \"" + prog.Program_Name + "\",";
                    newJson += "\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",";
                    newJson += "\"NATIONWIDE\": " + (prog.Nationwide == true ? 1 : 0) + ",";
                    newJson += "\"ONLINE\": " + (prog.Online == true ? 1 : 0) + ",";
                    newJson += "\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",";
                    newJson += "\"STATES\": \"" + prog.States_Of_Program_Delivery + "\"";

                    newJson += "}";

                    if (i < progCount - 1)
                    {
                        newJson += ",";
                        i++;
                    }
                }
            }*/

            //newJson.Append("]};");

            //$("#json-output-container").html(newJson);
            //return File(Encoding.UTF8.GetBytes(newJson.ToString()), "text/plain", "spousesData.txt");



            var orgs = _db.Organizations.AsNoTracking();
            var progs = _db.Programs.AsNoTracking();
            var opps = _db.Opportunities.AsNoTracking();

            Console.WriteLine("=====================================");
            Console.WriteLine("Generating Spouse Data");

            // Generate the string of JSON
            //string newJson = "[";
            StringBuilder newJson = new StringBuilder("var spouses = { data: [");

            int i = 0;

            List<int> spouseOrgs = new List<int>();

            try
            {
                foreach (ProgramModel prog in progs)
                {
                    if(prog.Is_Active)
                    {
                        if (prog.For_Spouses)
                        {
                            var org = orgs.FromCache().SingleOrDefault(x => x.Id == prog.Organization_Id);

                            if(org.Is_Active)
                            {
                                // Check to make sure we aren't duplicating orgs on the spouse list
                                if (!DoesOrgExistInSpouses(org.Id, spouseOrgs))
                                {
                                    string newName = "";

                                    if (org.Name.Equals(prog.Program_Name))
                                    {
                                        newName = org.Name;
                                    }
                                    else
                                    {
                                        newName = org.Name + " - " + prog.Program_Name;
                                    }


                                    string newProgramDuration = GetProgramDurationForProg(prog);

                                    if (i != 0)
                                    {
                                        newJson.Append(",");
                                    }

                                    newJson.Append("{");

                                    string newDeliveryMethod = GetDeliveryMethodForProg(prog);
                                    Console.WriteLine("newDeliverMethod: " + newDeliveryMethod);

                                    newJson.Append("\"PROGRAM\": \"" + newName + "\",");
                                    newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                                    newJson.Append("\"NATIONWIDE\":" + (prog.Nationwide ? 1 : 0) + ",");
                                    newJson.Append("\"ONLINE\":" + (prog.Online ? 1 : 0) + ",");
                                    newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                                    newJson.Append("\"STATES\": \"" + prog.States_Of_Program_Delivery + "\"");

                                    i++;

                                    newJson.Append("}");

                                    spouseOrgs.Add(org.Id);
                                    //}
                                }
                            }                            
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("e: " + e.Message);
            }


            newJson.Append("]};");

            Console.WriteLine("=====================================");

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "text/plain", "spousesData.txt");
        }

        public bool DoesOrgExistInSpouses(int id, List<int> spouseOrgs)
        {
            bool exists = false;

            foreach(int i in spouseOrgs)
            {
                if(i == id)
                {
                    exists = true;
                }
            }

            return exists;
        }

        [HttpGet]
        public IActionResult DownloadSpousesCSV()
        {
            var progs = _db.Programs;

            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("PROGRAM|URL|NATIONWIDE|ONLINE|DELIVERY_METHOD|STATES");

                foreach (ProgramModel prog in progs)
                {
                    var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);

                    string urlToDisplay = org != null ? org.Organization_Url : "";

                    stringBuilder.AppendLine($"" + $"{ prog.Program_Name}|{ urlToDisplay}|{ prog.Nationwide}|{ prog.Online}|{ prog.Delivery_Method}|{ prog.States_Of_Program_Delivery}");
                }

                return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", "spouses-" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
            }
            catch
            {
                return Error();
            }
        }

        [HttpGet]
        public IActionResult IngestMOUs()  // Ingests the data from one DB table, formats/validates it, and then replaces the current data in the organizations table with it
        {
            return View();
        }

        [HttpGet]
        public IActionResult IngestOrganizations()  // Ingests the data from one DB table, formats/validates it, and then replaces the current data in the organizations table with it
        {
            return View();
        }

        [HttpGet]
        public IActionResult IngestPrograms()  // Ingests the data from one DB table, formats/validates it, and then replaces the current data in the progras table with it
        {
            return View();
        }

        [HttpGet]
        public IActionResult IngestOpportunities()  // Ingests the data from one DB table, formats/validates it, and then replaces the current data in the opportunities table with it
        {
            return View();
        }

        [HttpGet]
        public IActionResult Utilities()
        {
            return View();
        }

        public IActionResult UpdateMOUDates()
        {
            var orgs = _db.Organizations;
            var progs = _db.Programs;
            var opps = _db.Opportunities;


            // Update Programs
            foreach (var p in progs)
            {
                var org = orgs.FirstOrDefault(m => m.Id == p.Organization_Id);
                var mou = _db.Mous.FirstOrDefault(m => m.Id == org.Mou_Id);


                Console.WriteLine("Should update program... exp date: " + mou.Expiration_Date + " and create date: " + mou.Creation_Date);
                p.Mou_Expiration_Date = mou.Expiration_Date;
                p.Mou_Creation_Date = mou.Creation_Date;
            }

            _db.SaveChanges();

            // Update Opportunities
            foreach (var o in opps)
            {
                ProgramModel prog = progs.FirstOrDefault(m => m.Id == o.Program_Id);

                Console.WriteLine("Shoud update opportunity... exp date: " + prog.Mou_Expiration_Date);
                o.Mou_Expiration_Date = prog.Mou_Expiration_Date;
            }

            _db.SaveChanges();

            return View();
        }


        public IActionResult UpdateStatesOfDelivery()
        {
            var orgs = _db.Organizations;
            var progs = _db.Programs;
            var opps = _db.Opportunities;

            // Update Programs
            foreach (var p in progs)
            {
                var relatedOpps = opps.FromCache().Where(e => e.Program_Id == p.Id).ToList();

                new UpdateStatesOfProgramDeliveryCommand().Execute(p, relatedOpps, _db);
            }

            _db.SaveChanges();

            // Update Organizations
            foreach(var o in orgs)
            {
                new UpdateOrgStatesOfProgramDeliveryCommand().Execute(o, _db);
            }

            _db.SaveChanges();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IngestMOUs(string source)  // Ingests the data from one DB table, formats/validates it, and then replaces the current data in the organizations table with it
        {
            // source is the source CSV file
            //MouIngestTest.csv

            string newSource = "OrgIngestTest14";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = "|",
                MissingFieldFound = null
            };

            string logMessage = "";

            // Remove existing records from Org Table
            //DeleteAllMOUs();
            DeleteAllMOUs();

            using (var reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + newSource + ".csv"))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                int i = 0;

                while (csv.Read())
                {
                    //var record = csv.GetRecord<OrganizationModel>();
                    // Do something with the record.

                    /*
                        public DateTime Creation_Date { get; set; }
                        public DateTime Expiration_Date { get; set; }
                        public string Url { get; set; }
                        public string Service { get; set; }
                        public bool Is_OSD { get; set; }
                        public string Organization_Name { get; set; }   // Update database on ingest
                        public int Legacy_Provider_Id { get; set; }   // Update database on ingest

                        ---------------------------

                        Provider Unique ID X
                        Parent Organization Name X
                        Date Authorized X
                        MOU Expiration Date X
                        MOU Packet Link X
                        Service X
                        Is_OSD X
                    */

                    int tempId = int.Parse(csv.GetField("MOU_ID"));

                    //var mou = _db.Mous.SingleOrDefault(x => x.Legacy_MOU_Id == tempId));

                    Console.Write("Parent Organization Name: " + csv.GetField("Parent Organization Name").ToString());

                    if (csv.GetField("MOU_Parent").ToString() == "TRUE")
                    {
                        // Remove (Historical Import) strings from dates
                        string newCreatedDate = csv.GetField("Date Authorized").ToString();
                        newCreatedDate = newCreatedDate.Replace("Unknown", "");

                        string newExpirationDate = csv.GetField("MOU Expiration Date").ToString();
                        newExpirationDate = newExpirationDate.Replace("Unknown", "");

                        DateTime output1;
                        var isValidDateTime1 = DateTime.TryParse(newCreatedDate, out output1);

                        DateTime output2;
                        var isValidDateTime2 = DateTime.TryParse(newExpirationDate, out output2);

                        bool isOSD = true;


                        //Console.WriteLine("-=-=-=-=-=-=newCreatedDate: " + newCreatedDate);
                        //Console.WriteLine("-=-=-=-=-=-=newUpdatedDate: " + newUpdatedDate);

                        //TODO: Convert to mapping
                        var newMou = new MouModel()
                        {
                            Creation_Date = output1,
                            Expiration_Date = output2,
                            Url = csv.GetField("MOU Packet Link").ToString(),
                            Service = csv.GetField("Services Supported").ToString(),
                            Is_OSD = isOSD,
                            Organization_Name = csv.GetField("Parent Organization Name").ToString(),
                            Legacy_Provider_Id = int.Parse(csv.GetField("Provider Unique ID")),
                            //Legacy_Mou_Id = int.Parse(csv.GetField("MOU_ID"))
                        };

                        _db.Mous.Add(newMou);
                        _db.SaveChanges();

                        logMessage += "\n===================== Record #" + i + " ====================";
                        logMessage += "\n====================================================";
                        logMessage += "\nOrganization_Name " + newMou.Organization_Name;
                        logMessage += "\nCreation_Date " + newMou.Creation_Date;
                        logMessage += "\nExpiration_Date " + newMou.Expiration_Date;
                        logMessage += "\nUrl " + newMou.Url;
                        logMessage += "\nService " + newMou.Service;
                        logMessage += "\nIs_OSD " + newMou.Is_OSD;
                        logMessage += "\nLegacy_Provider_Id " + newMou.Legacy_Provider_Id;
                        logMessage += "\n====================================================";
                    }

                    i++;
                }

                // Write the log file
                string strFileName = "SB-Mou-Ingest-Log.txt";

                try
                {
                    FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), strFileName), FileMode.Create, FileAccess.Write);
                    StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                    objStreamWriter.WriteLine(logMessage);
                    objStreamWriter.Close();
                    objFilestream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IngestOrganizations(string source)  // Ingests the data from one DB table, formats/validates it, and then replaces the current data in the organizations table with it
        {
            // source is the source CSV file
            //OrgIngestTest.csv

            string newSource = "OrgIngestTest14";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = "|",
                MissingFieldFound = null
            };

            string logMessage = "";

            // Remove existing records from Org Table
            //DeleteAllMOUs();
            DeleteAllOrganizations();
            
            using (var reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + newSource + ".csv"))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                int i = 0;

                while (csv.Read())
                {
                    //var record = csv.GetRecord<OrganizationModel>();
                    // Do something with the record.

                    // Remove (Historical Import) strings from dates
                    string newCreatedDate = csv.GetField("Date Added").ToString();
                    newCreatedDate = newCreatedDate.Replace(" (Historical Import)", "");

                    string newUpdatedDate = csv.GetField("Last Updated").ToString();
                    newUpdatedDate = newUpdatedDate.Replace(" (Historical Import)", "");

                    bool newIsActive = false;
                    string newDeactivatedDate = csv.GetField("Program Status").ToString();
                    if(newDeactivatedDate.Contains("Closed"))
                    {
                        if(!newDeactivatedDate.Contains("one-off") && !newDeactivatedDate.Contains("Duplicate Provider"))
                        {
                            if(newDeactivatedDate.Contains("(") && newDeactivatedDate.Contains(")"))
                            {
                                int pFrom = newDeactivatedDate.IndexOf("(") + 1;
                                int pTo = newDeactivatedDate.LastIndexOf(")");

                                newDeactivatedDate = newDeactivatedDate.Substring(pFrom, pTo - pFrom);
                            }
                            else
                            {
                                newDeactivatedDate = "";
                            }
                        }
                        else
                        {
                            newDeactivatedDate = "";
                        }
                        newIsActive = false;
                    }
                    else
                    {
                        newDeactivatedDate = "";
                        newIsActive = true;
                    }

                    Console.WriteLine("int.Parse(csv.GetField('Provider Unique ID')): " + int.Parse(csv.GetField("Provider Unique ID")));

                    int newMOUId;

                    /*
                     * THIS NEEDS TO REFERENCE A NEW FIELD UNIQUE MOU ID
                     * new sheet will have unique mou id -- legacy, and a parent column
                     * DONT make MOU objects for items marked as false in parent column
                     * set is osd to true, will set to false manually later
                     * 
                     * */

                    // Check if record with that ID exists
                    if (_db.Mous.Any(e => e.Organization_Name == csv.GetField("Parent Organization Name")))
                    {
                        newMOUId = _db.Mous.FirstOrDefault(e => e.Organization_Name == csv.GetField("Parent Organization Name")).Id;
                    }
                    else
                    {
                        newMOUId = -1;
                    }

                    int newOrgType = 0;
                    //Profit, Federal, State, County/Municipality/City, Non or Not for Profit
                    /*
                        <option value="0">Profit</option>
                        <option value="1">Non or Not for Profit</option>
                        <option value="2">County, Municipal, City</option>
                        <option value="3">State</option>
                        <option value="4">Federal</option>
                     */
                    if (csv.GetField("Type") != "")
                    {
                        if(csv.GetField("Type") == "Profit")
                        {
                            newOrgType = 0;
                        }
                        else if (csv.GetField("Type") == "Non or Not for Profit")
                        {
                            newOrgType = 1;
                        }
                        else if (csv.GetField("Type") == "County/Municipality/City")
                        {
                            newOrgType = 2;
                        }
                        else if (csv.GetField("Type") == "State")
                        {
                            newOrgType = 3;
                        }
                        else if (csv.GetField("Type") == "Federal")
                        {
                            newOrgType = 4;
                        }
                    }

                    Console.WriteLine("-=-=-=-=-=-=newCreatedDate: " + newCreatedDate);
                    Console.WriteLine("-=-=-=-=-=-=newUpdatedDate: " + newUpdatedDate);

                    OrganizationModel newOrg = new OrganizationModel
                    {
                        Name = csv.GetField("Organization Name"),
                        Parent_Organization_Name = csv.GetField("Parent Organization Name"),
                        //Name = csv.GetField("Parent Organization Name"),
                        Poc_First_Name = csv.GetField("Admin POC First Name 1"),
                        Poc_Last_Name = csv.GetField("Admin POC Last Name 1"),
                        Poc_Email = csv.GetField("Admin POC Email Address 1"),
                        Poc_Phone = csv.GetField("Admin POC Phone Number 1"),
                        Date_Deactivated = newDeactivatedDate != "" ? DateTime.Parse(newDeactivatedDate):new DateTime(),
                        Is_Active = newIsActive,
                        Date_Created = DateTime.Parse(newCreatedDate),
                        Date_Updated = DateTime.Parse(newUpdatedDate),
                        Created_By = "Ingest", // Set this so no errors occur
                        Updated_By = "Ingest", // Set this so no errors occur
                        Organization_Url = csv.GetField("URL"),
                        Organization_Type = newOrgType,//0, // Set this so no errors occur
                        Notes = csv.GetField("Notes"),
                        Legacy_Provider_Id = int.Parse(csv.GetField("Provider Unique ID")),
                        Mou_Id = newMOUId  
                    };

                    _db.Organizations.Add(newOrg);
                    _db.SaveChanges();

                    logMessage += "\n===================== Record #" + i + " ====================";
                    logMessage += "\n====================================================";
                    logMessage += "\nName " + newOrg.Name;
                    logMessage += "\nPoc_First_Name " + newOrg.Poc_First_Name;
                    logMessage += "\nPoc_Last_Name " + newOrg.Poc_Last_Name;
                    logMessage += "\nPoc_Email " + newOrg.Poc_Email;
                    logMessage += "\nPoc_Phone " + newOrg.Poc_Phone;
                    logMessage += "\nDate_Created " + newOrg.Date_Created;
                    logMessage += "\nDate_Updated " + newOrg.Date_Updated;
                    logMessage += "\nOrganization_Url " + newOrg.Organization_Url;
                    logMessage += "\nNotes " + newOrg.Notes;
                    logMessage += "\nLegacy_Provider_Id " + newOrg.Legacy_Provider_Id;
                    logMessage += "\n====================================================";

                    i++;
                }

                // Write the log file
                string strFileName = "SB-Org-Ingest-Log.txt";

                try
                {
                    FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), strFileName), FileMode.Create, FileAccess.Write);
                    StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                    objStreamWriter.WriteLine(logMessage);
                    objStreamWriter.Close();
                    objFilestream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }

            return View();
        }

        public async Task<IActionResult> AddJobFamilies()
        {
            bool foundJF = false;
            var programs = _db.Programs; // define query
            foreach (var newProg in programs) // query executed and data obtained from database
            {                
                    if(newProg.Job_Family.Contains("Architecture and Engineering"))
                        {
                            int newId = 1;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                            foundJF = true;
                        }
                if (newProg.Job_Family.Contains("Arts, Design, Entertainment, Sports, and Media"))
                        {
                            int newId = 2;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Building and Grounds Cleaning and Maintenance"))
                        {
                            int newId = 3;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Business and Financial Operations"))
                        {
                            int newId = 4;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Community and Social Service"))
                        {
                            int newId = 5;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Computer and Mathematical"))
                        {
                            int newId = 6;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Construction and Extraction"))
                        {
                            int newId = 7;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Education, Training, and Library"))
                        {
                            int newId = 8;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Farming, Fishing, and Forestry"))
                        {
                            int newId = 9;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Food Preparation and Serving Related"))
                        {
                            int newId = 10;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Healthcare Practitioners and Technical"))
                        {
                            int newId = 11;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Healthcare Support"))
                        {
                            int newId = 12;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Installation, Maintenance, and Repair"))
                        {
                            int newId = 13;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Legal"))
                        {
                            int newId = 14;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Life, Physical, and Social Science"))
                        {
                            int newId = 15;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Management"))
                        {
                            int newId = 16;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Military Specific"))
                        {
                            int newId = 17;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Protective Service"))
                        {
                            int newId = 18;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Sales and Related"))
                        {
                            int newId = 19;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Transportation and Material Moving"))
                        {
                            int newId = 20;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Other"))
                        {
                            int newId = 21;
                            ProgramJobFamily j = new ProgramJobFamily
                            {
                                Job_Family_Id = newId,
                                Program_Id = newProg.Id
                            };
                            _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Office and Administrative Support"))
                {
                    int newId = 22;
                    ProgramJobFamily j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Personal Care and Service"))
                {
                    int newId = 23;
                    ProgramJobFamily j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.Job_Family.Contains("Production"))
                {
                    int newId = 24;
                    ProgramJobFamily j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }


                if (foundJF == false)
                {
                    Console.WriteLine("\nCouldn't find Job Family ID in known IDs for: " + newProg.Job_Family);
                    //jfMsg += "\nCouldn't find Job Family ID in known IDs for: " + jf;
                    int newId = 21;
                    ProgramJobFamily j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                }
                
            }
            var result = await _db.SaveChangesAsync();

            if(result > 0)
            {
                return View("IngestPrograms");
            }
            else
            {
                return View("IngestPrograms");
            }

            //return View("IngestPrograms");
        }

        

        [HttpPost]
        public async Task<IActionResult> IngestPrograms(string source)  // Ingests the data from one DB table, formats/validates it, and then replaces the current data in the programs table with it
        {
            // source is the source CSV file
            //OrgIngestTest.csv

            string newSource = "OrgIngestTest14";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = "|",
                MissingFieldFound = null
            };

            string logMessage = "";
            string jfMsg = "";

            // Remove existing groups from Groups table
            DeleteAllOpportunityGroups();

            // Remove existing records from Opp Table
            DeleteAllOpportunities();

            // Remove existing records from Prog Table
            DeleteAllPrograms();

            List<ProgramLookup> dict = new List<ProgramLookup>();

            using (var reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + "ProgIngestTest14" + ".csv"))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    string progId = csv.GetField("Program Unique ID");
                    string durationFromFile = csv.GetField("Program Duration");

                    ProgramLookup item = new ProgramLookup();

                    bool active = false;
                    if(csv.GetField("Program Status").Contains("Active"))
                    {
                        active = true;
                    }

                    item.Is_Active = active;
                    item.Program_Name = csv.GetField("Program Name");
                    item.Program_Id = int.Parse(csv.GetField("Program Unique ID"));
                    item.Organization_Id = int.Parse(csv.GetField("Provider Unique ID"));
                    item.Program_Duration = durationFromFile;

                    dict.Add(item);

                    // Check for multiple programs in the id field
                    /*if (progId.Contains(","))
                    {
                        List<int> ids = progId.Replace(" ", "").Split(',').Select(int.Parse).ToList();
                        //List<string> durations = durationFromFile.Trim().Split(", ").ToList();

                        //Console.WriteLine("Mutilple IDs found for an org: " + ids.ToString());

                        //foreach(string d in durations)
                        //{
                           // Console.WriteLine("multiple duration found of: " + d);
                        //}

                        int i = 0;+

                        foreach (int id in ids)
                        {
                            ProgramLookup item = new ProgramLookup();

                            item.Program_Name = csv.GetField("Program Name");
                            item.Program_Id = id;
                            item.Organization_Id = int.Parse(csv.GetField("Provider Unique ID"));
                            //item.Program_Duration = durations[i] != null ? durations[i] : " ";

                            dict.Add(item);

                            i++;
                        }
                    }
                    else // Only one ID Likely, make one item
                    {
                        ProgramLookup item = new ProgramLookup();

                        item.Program_Name = csv.GetField("Program Name");
                        item.Program_Id = int.Parse(csv.GetField("Program Unique ID"));
                        item.Organization_Id = int.Parse(csv.GetField("Provider Unique ID"));
                        //item.Program_Duration = durationFromFile;

                        dict.Add(item);
                    }*/
                }
            }

            using (var reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + newSource + ".csv"))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                int i = 0;

                while (csv.Read())
                {
                    //var record = csv.GetRecord<ProgramModel>();
                    // Do something with the record.

                    // Remove un-parsable strings from dates, replace with current date
                    string newDateAuthorized = csv.GetField("Date Authorized").ToString();
                    newDateAuthorized = newDateAuthorized.Replace(" (Historical Import)", "");
                    if(newDateAuthorized.Contains("Unknown") || newDateAuthorized.Contains("N/A") || newDateAuthorized.Contains("") || newDateAuthorized.Contains(" "))
                    {
                        newDateAuthorized = DateTime.Now.ToString("MM/dd/yyyy");
                    }

                    string newMouCreation = csv.GetField("Date Authorized").ToString();
                    newMouCreation = newMouCreation.Replace(" (Historical Import)", "");
                    if (newMouCreation.Contains("Unknown") || newMouCreation.Contains("N/A") || newMouCreation.Contains("") || newMouCreation.Contains(" "))
                    {
                        newMouCreation = DateTime.Now.ToString("MM/dd/yyyy");
                    }

                    string newMouExpiration = csv.GetField("MOU Expiration Date").ToString();
                    newMouExpiration = newMouExpiration.Replace(" (Historical Import)", "");
                    if (newMouExpiration.Contains("Unknown") || newMouExpiration.Contains("N/A") || newMouExpiration.Contains("") || newMouExpiration.Contains(" "))
                    {
                        newMouExpiration = DateTime.Now.ToString("MM/dd/yyyy");
                    }

                    string newCreatedDate = csv.GetField("Date Added").ToString();
                    newCreatedDate = newCreatedDate.Replace(" (Historical Import)", "");
                    if (newCreatedDate.Contains("Unknown") || newCreatedDate.Contains("N/A") || newCreatedDate.Contains("") || newCreatedDate.Contains(" "))
                    {
                        newCreatedDate = DateTime.Now.ToString();
                    }

                    string newUpdatedDate = csv.GetField("Last Updated").ToString();
                    newUpdatedDate = newUpdatedDate.Replace(" (Historical Import)", "");
                    if (newUpdatedDate.Contains("Unknown") || newUpdatedDate.Contains("N/A") || newUpdatedDate.Contains("") || newUpdatedDate.Contains(" "))
                    {
                        newUpdatedDate = DateTime.Now.ToString();
                    }

                    bool newIsActive = false;
                    string newDeactivatedDate = csv.GetField("Program Status").ToString();
                    if (newDeactivatedDate.Contains("Closed"))
                    {
                        if (!newDeactivatedDate.Contains("one-off") && !newDeactivatedDate.Contains("Duplicate Provider"))
                        {
                            if (newDeactivatedDate.Contains("(") && newDeactivatedDate.Contains(")"))
                            {
                                int pFrom = newDeactivatedDate.IndexOf("(") + 1;
                                int pTo = newDeactivatedDate.LastIndexOf(")");

                                newDeactivatedDate = newDeactivatedDate.Substring(pFrom, pTo - pFrom);
                            }
                            else
                            {
                                newDeactivatedDate = "";
                            }
                        }
                        else
                        {
                            newDeactivatedDate = "";
                        }
                        newIsActive = false;
                    }
                    else
                    {
                        newDeactivatedDate = "";
                        newIsActive = true;
                    }

                    Console.WriteLine("Ingesting Program: " + csv.GetField("Program Name") + " - Is_Active: " + newIsActive);

                    // Determine bools

                    string newHasIntake = csv.GetField("Have Intake");
                    bool hasIntake;
                    if(newHasIntake == "Y" || newHasIntake == "Yes")
                    {
                        hasIntake = true;
                    }
                    else
                    {
                        hasIntake = false;
                    }

                    /*string newHasLocations = csv.GetField("Have Locations");
                    bool hasLocations;
                    if (newHasLocations == "Y" || newHasLocations == "Yes")
                    {
                        hasLocations = true;
                    }
                    else
                    {
                        hasLocations = false;
                    }*/

                    int newLocationDetails = int.Parse(csv.GetField("Location Details Available"));
                    bool locationDetails;
                    if (newLocationDetails == 1)
                    {
                        locationDetails = true;
                    }
                    else
                    {
                        locationDetails = false;
                    }

                    string newHasConsent = csv.GetField("Have Consent");
                    bool hasConsent;
                    if (newHasConsent == "Y" || newHasConsent == "Yes")
                    {
                        hasConsent = true;
                    }
                    else
                    {
                        hasConsent = false;
                    }

                    string newHasMultipleLocations = csv.GetField("Multiple Locations");
                    bool hasMultipleLocations;
                    if (newHasMultipleLocations == "Y" || newHasMultipleLocations == "Yes")
                    {
                        hasMultipleLocations = true;
                    }
                    else
                    {
                        hasMultipleLocations = false;
                    }

                    string newReportingForm2020 = csv.GetField("2020 Reporting Form");
                    bool hasReportingForm2020;
                    if (newReportingForm2020 == "Y" || newReportingForm2020 == "Yes")
                    {
                        hasReportingForm2020 = true;
                    }
                    else
                    {
                        hasReportingForm2020 = false;
                    }

                    int newNationwide = int.Parse(csv.GetField("Nationwide"));
                    bool nationwide;
                    if (newNationwide == 1)
                    {
                        nationwide = true;
                    }
                    else
                    {
                        nationwide = false;
                    }

                    int newOnline = int.Parse(csv.GetField("Online"));
                    bool online;
                    if (newOnline == 1)
                    {
                        online = true;
                    }
                    else
                    {
                        online = false;
                    }

                    string newCohorts = csv.GetField("Support Cohorts");
                    bool cohorts;
                    if(newCohorts == "Yes" || newCohorts == "Y")
                    {
                        cohorts = true;
                    }
                    else
                    {
                        cohorts = false;
                    }

                    string newProgramStatus = csv.GetField("Program Status");
                    bool programStatus;
                    if(newProgramStatus.Contains("Active"))
                    {
                        programStatus = true;
                    }
                    else
                    {
                        programStatus = false;
                    }

                    // Get rid of N/As in id cols
                    string newLhnIntakeTicketId = csv.GetField("LHN Intake Ticket Number").ToString();
                    newLhnIntakeTicketId.Replace("Email contact update ", "");
                    if(newLhnIntakeTicketId.Contains("N/A"))
                    {
                        newLhnIntakeTicketId = "";
                    }

                    if (newLhnIntakeTicketId.Contains("-"))
                    {
                        int start = newLhnIntakeTicketId.IndexOf("-");
                        newLhnIntakeTicketId.Remove(start, 1);
                    }

                    if (newLhnIntakeTicketId.Contains("\r\n"))
                    {
                        int start = newLhnIntakeTicketId.IndexOf("\r\n");
                        newLhnIntakeTicketId.Remove(start, 1);
                    }

                    // TAKING THE LAST ONE IN THE LIST CURRENTLY    **NEEDS TO BE UPDATED **
                    if (newLhnIntakeTicketId.Contains(" "))
                    {
                        string[] vals = newLhnIntakeTicketId.Split(" ");

                        newLhnIntakeTicketId = vals[vals.Length - 1];
                    }

                    // Get the new Organization ID based off the legacy one
                    int newOrgId;
                    if(csv.GetField("Provider Unique ID") != null)
                    {
                        int legacyId = int.Parse(csv.GetField("Provider Unique ID"));

                        var org = _db.Organizations.SingleOrDefault(x => x.Legacy_Provider_Id == legacyId);
                        newOrgId = org.Id;  // This is the ID in the table of the programs organization, based off of the legacy IDs
                    }
                    else
                    {
                        newOrgId = -1;
                    }

                    int newProgId;
                    int.TryParse(csv.GetField("Program Unique ID"), out newProgId);

                    // Program Duration
                    /*int newProgramDuration;
                    string pd = "";// = csv.GetField("Program Duration");

                    // Get the specific progam duration if there's multiples listed in the org doc
                    foreach(ProgramLookup pl in dict)
                    {
                        if(pl.Program_Id == newProgId)
                        {
                            pd = pl.Program_Duration;
                        }
                    }

                    // Decide how to mark the programs program duration
                    switch (pd)
                    {
                        case "1 - 30 days":
                        {
                            newProgramDuration = 0;
                            break;
                        }
                        case "31 - 60 days":
                        {
                            newProgramDuration = 1;
                            break;
                        }
                        case "61 - 90 days":
                        {
                            newProgramDuration = 2;
                            break;
                        }
                        case "91 - 120 days":
                        {
                            newProgramDuration = 3;
                            break;
                        }
                        case "121 - 150 days":
                        {
                            newProgramDuration = 4;
                            break;
                        }
                        case "151 - 180 days":
                        {
                            newProgramDuration = 5;
                            break;
                        }
                        case "Individually Developed – not to exceed 40 hours":
                        {
                            newProgramDuration = 6;
                            break;
                        }
                        case "Self-paced":
                        {
                            newProgramDuration = 7;
                            break;
                        }
                        default:
                        {
                            newProgramDuration = 6; //Individually Developed – not to exceed 40 hours
                            break;
                        }
                    }*/

                    // Delivery Method
                    int newDeliveryMethod;
                    string dm = csv.GetField("Delivery Method");

                    switch (dm)
                    {
                        case "In-person":
                            {
                                newDeliveryMethod = 0;
                                break;
                            }
                        case "Online":
                            {
                                newDeliveryMethod = 1;
                                break;
                            }
                        case "Hybrid (in-person and online)":
                            {
                                newDeliveryMethod = 2;
                                break;
                            }
                        default:
                            {
                                newDeliveryMethod = 0; //In-person
                                break;
                            }
                    }

                    //Console.WriteLine("newDeliveryMethod: " + newDeliveryMethod);

                    bool newForSpouses = false;

                    string pps = csv.GetField("Participation Populations");
                    Console.WriteLine("Prog Name: " + csv.GetField("Program Name") + " with PP of " + pps);

                    string[] splitPops = pps.Split(", ");


                    if(pps.Contains("spouses"))
                    {
                        newForSpouses = true;
                    }
                        /*if (csv.GetField("Program Unique ID") != null)
                        {
                            int legacyProgramId = int.Parse(csv.GetField("Program Unique ID"));

                            var prog = _db.Programs.SingleOrDefault(x => x.Legacy_Program_Id == legacyProgramId);
                            newProgId = prog.Id;  // This is the ID in the table of the program, based off of the legacy IDs
                        }
                        else
                        {
                            newProgId = -1;
                        }*/

                        //if(dict.Count > 0)
                        //{
                        //Console.WriteLine("Program Name on sheet: " + csv.GetField("Program Name"));
                        //Console.WriteLine("========================================================================================");
                        //foreach (ProgramLookup p in dict)
                        //{
                        //Console.WriteLine("p.Program_Name in dict: " + p.Program_Name);
                        //Console.WriteLine("int.Parse(csv.GetField(Provider Unique ID): " + int.Parse(csv.GetField("Provider Unique ID")));
                        //Console.WriteLine("int.Parse(csv.GetField(Program Unique ID): " + csv.GetField<int>("Program Unique ID"));
                        //Console.WriteLine("p.Organization_Id: " + p.Organization_Id);
                        // Console.WriteLine("p.Program_Id: " + p.Program_Id);

                        string progId = csv.GetField("Program Unique ID");

                                    // THIS NEEDS TO MOVE TO WRAP THE ENTIRETY OF CREATING A PROGRAM INSTEAD OF CHECKING HERE... MULTIPLE PROGRAMS NEED TO BE MADE FOR ITEMS WITH COMMAS
                                    if (progId.Contains(","))
                                    {
                                        List<int> ids = progId.Replace(" ", "").Split(',').Select(int.Parse).ToList();

                                        foreach (int id in ids)
                                        {
                                            //Console.WriteLine("-=-=-=-=-=-=newCreatedDate: " + newCreatedDate);
                                            //Console.WriteLine("-=-=-=-=-=-=newUpdatedDate: " + newUpdatedDate);

                                            //Console.WriteLine("Program ID: " + newProgId);
                                            Console.WriteLine("Provider ID: " + int.Parse(csv.GetField("Provider Unique ID")));

                                    // Program Duration
                                    int newProgramDuration;
                                    string pd = "";// = csv.GetField("Program Duration");

                                    string newProgramName = "";

                                    // Get the specific progam duration if there's multiples listed in the org doc
                                    foreach (ProgramLookup pl in dict)
                                    {
                                        if (pl.Program_Id == id)
                                        {
                                            pd = pl.Program_Duration;
                                            newProgramName = pl.Program_Name;
                                            newIsActive = pl.Is_Active;   // THIS OVERRIDES THE OTHER LOGIC ABOVE SO THAT THE ACTIVE STATUS IS PULLED DIRECTLY FROM THE PROGRAMS DATA INSTEAD OF INFERRED FROM THE ORGS DATA
                                        }
                                    }

                                    // Decide how to mark the programs program duration
                                    switch (pd)
                                    {
                                        case "1 - 30 days":
                                            {
                                                newProgramDuration = 0;
                                                break;
                                            }
                                        case "31 - 60 days":
                                            {
                                                newProgramDuration = 1;
                                                break;
                                            }
                                        case "61 - 90 days":
                                            {
                                                newProgramDuration = 2;
                                                break;
                                            }
                                        case "91 - 120 days":
                                            {
                                                newProgramDuration = 3;
                                                break;
                                            }
                                        case "121 - 150 days":
                                            {
                                                newProgramDuration = 4;
                                                break;
                                            }
                                        case "151 - 180 days":
                                            {
                                                newProgramDuration = 5;
                                                break;
                                            }
                                        case "Individually Developed – not to exceed 40 hours":
                                            {
                                                newProgramDuration = 6;
                                                break;
                                            }
                                        case "Self-paced":
                                            {
                                                newProgramDuration = 7;
                                                break;
                                            }
                                        default:
                                            {
                                                newProgramDuration = 6; //Individually Developed – not to exceed 40 hours
                                                break;
                                            }
                                    }

                            string newSS = csv.GetField("Services Supported");

                            if(newSS == "" || newSS == null)
                            {
                                newSS = "All Services";
                            }

                            ProgramModel tempProg = new ProgramModel
                            {
                                Program_Name = newProgramName,
                                Organization_Name = csv.GetField("Organization Name"),
                                Organization_Id = newOrgId,
                                Lhn_Intake_Ticket_Id = newLhnIntakeTicketId,
                                Has_Intake = hasIntake,
                                Intake_Form_Version = csv.GetField("Intake Form Version"),
                                Qp_Intake_Submission_Id = csv.GetField("QP Intake ID"),
                                //Has_Locations = hasLocations,
                                Location_Details_Available = locationDetails,
                                Has_Consent = hasConsent,
                                Qp_Location_Submission_Id = csv.GetField("QP Locations/Refresh ID"),
                                Lhn_Location_Ticket_Id = csv.GetField("LHN Location Ticket Number"),
                                Has_Multiple_Locations = hasMultipleLocations,
                                Reporting_Form_2020 = hasReportingForm2020,
                                Date_Authorized = DateTime.Parse(newDateAuthorized),
                                Mou_Link = csv.GetField("MOU Packet Link"),
                                Mou_Creation_Date = DateTime.Parse(newMouCreation),
                                Mou_Expiration_Date = DateTime.Parse(newMouExpiration),
                                Nationwide = nationwide,
                                Online = online,
                                Participation_Populations = csv.GetField("Participation Populations"),
                                //Delivery_Method = newDeliveryMethod,//csv.GetField("Delivery Method"),
                                States_Of_Program_Delivery = csv.GetField("State(s) of Program Delivery"),
                                Program_Duration = newProgramDuration,//csv.GetField("Program Duration"),
                                Support_Cohorts = cohorts,
                                Opportunity_Type = csv.GetField("Opportunity Type"),
                                Job_Family = csv.GetField("Functional Area / Job Family"),
                                Services_Supported = newSS,// csv.GetField("Services Supported"),
                                Enrollment_Dates = csv.GetField("Enrollment Dates"),
                                Date_Created = DateTime.Parse(newCreatedDate),
                                Date_Updated = DateTime.Parse(newUpdatedDate),
                                Date_Deactivated = newDeactivatedDate != "" ? DateTime.Parse(newDeactivatedDate) : new DateTime(),
                                Is_Active = newIsActive,
                                Created_By = "Ingest", // Set this so no errors occur
                                Updated_By = "Ingest", // Set this so no errors occur
                                Program_Url = csv.GetField("URL"),
                                Program_Status = programStatus,
                                Admin_Poc_First_Name = csv.GetField("Admin POC First Name 1"),
                                Admin_Poc_Last_Name = csv.GetField("Admin POC Last Name 1"),
                                Admin_Poc_Email = csv.GetField("Admin POC Email Address 1"),
                                Admin_Poc_Phone = csv.GetField("Admin POC Phone Number 1"),
                                Public_Poc_Name = csv.GetField("POC for Site - Name"),
                                Public_Poc_Email = csv.GetField("POC for Site - Email"),
                                Notes = csv.GetField("Notes"),
                                For_Spouses = newForSpouses,
                                Legacy_Program_Id = id,
                                Legacy_Provider_Id = int.Parse(csv.GetField("Provider Unique ID"))
                            };

                            _db.Programs.Add(tempProg);
                            //_db.SaveChanges();

                            var result = await _db.SaveChangesAsync();

                            if (result >= 1)
                            {
                                // Generate Participation Popluation Entries for this program
                                //string pps = csv.GetField("Participation Populations");

                                //string[] splitPops = pps.Split(", ");

                                foreach (string s in splitPops)
                                {
                                    //Console.WriteLine("s: " + s);
                                    int newPop = FindProgramParticipationPopulationIdFromName(s);

                                    if (newPop != -1)
                                    {
                                        ProgramParticipationPopulation pp = new ProgramParticipationPopulation
                                        {
                                            Program_Id = tempProg.Id,
                                            Participation_Population_Id = newPop
                                        };

                                        _db.ProgramParticipationPopulation.Add(pp);
                                    }
                                }

                                // Generate Services Supported Entries for this program
                                string ss = csv.GetField("Services Supported");

                                string[] splitServices = ss.Split(", ");

                                foreach (string s in splitServices)
                                {
                                    Console.WriteLine("ss: " + s);
                                    int newService = FindProgramServiceIdFromName(s);

                                    if (newService != -1)
                                    {
                                        ProgramService ps = new ProgramService
                                        {
                                            Program_Id = tempProg.Id,
                                            Service_Id = newService
                                        };

                                        _db.ProgramService.Add(ps);
                                    }
                                }

                                // Generate Delivery Method Entries for this program
                                string dms = csv.GetField("Delivery Method");

                                if(dms == "Hybrid (in-person and online) and In-person" || dms == "Hybrid (In-person and online) and In-person")
                                {
                                    int newMethod = FindProgramDeliveryMethodIdFromName("Hybrid (In-person and online)");

                                    if (newMethod != -1)
                                    {
                                        ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }

                                    newMethod = FindProgramDeliveryMethodIdFromName("In-person");

                                    if (newMethod != -1)
                                    {
                                        ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }
                                }
                                else if (dms == "Hybrid (in-person and online) and Online" || dms == "Hybrid (In-person and online) and Online")
                                {
                                    int newMethod = FindProgramDeliveryMethodIdFromName("Hybrid (In-person and online)");

                                    if (newMethod != -1)
                                    {
                                        ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }

                                    newMethod = FindProgramDeliveryMethodIdFromName("Online");

                                    if (newMethod != -1)
                                    {
                                        ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }
                                }
                                else if (dms == "In-person and Online")
                                {
                                    int newMethod = FindProgramDeliveryMethodIdFromName("In-person");

                                    if (newMethod != -1)
                                    {
                                        ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }

                                    newMethod = FindProgramDeliveryMethodIdFromName("Online");

                                    if (newMethod != -1)
                                    {
                                        ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }
                                }
                                else
                                {
                                    string[] splitMethods = dms.Split(", ");

                                    foreach (string m in splitMethods)
                                    {
                                        Console.WriteLine("dm: " + m);
                                        int newMethod = FindProgramDeliveryMethodIdFromName(m);

                                        if (newMethod != -1)
                                        {
                                            ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                            {
                                                Program_Id = tempProg.Id,
                                                Delivery_Method_Id = newMethod
                                            };

                                            _db.ProgramDeliveryMethod.Add(pdm);
                                        }
                                    }
                                }

                                var result2 = await _db.SaveChangesAsync();
                            }

                            logMessage += "\n===================== Record #" + i + " ====================";
                                    logMessage += "\n====================================================";
                                    logMessage += "\nProgram_Name " + tempProg.Program_Name;
                                    logMessage += "\nOrganization_Name " + tempProg.Organization_Name;
                                    logMessage += "\nOrganization_Id " + tempProg.Organization_Id;
                                    logMessage += "\nLhn_Intake_Ticket_Id " + tempProg.Lhn_Intake_Ticket_Id;
                                    logMessage += "\nHas_Intake " + tempProg.Has_Intake;
                                    logMessage += "\nIntake_Form_Version " + tempProg.Intake_Form_Version;
                                    logMessage += "\nQp_Intake_Submission_Id " + tempProg.Qp_Intake_Submission_Id;
                                    //logMessage += "\nHas_Locations " + newProg.Has_Locations;
                                    logMessage += "\nLocation_Details_Available " + tempProg.Location_Details_Available;
                                    logMessage += "\nHas_Consent " + tempProg.Has_Consent;
                                    logMessage += "\nQp_Location_Submission_Id " + tempProg.Qp_Location_Submission_Id;
                                    logMessage += "\nLhn_Location_Ticket_Id " + tempProg.Lhn_Location_Ticket_Id;
                                    logMessage += "\nHas_Multiple_Locations " + tempProg.Has_Multiple_Locations;
                                    logMessage += "\nReporting_Form_2020 " + tempProg.Reporting_Form_2020;
                                    logMessage += "\nDate_Authorized " + tempProg.Date_Authorized;
                                    logMessage += "\nMou_Link " + tempProg.Mou_Link;
                                    logMessage += "\nMou_Creation_Date " + tempProg.Mou_Creation_Date;
                                    logMessage += "\nMou_Expiration_Date " + tempProg.Mou_Expiration_Date;
                                    logMessage += "\nNationwide " + tempProg.Nationwide;
                                    logMessage += "\nOnline " + tempProg.Online;
                                    logMessage += "\nParticipation_Populations " + tempProg.Participation_Populations;
                                    logMessage += "\nDelivery_Method " + tempProg.Delivery_Method;
                                    logMessage += "\nStates_Of_Program_Delivery " + tempProg.States_Of_Program_Delivery;
                                    logMessage += "\nProgram_Duration " + tempProg.Program_Duration;
                                    logMessage += "\nSupport_Cohorts " + tempProg.Support_Cohorts;
                                    logMessage += "\nOpportunity_Type " + tempProg.Opportunity_Type;
                                    logMessage += "\nJob_Family " + tempProg.Job_Family;
                                    logMessage += "\nServices_Supported " + tempProg.Services_Supported;
                                    logMessage += "\nEnrollment_Dates " + tempProg.Enrollment_Dates;
                                    logMessage += "\nDate_Created " + tempProg.Date_Created;
                                    logMessage += "\nDate_Updated " + tempProg.Date_Updated;
                                    logMessage += "\nCreated_By " + tempProg.Created_By; // Set this so no errors occur
                                    logMessage += "\nUpdated_By " + tempProg.Updated_By; // Set this so no errors occur
                                    logMessage += "\nProgram_Url " + tempProg.Program_Url;
                                    logMessage += "\nProgram_Status " + tempProg.Program_Status;
                                    logMessage += "\nAdmin_Poc_First_Name " + tempProg.Admin_Poc_First_Name;
                                    logMessage += "\nAdmin_Poc_Last_Name " + tempProg.Admin_Poc_Last_Name;
                                    logMessage += "\nAdmin_Poc_Email " + tempProg.Admin_Poc_Email;
                                    logMessage += "\nAdmin_Poc_Phone " + tempProg.Admin_Poc_Phone;
                                    logMessage += "\nPublic_Poc_Name " + tempProg.Public_Poc_Name;
                                    logMessage += "\nPublic_Poc_Email " + tempProg.Public_Poc_Email;
                                    logMessage += "\nNotes " + tempProg.Notes;
                                    logMessage += "\nFor_Spouses " + tempProg.For_Spouses;
                                    logMessage += "\nLegacy_Program_Id " + tempProg.Legacy_Program_Id;
                                    logMessage += "\nLegacy_Provider_Id " + tempProg.Legacy_Provider_Id;
                                    logMessage += "\n====================================================";

                                    i++;
                                }
                            }
                            else
                            {
                                //Console.WriteLine("-=-=-=-=-=-=newCreatedDate: " + newCreatedDate);
                                //Console.WriteLine("-=-=-=-=-=-=newUpdatedDate: " + newUpdatedDate);

                                //Console.WriteLine("Program ID: " + newProgId);
                                Console.WriteLine("Provider ID: " + int.Parse(csv.GetField("Provider Unique ID")));

                        // Program Duration
                        int newProgramDuration;
                        string pd = "";// = csv.GetField("Program Duration");

                        // Get the specific progam duration if there's multiples listed in the org doc
                        foreach (ProgramLookup pl in dict)
                        {
                            if (pl.Program_Id == newProgId)
                            {
                                pd = pl.Program_Duration;
                            }
                        }

                        // Decide how to mark the programs program duration
                        switch (pd)
                        {
                            case "1 - 30 days":
                                {
                                    newProgramDuration = 0;
                                    break;
                                }
                            case "31 - 60 days":
                                {
                                    newProgramDuration = 1;
                                    break;
                                }
                            case "61 - 90 days":
                                {
                                    newProgramDuration = 2;
                                    break;
                                }
                            case "91 - 120 days":
                                {
                                    newProgramDuration = 3;
                                    break;
                                }
                            case "121 - 150 days":
                                {
                                    newProgramDuration = 4;
                                    break;
                                }
                            case "151 - 180 days":
                                {
                                    newProgramDuration = 5;
                                    break;
                                }
                            case "Individually Developed – not to exceed 40 hours":
                                {
                                    newProgramDuration = 6;
                                    break;
                                }
                            case "Self-paced":
                                {
                                    newProgramDuration = 7;
                                    break;
                                }
                            default:
                                {
                                    newProgramDuration = 6; //Individually Developed  – not to exceed 40 hours
                                    break;
                                }
                        }

                        ProgramModel newProg = new ProgramModel
                                {
                                    Program_Name = csv.GetField("Program Name"),
                                    Organization_Name = csv.GetField("Organization Name"),
                                    Organization_Id = newOrgId,
                                    Lhn_Intake_Ticket_Id = newLhnIntakeTicketId,
                                    Has_Intake = hasIntake,
                                    Intake_Form_Version = csv.GetField("Intake Form Version"),
                                    Qp_Intake_Submission_Id = csv.GetField("QP Intake ID"),
                                    Is_Active = newIsActive,
                                    //Has_Locations = hasLocations,
                                    Location_Details_Available = locationDetails,
                                    Has_Consent = hasConsent,
                                    Qp_Location_Submission_Id = csv.GetField("QP Locations/Refresh ID"),
                                    Lhn_Location_Ticket_Id = csv.GetField("LHN Location Ticket Number"),
                                    Has_Multiple_Locations = hasMultipleLocations,
                                    Reporting_Form_2020 = hasReportingForm2020,
                                    Date_Authorized = DateTime.Parse(newDateAuthorized),
                                    Mou_Link = csv.GetField("MOU Packet Link"),
                                    Mou_Creation_Date = DateTime.Parse(newMouCreation),
                                    Mou_Expiration_Date = DateTime.Parse(newMouExpiration),
                                    Nationwide = nationwide,
                                    Online = online,
                                    Participation_Populations = csv.GetField("Participation Populations"),
                                    //Delivery_Method = newDeliveryMethod,//csv.GetField("Delivery Method"),
                                    States_Of_Program_Delivery = csv.GetField("State(s) of Program Delivery"),
                                    Program_Duration = newProgramDuration, //csv.GetField("Program Duration"),
                                    Support_Cohorts = cohorts,
                                    Opportunity_Type = csv.GetField("Opportunity Type"),
                                    Job_Family = csv.GetField("Functional Area / Job Family"),
                                    Services_Supported = csv.GetField("Services Supported"),
                                    Enrollment_Dates = csv.GetField("Enrollment Dates"),
                                    Date_Created = DateTime.Parse(newCreatedDate),
                                    Date_Updated = DateTime.Parse(newUpdatedDate),
                                    Created_By = "Ingest", // Set this so no errors occur
                                    Updated_By = "Ingest", // Set this so no errors occur
                                    Program_Url = csv.GetField("URL"),
                                    Program_Status = programStatus,
                                    Admin_Poc_First_Name = csv.GetField("Admin POC First Name 1"),
                                    Admin_Poc_Last_Name = csv.GetField("Admin POC Last Name 1"),
                                    Admin_Poc_Email = csv.GetField("Admin POC Email Address 1"),
                                    Admin_Poc_Phone = csv.GetField("Admin POC Phone Number 1"),
                                    Public_Poc_Name = csv.GetField("POC for Site - Name"),
                                    Public_Poc_Email = csv.GetField("POC for Site - Email"),
                                    Notes = csv.GetField("Notes"),
                                    For_Spouses = newForSpouses,
                                    Legacy_Program_Id = newProgId,
                                    Legacy_Provider_Id = int.Parse(csv.GetField("Provider Unique ID"))
                                };

                                _db.Programs.Add(newProg);
                                var result = await _db.SaveChangesAsync();

                                if(result >= 1)
                                {
                                    // Generate Participation Popluation Entries for this program
                                    //string pps = csv.GetField("Participation Populations");

                                    //string[] splitPops = pps.Split(", ");

                                    foreach (string s in splitPops)
                                    {
                                        //Console.WriteLine("s: " + s);
                                        int newPop = FindProgramParticipationPopulationIdFromName(s);

                                        if(newPop != -1)
                                        {
                                            ProgramParticipationPopulation pp = new ProgramParticipationPopulation
                                            {
                                                Program_Id = newProg.Id,
                                                Participation_Population_Id = newPop
                                            };

                                            _db.ProgramParticipationPopulation.Add(pp);
                                        }
                                    }

                                    // Generate Services Supported Entries for this program
                                    string ss = csv.GetField("Services Supported");

                                    string[] splitServices = ss.Split(", ");

                                    foreach (string s in splitServices)
                                    {
                                        Console.WriteLine("ss: " + s);
                                        int newService = FindProgramServiceIdFromName(s);

                                        if (newService != -1)
                                        {
                                            ProgramService ps = new ProgramService
                                            {
                                                Program_Id = newProg.Id,
                                                Service_Id = newService
                                            };

                                            _db.ProgramService.Add(ps);
                                        }
                                    }

                            // Generate Delivery Method Entries for this program
                            string dms = csv.GetField("Delivery Method");

                            if (dms == "Hybrid (in-person and online) and In-person" || dms == "Hybrid (In-person and online) and In-person")
                            {
                                int newMethod = FindProgramDeliveryMethodIdFromName("Hybrid (In-person and online)");

                                if (newMethod != -1)
                                {
                                    ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }

                                newMethod = FindProgramDeliveryMethodIdFromName("In-person");

                                if (newMethod != -1)
                                {
                                    ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }
                            }
                            else if (dms == "Hybrid (in-person and online) and Online" || dms == "Hybrid (In-person and online) and Online")
                            {
                                int newMethod = FindProgramDeliveryMethodIdFromName("Hybrid (In-person and online)");

                                if (newMethod != -1)
                                {
                                    ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }

                                newMethod = FindProgramDeliveryMethodIdFromName("Online");

                                if (newMethod != -1)
                                {
                                    ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }
                            }
                            else if (dms == "In-person and Online")
                            {
                                int newMethod = FindProgramDeliveryMethodIdFromName("In-person");

                                if (newMethod != -1)
                                {
                                    ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }

                                newMethod = FindProgramDeliveryMethodIdFromName("Online");

                                if (newMethod != -1)
                                {
                                    ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }
                            }
                            else
                            {
                                string[] splitMethods = dms.Split(", ");

                                foreach (string m in splitMethods)
                                {
                                    Console.WriteLine("dm: " + m);
                                    int newMethod = FindProgramDeliveryMethodIdFromName(m);

                                    if (newMethod != -1)
                                    {
                                        ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = newProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }
                                }
                            }

                            var result2 = await _db.SaveChangesAsync();

                            // Generate Delivery Method Entries for this program
                            /*string dms = csv.GetField("Delivery Method");

                            string[] splitMethods = dms.Split(", ");

                            foreach (string m in splitMethods)
                            {
                                Console.WriteLine("dm: " + m);
                                int newMethod = FindProgramDeliveryMethodIdFromName(m);

                                if (newMethod != -1)
                                {
                                    ProgramDeliveryMethod pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }
                            }

                    var result2 = await _db.SaveChangesAsync();*/
                        }
                                //var result = await _db.SaveChangesAsync();
                                logMessage += "\n===================== Record #" + i + " ====================";
                                logMessage += "\n====================================================";
                                logMessage += "\nProgram_Name " + newProg.Program_Name;
                                logMessage += "\nOrganization_Id " + newProg.Organization_Id;
                                logMessage += "\nLhn_Intake_Ticket_Id " + newProg.Lhn_Intake_Ticket_Id;
                                logMessage += "\nHas_Intake " + newProg.Has_Intake;
                                logMessage += "\nIntake_Form_Version " + newProg.Intake_Form_Version;
                                logMessage += "\nQp_Intake_Submission_Id " + newProg.Qp_Intake_Submission_Id;
                                //logMessage += "\nHas_Locations " + newProg.Has_Locations;
                                logMessage += "\nLocation_Details_Available " + newProg.Location_Details_Available;
                                logMessage += "\nHas_Consent " + newProg.Has_Consent;
                                logMessage += "\nQp_Location_Submission_Id " + newProg.Qp_Location_Submission_Id;
                                logMessage += "\nLhn_Location_Ticket_Id " + newProg.Lhn_Location_Ticket_Id;
                                logMessage += "\nHas_Multiple_Locations " + newProg.Has_Multiple_Locations;
                                logMessage += "\nReporting_Form_2020 " + newProg.Reporting_Form_2020;
                                logMessage += "\nDate_Authorized " + newProg.Date_Authorized;
                                logMessage += "\nMou_Link " + newProg.Mou_Link;
                                logMessage += "\nMou_Creation_Date " + newProg.Mou_Creation_Date;
                                logMessage += "\nMou_Expiration_Date " + newProg.Mou_Expiration_Date;
                                logMessage += "\nNationwide " + newProg.Nationwide;
                                logMessage += "\nOnline " + newProg.Online;
                                logMessage += "\nParticipation_Populations " + newProg.Participation_Populations;
                                logMessage += "\nDelivery_Method " + newProg.Delivery_Method;
                                logMessage += "\nStates_Of_Program_Delivery " + newProg.States_Of_Program_Delivery;
                                logMessage += "\nProgram_Duration " + newProg.Program_Duration;
                                logMessage += "\nSupport_Cohorts " + newProg.Support_Cohorts;
                                logMessage += "\nOpportunity_Type " + newProg.Opportunity_Type;
                                logMessage += "\nJob_Family " + newProg.Job_Family;
                                logMessage += "\nServices_Supported " + newProg.Services_Supported;
                                logMessage += "\nEnrollment_Dates " + newProg.Enrollment_Dates;
                                logMessage += "\nDate_Created " + newProg.Date_Created;
                                logMessage += "\nDate_Updated " + newProg.Date_Updated;
                                logMessage += "\nCreated_By " + newProg.Created_By; // Set this so no errors occur
                                logMessage += "\nUpdated_By " + newProg.Updated_By; // Set this so no errors occur
                                logMessage += "\nProgram_Url " + newProg.Program_Url;
                                logMessage += "\nProgram_Status " + newProg.Program_Status;
                                logMessage += "\nAdmin_Poc_First_Name " + newProg.Admin_Poc_First_Name;
                                logMessage += "\nAdmin_Poc_Last_Name " + newProg.Admin_Poc_Last_Name;
                                logMessage += "\nAdmin_Poc_Email " + newProg.Admin_Poc_Email;
                                logMessage += "\nAdmin_Poc_Phone " + newProg.Admin_Poc_Phone;
                                logMessage += "\nPublic_Poc_Name " + newProg.Public_Poc_Name;
                                logMessage += "\nPublic_Poc_Email " + newProg.Public_Poc_Email;
                                logMessage += "\nNotes " + newProg.Notes;
                                logMessage += "\nFor_Spouses " + newProg.For_Spouses;
                                logMessage += "\nLegacy_Program_Id " + newProg.Legacy_Program_Id;
                                logMessage += "\nLegacy_Provider_Id " + newProg.Legacy_Provider_Id;
                                logMessage += "\nJob Family Notes: " + jfMsg;
                                logMessage += "\n====================================================";

                                i++;
                                //Console.WriteLine("p.Organization_Id: " + p.Organization_Id);
                                //Console.WriteLine("p.Program_Id: " + p.Program_Id);
                            }
                    //}
                    //}

                    
                }

                // Write the log file
                string strFileName = "SB-Prog-Ingest-Log.txt";

                try
                {
                    FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), strFileName), FileMode.Create, FileAccess.Write);
                    StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                    objStreamWriter.WriteLine(logMessage);
                    objStreamWriter.Close();
                    objFilestream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IngestOpportunities(string source)  // Ingests the data from one DB table, formats/validates it, and then replaces the current data in the organizations table with it
        {
            // source is the source CSV file
            //OppIngestTest.csv

            string newSource = "OppIngestTest14";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = "|",
                MissingFieldFound = null
            };

            string logMessage = "";
            string badRefsMessage = "";

            // Remove existing groups from Groups table
            DeleteAllOpportunityGroups();

            // Remove existing records from Opp Table
            DeleteAllOpportunities();

            

            using (var reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + newSource + ".csv"))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                int i = 0;

                while (csv.Read())
                {
                    //var record = csv.GetRecord<OpportunityModel>();
                    // Do something with the record.

                    // Remove (Historical Import) strings from dates
                    string newCreatedDate = csv.GetField("DATEENTERED").ToString();
                    newCreatedDate = newCreatedDate.Replace(" (Historical Import)", "");

                    string newUpdatedDate = csv.GetField("Date Updated").ToString();
                    newUpdatedDate = newUpdatedDate.Replace(" (Historical Import)", "");

                    string newInitDate = csv.GetField("DATEPROGRAMINITIATED").ToString();
                    newInitDate = newInitDate.Replace("Unknown", "");
                    if (newInitDate == "")
                    {
                        newInitDate = DateTime.Now.ToString();
                    }

                    string newMultLoc = csv.GetField("Multiple Locations");
                    bool multLoc;
                    if (newMultLoc == "Y")
                    {
                        multLoc = true;
                    }
                    else
                    {
                        multLoc = false;
                    }

                    string newCohorts = csv.GetField("Cohorts");
                    bool cohorts;
                    if(newCohorts == "Y" || newCohorts == "Yes")
                    {
                        cohorts = true;
                    }
                    else
                    {
                        cohorts = false;
                    }

                    string newMous = csv.GetField("MOUs");
                    bool mous;
                    if(newMous == "Y")
                    {
                        mous = true;
                    }
                    else
                    {
                        mous = false;
                    }

                    string newNationwide = csv.GetField("NATIONWIDE");
                    bool nationwide;
                    if(newNationwide == "1")
                    {
                        nationwide = true;
                    }
                    else
                    {
                        nationwide = false;
                    }

                    string newOnline = csv.GetField("ONLINE");
                    bool online;
                    if (newOnline == "1")
                    {
                        online = true;
                    }
                    else
                    {
                        online = false;
                    }

                    int newNumLocations;
                    Int32.TryParse(csv.GetField("Number of Locations"), out newNumLocations);

                    int newId;
                    Int32.TryParse(csv.GetField("ID"), out newId);

                    int newGroupId;
                    Int32.TryParse(csv.GetField("GROUPID"), out newGroupId);


                    // Get the new Organization ID based off the legacy one
                    int newOrgId = -1;
                    int newProgId = -1;
                    string newMou_Link = "";
                    DateTime newMou_Expiration_Date = new DateTime();
                    string newAdmin_Poc_First_Name = "";
                    string newAdmin_Poc_Last_Name = "";
                    string newAdmin_Poc_Email = "";
                    if (csv.GetField("Provider Unique ID") != null)
                    {
                        int legacyId = int.Parse(csv.GetField("Provider Unique ID"));

                        var org = _db.Organizations.SingleOrDefault(x => x.Legacy_Provider_Id == legacyId);
                        newOrgId = org.Id;  // This is the ID in the table of the programs organization, based off of the legacy IDs

                        // Get the new Program ID based off the legacy one
                        if (csv.GetField("Program Unique ID") != null)
                        {
                            int legacyProgId;// = int.Parse(csv.GetField("Program Unique ID"));
                            Int32.TryParse(csv.GetField("Program Unique ID"), out legacyProgId);

                            
                            //Console.WriteLine("Program_Name: " + csv.GetField("PROGRAMNAME"));
                            //Console.WriteLine("legacyId: " + legacyId);
                            //Console.WriteLine("legacyProgId: " + legacyProgId);

                            var prog = _db.Programs.SingleOrDefault(x => x.Legacy_Program_Id == legacyProgId &&
                            x.Legacy_Provider_Id == legacyId);

                            if(prog != null)
                            {
                                newProgId = prog.Id;  // This is the ID in the table of the programs organization, based off of the legacy IDs

                                newMou_Link = prog.Mou_Link;
                                newMou_Expiration_Date = prog.Mou_Expiration_Date;
                                newAdmin_Poc_First_Name = prog.Admin_Poc_First_Name;
                                newAdmin_Poc_Last_Name = prog.Admin_Poc_Last_Name;
                                newAdmin_Poc_Email = prog.Admin_Poc_Email;
                            }
                            else
                            {
                                // add this to the list of items that need attention
                                newProgId = -1;

                                badRefsMessage += "Couldn't find program to tie to opportunity\n";
                                badRefsMessage += "====================================================================\n";
                                badRefsMessage += "Org Name: " + org.Name + "\n";
                                badRefsMessage += "Org ID in DB: " + org.Id + "\n";
                                badRefsMessage += "Legacy Provider ID: " + legacyId + "\n";
                                badRefsMessage += "Legacy Program ID: " + legacyProgId + "\n";
                                badRefsMessage += "====================================================================\n";
                            }

                        }
                    }

                    //Console.WriteLine("newOrgId: " + newOrgId);
                    //Console.WriteLine("newProgId: " + newProgId);


                    string newDeliveryMethod = "0";

                    if(csv.GetField("Delivery Method") == "In-person")
                    {
                        newDeliveryMethod = "0";
                    }
                    else if (csv.GetField("Delivery Method") == "Online")
                    {
                        newDeliveryMethod = "1";
                    }
                    else if (csv.GetField("Delivery Method") == "Hybrid (in-person and online)" || csv.GetField("Delivery Method") == "Hybrid" || csv.GetField("Delivery Method") == "Hybrid (in-person and online) and Online")
                    {
                        newDeliveryMethod = "2";
                    }

                    /*if(csv.GetField("Program Status") == "Active")
                    {
                        newActive = true;
                    }*/

                    bool newIsActive = false;
                    string newDeactivatedDate = csv.GetField("Program Status").ToString();
                    Console.WriteLine("newDeactivatedDate: " + newDeactivatedDate);
                    if (newDeactivatedDate.Contains("Closed"))
                    {
                        // Opportunities from ingest dont have deactivation dates, this was added in the CMS, so we dont need to pull them here
                        /*if (!newDeactivatedDate.Contains("one-off") && !newDeactivatedDate.Contains("Duplicate Provider"))
                        {
                            int pFrom = newDeactivatedDate.IndexOf("(") + 1;
                            int pTo = newDeactivatedDate.LastIndexOf(")");

                            newDeactivatedDate = newDeactivatedDate.Substring(pFrom, pTo - pFrom);
                        }
                        else
                        {
                            newDeactivatedDate = "";
                        }*/
                        Console.WriteLine("newDeactivatedDate contained Closed");
                        newDeactivatedDate = "";
                        newIsActive = false;
                    }
                    else
                    {
                        Console.WriteLine("newDeactivatedDate didnt contain Closed");
                        newDeactivatedDate = "";
                        newIsActive = true;
                    }

                    //int newProgId;
                    //Int32.TryParse(csv.GetField("Program Unique ID"), out newProgId);

                    //Console.WriteLine("-=-=-=-=-=-=newCreatedDate: " + newCreatedDate);
                    //Console.WriteLine("-=-=-=-=-=-=newUpdatedDate: " + newUpdatedDate);

                    OpportunityModel newOpp = new OpportunityModel
                    {
                        Group_Id = newGroupId,
                        Organization_Id = newOrgId,
                        Organization_Name = csv.GetField("ORGANIZATION"),
                        Program_Id = newProgId,
                        Date_Deactivated = newDeactivatedDate != "" ? DateTime.Parse(newDeactivatedDate) : new DateTime(),
                        Is_Active = newIsActive,
                        Program_Name = csv.GetField("PROGRAMNAME"),
                        Opportunity_Url = csv.GetField("URL"),
                        Date_Program_Initiated = DateTime.Parse(newInitDate),
                        Date_Created = DateTime.Parse(newCreatedDate),
                        Date_Updated = DateTime.Parse(newUpdatedDate),
                        Created_By = "Ingest", // Set this so no errors occur
                        Updated_By = "Ingest", // Set this so no errors occur
                        Employer_Poc_Name = csv.GetField("EMPLOYERPOC"),
                        Employer_Poc_Email = csv.GetField("EMPLOYERPOCEMAIL"),
                        Training_Duration = csv.GetField("DURATIONOFTRAINING"),
                        Service = csv.GetField("SERVICE"),
                        Delivery_Method = newDeliveryMethod,
                        Multiple_Locations = multLoc,
                        Program_Type = csv.GetField("Program Type"),
                        Job_Families = csv.GetField("JOBFAMILIES"),
                        Participation_Populations = csv.GetField("Participation Populations"),
                        Support_Cohorts = cohorts, //Yes/null
                        Enrollment_Dates = csv.GetField("Enrollment Dates"),
                        Mous = mous,   //y/n
                        Num_Locations = newNumLocations,
                        Installation = csv.GetField("INSTALLATION"),
                        City = csv.GetField("CITY"),
                        State = csv.GetField("STATE"),
                        Zip = csv.GetField("ZIP"),
                        Lat = double.Parse(csv.GetField("LAT")),
                        Long = double.Parse(csv.GetField("LONG")),
                        Nationwide = nationwide,
                        Online = online,
                        Summary_Description = csv.GetField("SUMMARYDESCRIPTION"),
                        Jobs_Description = csv.GetField("JOBSDESCRIPTION"),
                        Links_To_Prospective_Jobs = csv.GetField("LINKSTOPROSPECTIVE JOBS"),
                        Locations_Of_Prospective_Jobs_By_State = csv.GetField("LOCATIONSOFPROSPECTIVEJOBSBYSTATE"),
                        Salary = csv.GetField("SALARY"),
                        Prospective_Job_Labor_Demand = csv.GetField("Prospective Job Labor Demand"),
                        Target_Mocs = csv.GetField("TARGETMOCs"),
                        Other_Eligibility_Factors = csv.GetField("OTHERELIGIBILITYFACTORS"),
                        Cost = csv.GetField("COST"),
                        Other = csv.GetField("OTHER"),
                        Notes = csv.GetField("Notes"),
                        Mou_Link = newMou_Link,
                        Mou_Expiration_Date = newMou_Expiration_Date,
                        Admin_Poc_First_Name = newAdmin_Poc_First_Name,
                        Admin_Poc_Last_Name = newAdmin_Poc_Last_Name,
                        Admin_Poc_Email = newAdmin_Poc_Email,
                        Legacy_Program_Id = int.Parse(csv.GetField("Program Unique ID")),
                        Legacy_Provider_Id = int.Parse(csv.GetField("Provider Unique ID")),
                        Legacy_Opportunity_Id = newId
                    };

                    _db.Opportunities.Add(newOpp);
                    await _db.SaveChangesAsync();

                    //TODO: convert to mapping
                    var newGroup = new OpportunityGroupModel
                    {
                        Group_Id = newGroupId,
                        Opportunity_Id = newOpp.Id,
                        Title = "",
                        Lat = newOpp.Lat,
                        Long = newOpp.Long
                    };

                    _db.OpportunityGroups.Add(newGroup);
                    await _db.SaveChangesAsync();

                    

                    logMessage += "\n===================== Record #" + i + " ====================";
                    logMessage += "\n====================================================";
                    logMessage += "\nProgram_Name " + newOpp.Program_Name;
                    logMessage += "\nOpportunity_Url " + newOpp.Opportunity_Url;
                    logMessage += "\nDate_Program_Initiated " + newOpp.Date_Program_Initiated;
                    logMessage += "\nDate_Created " + newOpp.Date_Created;
                    logMessage += "\nDate_Updated " + newOpp.Date_Updated;
                    logMessage += "\nCreated_By " + newOpp.Created_By;
                    logMessage += "\nUpdated_By " + newOpp.Updated_By;
                    logMessage += "\nEmployer_Poc_Name " + newOpp.Employer_Poc_Name;
                    logMessage += "\nEmployer_Poc_Email " + newOpp.Employer_Poc_Email;
                    logMessage += "\nTraining_Duration " + newOpp.Training_Duration;
                    logMessage += "\nService " + newOpp.Service;
                    logMessage += "\nDelivery_Method " + newOpp.Delivery_Method;
                    logMessage += "\nMultiple_Locations " + newOpp.Multiple_Locations;
                    logMessage += "\nProgram_Type " + newOpp.Program_Type;
                    logMessage += "\nJob_Families " + newOpp.Job_Families;
                    logMessage += "\nParticipation_Populations " + newOpp.Participation_Populations;
                    logMessage += "\nSupport_Cohorts " + newOpp.Support_Cohorts;
                    logMessage += "\nEnrollment_Dates " + newOpp.Enrollment_Dates;
                    logMessage += "\nMous " + newOpp.Mous;
                    logMessage += "\nNum_Locations " + newOpp.Num_Locations;
                    logMessage += "\nInstallation " + newOpp.Installation;
                    logMessage += "\nCity " + newOpp.City;
                    logMessage += "\nState " + newOpp.State;
                    logMessage += "\nZip " + newOpp.Zip;
                    logMessage += "\nLat " + newOpp.Lat;
                    logMessage += "\nLong " + newOpp.Long;
                    logMessage += "\nNationwide " + newOpp.Nationwide;
                    logMessage += "\nOnline " + newOpp.Online;
                    logMessage += "\nSummary_Description " + newOpp.Summary_Description;
                    logMessage += "\nJobs_Description " + newOpp.Jobs_Description;
                    logMessage += "\nLinks_To_Prospective_Jobs " + newOpp.Links_To_Prospective_Jobs;
                    logMessage += "\nLocations_Of_Prospective_Jobs_By_State " + newOpp.Locations_Of_Prospective_Jobs_By_State;
                    logMessage += "\nSalary " + newOpp.Salary;
                    logMessage += "\nProspective_Job_Labor_Demand " + newOpp.Prospective_Job_Labor_Demand;
                    logMessage += "\nTarget_Mocs " + newOpp.Target_Mocs;
                    logMessage += "\nOther_Eligibility_Factors " + newOpp.Other_Eligibility_Factors;
                    logMessage += "\nCost " + newOpp.Cost;
                    logMessage += "\nOther " + newOpp.Other;
                    logMessage += "\nNotes " + newOpp.Notes;
                    logMessage += "\nGroup_Id " + newOpp.Group_Id;
                    logMessage += "\nLegacy_Program_Id " + newOpp.Legacy_Program_Id;
                    logMessage += "\nLegacy_Provider_Id " + newOpp.Legacy_Provider_Id;
                    logMessage += "\nLegacy_Opportunity_Id " + newOpp.Legacy_Opportunity_Id;
                    logMessage += "\n====================================================";

                    i++;
                }

                // Write the log file
                string strFileName = "SB-Opp-Ingest-Log.txt";

                try
                {
                    FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), strFileName), FileMode.Create, FileAccess.Write);
                    StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                    objStreamWriter.WriteLine(logMessage);
                    objStreamWriter.Close();
                    objFilestream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }


                // Write the bad reference log file
                string refFileName = "SB-Opp-Ingest-Bad-Refs.txt";

                try
                {
                    FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), refFileName), FileMode.Create, FileAccess.Write);
                    StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                    objStreamWriter.WriteLine(badRefsMessage);
                    objStreamWriter.Close();
                    objFilestream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }
            }

            return View();
        }

        public async Task<IActionResult> AddStates()
        {
            // if program record has yes in "locationsdetailsavailable", pull data from opps and use that for states prog delivery
            // HOLD OFF ON THIS




            // Update states of program delivery on program and org records
            foreach (ProgramModel prog in _db.Programs)
            {
                //ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == opp.Program_Id);
                List<OpportunityModel> opps = _db.Opportunities.Where(e => e.Program_Id == prog.Id).ToList();
                OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == prog.Organization_Id);

                //await UpdateStatesOfProgramDelivery(prog, opps);
                // Update Program
                string newStateList = "";
                int num = 0;

                List<string> states = new List<string>();

                Console.WriteLine("Program Name: " + prog.Program_Name + "=====================");

                if(opps.Count > 0)
                {
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

                //await _db.SaveChangesAsync();

                //await UpdateOrgStatesOfProgramDelivery(org);

                Console.WriteLine("Setting Program states for " + prog.Program_Name + " to " + newStateList);
                

                Console.WriteLine("End Program =======================================");

                //await _db.SaveChangesAsync();
            }

            /*This is the right save*/
            await _db.SaveChangesAsync();

            foreach (OrganizationModel org in _db.Organizations)
            {
                Console.WriteLine("Org Name: " + org.Name + "=====================");
                // Get all programs from org
                List<ProgramModel> progs = _db.Programs.Where(e => e.Organization_Id == org.Id).ToList();

                List<string> states2 = new List<string>();


                foreach (ProgramModel p in progs)
                {
                    string progStates = "";
                    progStates = p.States_Of_Program_Delivery;
                    //Console.WriteLine("progStates: " + progStates);

                    // Split out each programs states of program delivery, and add them to the states array
                    progStates = progStates.Replace(" ", "");
                    string[] splitStates = progStates.Split(",");

                    foreach (string s in splitStates)
                    {
                        if (s != "" && s != " ")
                        {
                            //Console.WriteLine("s in splitstates: " + s);
                            bool found = false;

                            foreach (string st in states2)
                            {
                                if (s == st)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (found == false)
                            {
                                states2.Add(s);
                            }
                        }
                    }
                }

                // Sort states alphabetically
                states2.Sort();

                // Go through and remove duplicate entries
                int count = 0;
                string orgStates = "";

                foreach (string s in states2)
                {
                    //Console.WriteLine("Checking state s: " + s);

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

                Console.WriteLine("Setting Org States For " + org.Name + " to " + orgStates);

                // Do the same thing for orgs
            }


            /*This is the right save*/
            await _db.SaveChangesAsync();

            return View("IngestOpportunities");
        }

        public async Task<IActionResult> RectifyRecords()
        {
            var orgs = _db.Organizations;
            var progs = _db.Programs;
            var opps = _db.Opportunities;

            // Look for disabled orgs, handle disabling children
            foreach(var org in orgs)
            {
                if(org.Is_Active == false)
                {
                    // Get child programs
                    var relatedProgs = progs.Where(x => x.Organization_Id == org.Id);

                    // Set each child program to disabled
                    foreach (var p in relatedProgs)
                    {
                        p.Is_Active = false;

                        // Look for child opps, set them to disabled
                        var relatedOpps = opps.Where(x => x.Organization_Id == org.Id);

                        // Set each child opp to disabled
                        foreach (var o in relatedOpps)
                        {
                            o.Is_Active = false;
                        }
                    }
                }
            }

            // Look for disabled programs, disable child opps
            foreach (var p in progs)
            {
                if (p.Is_Active == false)
                {
                    // Look for child opps, set them to disabled
                    var relatedOpps = opps.Where(x => x.Program_Id == p.Id);

                    // Set each child opp to disabled
                    foreach (var o in relatedOpps)
                    {
                        o.Is_Active = false;
                    }
                }
            }

            await _db.SaveChangesAsync();

            return View("IngestOpportunities");
        }

        public async void UpdateOrgStatesOfProgramDelivery(OrganizationModel org)
        {
            // Get all programs from org
            List<ProgramModel> progs = _db.Programs.Where(e => e.Organization_Id == org.Id).ToList();

            List<string> states = new List<string>();


            foreach (ProgramModel p in progs)
            {
                string progStates = "";
                progStates = p.States_Of_Program_Delivery;
                //Console.WriteLine("progStates: " + progStates);

                // Split out each programs states of program delivery, and add them to the states array
                progStates = progStates.Replace(" ", "");
                string[] splitStates = progStates.Split(",");

                foreach (string s in splitStates)
                {
                    if (s != "" && s != " ")
                    {
                        //Console.WriteLine("s in splitstates: " + s);
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
                //Console.WriteLine("Checking state s: " + s);

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

            await _db.SaveChangesAsync();
        }

        public async void UpdateStatesOfProgramDelivery(ProgramModel prog, List<OpportunityModel> opps)
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

            prog.States_Of_Program_Delivery = newStateList;

            await _db.SaveChangesAsync();
        }

        public int FindProgramParticipationPopulationIdFromName(string name)
        {
            int id = -1;

            ParticipationPopulation pop = _db.ParticipationPopulations.FirstOrDefault(e => e.Name.ToLower().Equals(name.ToLower()));

            //Console.WriteLine("pop.Name: " + pop.Name);
            //Console.WriteLine("name: " + name);

            if (pop != null)
            {
                id = pop.Id;
            }

            return id;
        }

        public string GetProgramServiceNameFromId(int id)
        {
            string name = "";

            Service service = _db.Services.FirstOrDefault(e => e.Id == id);

            if(service != null)
            {
                name = service.Name;
            }

            return name;
        }

        public int FindProgramServiceIdFromName(string name)
        {
            int id = -1;

            Service service = _db.Services.FirstOrDefault(e => e.Name.ToLower().Equals(name.ToLower()));

            //Console.WriteLine("pop.Name: " + pop.Name);
            //Console.WriteLine("name: " + name);

            if (service != null)
            {
                id = service.Id;
            }

            Console.WriteLine("========Returning value from FindProgramServiceIdFromName of " + id);

            return id;
        }

        public int FindProgramDeliveryMethodIdFromName(string name)
        {
            int id = -1;

            DeliveryMethod method = _db.DeliveryMethods.FirstOrDefault(e => e.Name.ToLower().Equals(name.ToLower()));

            //Console.WriteLine("pop.Name: " + pop.Name);
            //Console.WriteLine("name: " + name);

            if (method != null)
            {
                id = method.Id;
            }

            //Console.WriteLine("========Returning value from FindProgramDeliveryMethodIdFromName of " + id);

            return id;
        }

        [HttpGet]
        public IActionResult DeleteAuditRecords()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DeleteAllAuditRecords()
        {
            if (_db.Audits.Any())
            {
                var audits = new List<AuditModel>();

                foreach (var audit in _db.Audits)
                {
                    audits.Add(audit);
                }

                if (audits.Count > 0)
                {
                    _db.Audits.RemoveRange(audits);
                    _db.SaveChanges();
                }
            }

            return View();
        }

        private void DeleteAllMOUs()
        {
            if (_db.Mous.Any())
            {
                var mous = new List<MouModel>();

                foreach (var mou in _db.Mous)
                {
                    mous.Add(mou);
                }

                if (mous.Count > 0)
                {
                    _db.Mous.RemoveRange(mous);
                    _db.SaveChanges();
                }
            }
        }

        private void DeleteAllOrganizations()
        {
            if(_db.Organizations.Any())
            {
                List<OrganizationModel> orgs = new List<OrganizationModel>();

                foreach (OrganizationModel org in _db.Organizations)
                {
                    orgs.Add(org);
                }

                if(orgs.Count > 0)
                {
                    _db.Organizations.RemoveRange(orgs);
                    _db.SaveChanges();
                }
            }
        }

        private void DeleteAllPrograms()
        {
            if (_db.Programs.Any())
            {
                List<ProgramModel> progs = new List<ProgramModel>();

                foreach (ProgramModel prog in _db.Programs)
                {
                    progs.Add(prog);
                }

                if (progs.Count > 0)
                {
                    _db.Programs.RemoveRange(progs);
                    _db.SaveChanges();
                }
            }
        }

        private void DeleteAllOpportunities()
        {
            if (_db.Opportunities.Any())
            {
                List<OpportunityModel> opps = new List<OpportunityModel>();

                foreach (OpportunityModel opp in _db.Opportunities)
                {
                    opps.Add(opp);
                }

                if(opps.Count > 0)
                {
                    _db.Opportunities.RemoveRange(opps);
                    _db.SaveChanges();
                }
            }
        }

        private void DeleteAllOpportunityGroups()
        {
            if (_db.OpportunityGroups.Any())
            {
                var groups = new List<OpportunityGroupModel>();

                foreach (var group in _db.OpportunityGroups)
                {
                    groups.Add(group);
                }

                if (groups.Count > 0)
                {
                    _db.OpportunityGroups.RemoveRange(groups);
                    _db.SaveChanges();
                }
            }
        }

        private void DeleteAllParticipationPopulations()
        {
            if (_db.ProgramParticipationPopulation.Any())
            {
                List<ProgramParticipationPopulation> pps = new List<ProgramParticipationPopulation>();

                foreach (ProgramParticipationPopulation pp in _db.ProgramParticipationPopulation)
                {
                    pps.Add(pp);
                }

                if (pps.Count > 0)
                {
                    _db.ProgramParticipationPopulation.RemoveRange(pps);
                    _db.SaveChanges();
                }
            }
        }

        private void DeleteAllPendingParticipationPopulations()
        {
            if (_db.PendingProgramParticipationPopulation.Any())
            {
                List<PendingProgramParticipationPopulation> pps = new List<PendingProgramParticipationPopulation>();

                foreach (PendingProgramParticipationPopulation pp in _db.PendingProgramParticipationPopulation)
                {
                    pps.Add(pp);
                }

                if (pps.Count > 0)
                {
                    _db.PendingProgramParticipationPopulation.RemoveRange(pps);
                    _db.SaveChanges();
                }
            }
        }

        private void DeleteAllJobFamilies()
        {
            if (_db.ProgramJobFamily.Any())
            {
                List<ProgramJobFamily> jfs = new List<ProgramJobFamily>();

                foreach (ProgramJobFamily jf in _db.ProgramJobFamily)
                {
                    jfs.Add(jf);
                }

                if (jfs.Count > 0)
                {
                    _db.ProgramJobFamily.RemoveRange(jfs);
                    _db.SaveChanges();
                }
            }
        }

        private void DeleteAllPendingJobFamilies()
        {
            if (_db.PendingProgramJobFamily.Any())
            {
                List<PendingProgramJobFamily> jfs = new List<PendingProgramJobFamily>();

                foreach (PendingProgramJobFamily jf in _db.PendingProgramJobFamily)
                {
                    jfs.Add(jf);
                }

                if (jfs.Count > 0)
                {
                    _db.PendingProgramJobFamily.RemoveRange(jfs);
                    _db.SaveChanges();
                }
            }
        }
    }
}
