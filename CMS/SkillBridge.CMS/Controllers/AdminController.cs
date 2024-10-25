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
using SkillBridge.CMS.ViewModel;
using SkillBridge.Business.Command;
using Z.EntityFramework.Plus;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Query;
using SkillBridge.Business.Util.Ingest;
using Taku.Core.Global;
using SkillBridge.Business.Query.DataDownload;
using Taku.Core.Command;

namespace SkillBridge.CMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(ILogger<AdminController> _logger,
        RoleManager<IdentityRole> _roleManager,
        UserManager<ApplicationUser> _userManager,
        ApplicationDbContext _db,
        IEmailSender _emailSender,
        IRenderDropDownJsFileCommand _renderDropDownJsFileCommand,
        IDropdownDataQuery _dropdownDataQuery,
        ILocationDataQuery _locationDataQuery,
        ISerializeObjectCommand _serializeObjectCommand) : Controller
    {
        public IActionResult Index()
        {
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
            if (ModelState.IsValid)
            {
                var identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                var result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Admin");
                }

                foreach (var error in result.Errors)
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

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
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

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
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
            var newJson = "var orgs = { data: ";

            var i = 0;

            try
            {
                foreach (var prog in progs)
                {
                    newJson += "{";

                    var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);

                    var urlToDisplay = org != null ? org.Organization_Url : "";

                    newJson += "\"PROGRAM\": " + prog.ProgramName + ",";
                    newJson += "\"URL\": " + urlToDisplay + ",";
                    newJson += "\"OPPORTUNITY_TYPE\": " + prog.OpportunityType + ",";
                    newJson += "\"DELIVERY_METHOD\": " + prog.DeliveryMethod + ",";
                    newJson += "\"PROGRAM_DURATION\": " + prog.ProgramDuration + ",";
                    newJson += "\"STATES\": " + prog.StatesOfProgramDelivery + ",";
                    newJson += "\"NATIONWIDE\": " + prog.Nationwide + ",";
                    newJson += "\"ONLINE\": " + prog.Online + ",";
                    newJson += "\"COHORTS\": " + prog.SupportCohorts + ",";
                    newJson += "\"JOB_FAMILY\": " + prog.JobFamily + ",";
                    newJson += "\"LOCATION_DETAILS_AVAILABLE\": " + prog.LocationDetailsAvailable;

                    newJson += "}";

                    // Add comma if this isn't the last object
                    if (i < progs.Count() - 1)
                    {
                        newJson += ",";
                        i++;
                    }
                }
            }
            catch (Exception e)
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
            var newJson = "var locations = { data: ";

            var i = 0;

            foreach (var opp in opps)
            {
                newJson += "{";

                newJson += "\"ID\": " + opp.Id + ",";
                newJson += "\"GROUPID\": " + opp.GroupId + ",";
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
            var newJson = "var spouses = { data: ";

            var i = 0;

            foreach (var prog in progs)
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

                var urlToDisplay = org != null ? org.Organization_Url : "";
                /*                 *
                "PROGRAM": "The Park Clinic for Plastic Surgery - Healthcare Admin Internship",
                "URL": "https://www.theparkplasticsurgery.com/",
                "NATIONWIDE": 0,
                "ONLINE": 0,
                "DELIVERY_METHOD": "In-person",
                "STATES": "AL"
                */

                newJson += "\"PROGRAM\": " + prog.ProgramName + ",";
                newJson += "\"URL\": " + urlToDisplay + ",";
                newJson += "\"NATIONWIDE\": " + prog.Nationwide + ",";
                newJson += "\"ONLINE\": " + prog.Online + ",";
                newJson += "\"DELIVERY_METHOD\": " + prog.DeliveryMethod + ",";
                newJson += "\"STATES\": " + prog.StatesOfProgramDelivery;

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
            var orgs = _db.Organizations.ToList();

            foreach (var org in orgs)
            {
                UpdateOrgStatesOfProgramDeliveryNonAsync(org);
            }
        }

        private void UpdateOrgStatesOfProgramDeliveryNonAsync(OrganizationModel org)
        {
            // Get all programs from org
            List<ProgramModel> progs = _db.Programs.Where(e => e.OrganizationId == org.Id).ToList();

            var states = new List<string>();

            foreach (var p in progs)
            {
                var progStates = "";
                progStates = p.StatesOfProgramDelivery;
                Console.WriteLine("progStates: " + progStates);

                // Split out each programs states of program delivery, and add them to the states array
                progStates = progStates.Replace(" ", "");
                var splitStates = progStates.Split(",");

                foreach (var s in splitStates)
                {
                    if (s != "" && s != " ")
                    {
                        Console.WriteLine("s in splitstates: " + s);
                        var found = false;

                        foreach (var st in states)
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
            var count = 0;
            var orgStates = "";

            foreach (var s in states)
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
            //string newJson = "";
            var newJson = new StringBuilder("");

            var i = 0;

            var progCount = progs.ToList().Count;

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
                    var relatedProgs = progs.Where(m => m.OrganizationId == org.Id).ToList();

                    var newProgramIds = "";
                    var progInc = 0;

                    // checking for prog active status underneath org is how its set up right now, but maybe shouldnt be in the future

                    foreach (var prog in relatedProgs)
                    {
                        //string add = progInc == 0 ? prog.Legacy_Program_Id.ToString() : " " + prog.Legacy_Program_Id;
                        var add = progInc == 0 ? prog.Id.ToString() : " " + prog.Id;
                        newProgramIds += add;
                        progInc++;
                    }

                    newProgramIds.Trim();

                    if (relatedProgs.Count > 0)
                    {
                        //string newDeliveryMethod = GetDeliveryMethodForProg(relatedProgs0]);   // captured as a input on the create program page
                        //string newProgramDuration = GetProgramDurationForProg(relatedProgs0]); // ^^
                        //string newCohorts = GetCohortsForProg(relatedProgs0]);     // supports

                        var newDeliveryMethod = GetDeliveryMethodListForOrg(relatedProgs);   // captured as a input on the create program page
                        var newProgramDuration = GetProgramDurationListForOrg(relatedProgs); // ^^
                                                                                                //string newCohorts = GetCohortsAggregateForOrg(relatedProgs);     // supports
                        var opportunityTypes = GetOpportunityTypesListForOrg(relatedProgs);
                        var jobFamilies = GetJobFamiliesListForOrg(relatedProgs);

                        var newCohorts = "No";
                        foreach (var p in relatedProgs)
                        {
                            if (p.Nationwide)
                            {
                                newCohorts = "Yes";
                            }
                        }

                        var nationwide = false;
                        foreach (var p in relatedProgs)
                        {
                            if (p.Nationwide)
                            {
                                nationwide = true;
                            }
                        }

                        var online = false;
                        foreach (var p in relatedProgs)
                        {
                            if (p.Online)
                            {
                                online = true;
                            }
                        }

                        var locationDetailsAvailable = false;
                        foreach (var p in relatedProgs)
                        {
                            if (p.LocationDetailsAvailable)
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
                        newJson.Append("\"NATIONWIDE\":" + (nationwide ? 1 : 0) + ",");
                        newJson.Append("\"ONLINE\":" + (online ? 1 : 0) + ",");
                        newJson.Append("\"COHORTS\":\"" + newCohorts + "\",");
                        newJson.Append("\"JOB_FAMILY\":\"" + jobFamilies + "\",");
                        newJson.Append("\"LOCATION_DETAILS_AVAILABLE\":" + (locationDetailsAvailable ? 1 : 0));

                        newJson.Append("}");

                        i++;

                        /*string newDeliveryMethod = GetDeliveryMethodForProg(relatedProgs0]);   // captured as a input on the create program page
                        string newProgramDuration = GetProgramDurationForProg(relatedProgs0]); // ^^
                        string newCohorts = GetCohortsForProg(relatedProgs0]);     // supports

                        if (i != 0)
                        {
                            newJson.Append(",");
                        }

                        // USE AGGREGATES OF ALL PROGRAMS

                        newJson.Append("{");

                        newJson.Append("\"PROGRAM STATUS\": \"" + (relatedProgs0].Program_Status ? "Active" : "Closed (" + relatedProgs0].Date_Deactivated + ")") + "\",");
                        newJson.Append("\"PROVIDER UNIQUE ID\": " + org.Legacy_Provider_Id + ",");
                        newJson.Append("\"PROGRAM UNIQUE ID\": \"" + newProgramIds + "\",");
                        newJson.Append("\"PROGRAM\": \"" + org.Name + "\",");
                        newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                        newJson.Append("\"OPPORTUNITY_TYPE\": \"" + relatedProgs0].Opportunity_Type + "\",");
                        newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                        newJson.Append("\"PROGRAM_DURATION\": \"" + newProgramDuration + "\",");
                        newJson.Append("\"STATES\": \"" + relatedProgs0].States_Of_Program_Delivery + "\",");
                        newJson.Append("\"NATIONWIDE\":" + (relatedProgs0].Nationwide == true ? 1 : 0) + ",");
                        newJson.Append("\"ONLINE\":" + (relatedProgs0].Online == true ? 1 : 0) + ",");
                        newJson.Append("\"COHORTS\":\"" + newCohorts + "\",");
                        newJson.Append("\"JOB_FAMILY\":\"" + relatedProgs0].JobFamily + "\",");
                        newJson.Append("\"LOCATION_DETAILS_AVAILABLE\":" + (relatedProgs0].Location_Details_Available == true ? 1 : 0));

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
                                newJson.Append("\"STATES\": \"" + prog.StatesOfProgramDelivery + "\",");
                                newJson.Append("\"NATIONWIDE\":" + (prog.Nationwide == true ? 1 : 0) + ",");
                                newJson.Append("\"ONLINE\":" + (prog.Online == true ? 1 : 0) + ",");
                                newJson.Append("\"COHORTS\":\"" + newCohorts + "\",");
                                newJson.Append("\"JOB_FAMILY\":\"" + prog.JobFamily + "\",");
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
            string newJson = "var orgs = { data: " + JsonConvert.SerializeObject(programs, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) + "]};";*/

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "application/json", "AF-Org-" + DateTime.Today.ToString("MM-dd-yy") + ".json");
        }

        public string GetDeliveryMethodListForOrg(List<ProgramModel> relatedProgs)
        {
            var returnString = "";
            var i = 0;

            var dms = new List<string>();
            var dmids = new List<int>();

            //bool isScottsdale = false;

            foreach (var p in relatedProgs)
            {
                var pdm = _db.ProgramDeliveryMethod.AsNoTracking().Where(x => x.Program_Id == p.Id).ToList();

                foreach (var d in pdm)
                {
                    dmids.Add(d.Delivery_Method_Id);
                }

                /*if(p.Organization_Id == 121)
                {
                    isScottsdale = true;
                }*/
            }

            // Get unique values
            var uniqueDmids = dmids.Distinct().ToList();

            /*if(isScottsdale)
            {
                foreach(int j in uniqueDmids)
                {
                    Console.WriteLine("uniqueDmids j: " + j);
                }
            }*/

            // Combine values into a comma separated string
            foreach (var dm in uniqueDmids)
            {
                var val = "";
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
            var returnString = "";
            var i = 0;

            var pds = new List<string>();

            // Get a list of items from the programs
            foreach (var p in relatedProgs)
            {
                pds.Add(p.ProgramDuration.ToString());
            }

            // Get unique values
            var uniquePds = pds.Distinct().ToList();

            // Combine values into a comma separated string
            foreach (var pd in uniquePds)
            {
                var val = "";

                switch (pd)
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
            var returnString = "";
            var i = 0;

            var ots = new List<string>();

            // Get a list of items from the programs
            foreach (var p in relatedProgs)
            {
                ots.Add(p.OpportunityType);
            }

            // Get unique values
            var uniqueOts = ots.Distinct().ToList();

            // Combine values into a comma separated string
            foreach (var ot in uniqueOts)
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
            var returnString = "";
            var i = 0;

            var jfs = new List<string>();

            foreach (var p in relatedProgs)
            {
                var pjfs = _db.ProgramJobFamily.Where(x => x.Program_Id == p.Id).ToList();

                foreach (var jf in pjfs)
                {
                    var fam = _db.JobFamilies.FirstOrDefault(x => x.Id == jf.Job_Family_Id);

                    jfs.Add(fam.Name);
                }
            }

            // Get unique values
            var uniqueJfs = jfs.Distinct().ToList();

            // Combine values into a comma separated string
            foreach (var j in uniqueJfs)
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
            //string newJson = "";
            var newJson = new StringBuilder("");

            var i = 0;

            var progCount = progs.ToList().Count;

            try
            {
                foreach (var prog in progs)
                {
                    //var org = _db.Organizations.AsNoTracking().FromCache().SingleOrDefault(x => x.Id == prog.Organization_Id);

                    var newProgramDuration = GetProgramDurationForProg(prog);

                    if (prog.IsActive)
                    {
                        var org = orgs.FromCache().SingleOrDefault(x => x.Id == prog.OrganizationId);

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
                            newJson.Append("\"Provider Unique ID\": " + prog.OrganizationId + ",");
                            newJson.Append("\"Program Unique ID\": " + prog.Id + ",");
                            newJson.Append("\"Program Status\": \"" + (prog.IsActive ? "Active" : "Closed (" + prog.DateDeactivated + ")") + "\",");
                            newJson.Append("\"Program & Organization\": \"" + prog.OrganizationName + "\",");
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
            //string newJson = "";
            var newJson = new StringBuilder("");

            var i = 0;

            var oppCount = opps.ToList().Count;

            try
            {
                foreach (var opp in opps)
                {
                    var prog = progs.AsNoTracking().SingleOrDefault(x => x.Id == opp.Program_Id);
                    var org = orgs.AsNoTracking().SingleOrDefault(x => x.Id == opp.Organization_Id);

                    var newProgramDuration = GetProgramDurationForProg(prog);

                    if (org.Is_Active)
                    {
                        if (prog.IsActive)
                        {
                            if (opp.Is_Active)
                            {
                                // Add comma if this isn't the last object
                                if (i != 0)
                                {
                                    newJson.Append(",");
                                }

                                newJson.Append("{");

                                newJson.Append("\"PROGRAM STATUS\":\"" + (prog.IsActive ? "Active" : "Closed (" + prog.DateDeactivated.ToString("MM/dd/yyyy") + ")") + "\",");
                                //newJson.Append("\"PROVIDER UNIQUE ID\":" + opp.Legacy_Provider_Id + ",");
                                //newJson.Append("\"PROGRAM UNIQUE ID\":" + opp.Legacy_Program_Id + ",");
                                newJson.Append("\"PROVIDER UNIQUE ID\":" + opp.Organization_Id + ",");
                                newJson.Append("\"PROGRAM UNIQUE ID\":" + opp.Program_Id + ",");
                                newJson.Append("\"ID\":" + opp.Id + ",");
                                newJson.Append("\"GROUPID\":" + opp.GroupId + ",");
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
            string newJson = "var orgs = { data: " + JsonConvert.SerializeObject(programs, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) + "]};";*/

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
            //string newJson = "";
            var newJson = new StringBuilder("");

            var i = 0;

            var spouseOrgs = new List<int>();

            try
            {
                foreach (var prog in progs)
                {
                    if (prog.ForSpouses && prog.IsActive)
                    {
                        var org = orgs.AsNoTracking().SingleOrDefault(x => x.Id == prog.OrganizationId);

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

                        if (org.Is_Active)
                        {
                            var newProgramDuration = GetProgramDurationForProg(prog);

                            if (i != 0)
                            {
                                newJson.Append(",");
                            }

                            newJson.Append("{");

                            var newDeliveryMethod = GetDeliveryMethodForProg(prog);

                            newJson.Append("\"PROGRAM STATUS\":\"" + (prog.IsActive ? "Active" : "Closed (" + prog.DateDeactivated.ToString("MM/dd/yyyy") + ")") + "\",");
                            //newJson.Append("\"PROVIDER UNIQUE ID\":" + prog.Legacy_Provider_Id + ",");
                            //newJson.Append("\"PROGRAM UNIQUE ID\":" + prog.Legacy_Program_Id + ",");
                            newJson.Append("\"PROVIDER UNIQUE ID\":" + prog.OrganizationId + ",");
                            newJson.Append("\"PROGRAM UNIQUE ID\":" + prog.Id + ",");
                            newJson.Append("\"PROGRAM\": \"" + org.Name + "\",");
                            newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                            newJson.Append("\"NATIONWIDE\":" + (prog.Nationwide ? 1 : 0) + ",");
                            newJson.Append("\"ONLINE\":" + (prog.Online ? 1 : 0) + ",");
                            newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                            newJson.Append("\"STATES\": \"" + prog.StatesOfProgramDelivery + "\"");

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
            //string newJson = "var orgs = { data: ";
            var newJson = new StringBuilder("var orgs = { data: ");

            var i = 0;

            var progCount = progs.ToList().Count;

            try
            {
                foreach (var org in orgs)
                {
                    if (org.Is_Active)
                    {
                        var hasActiveProgram = false;

                        //foreach(var org in orgs)
                        //{
                        var subProgs = progs.Where(x => x.OrganizationId == org.Id).ToList();
                        var deliveryMethods = new List<string>();
                        var opportunityTypes = new List<string>();
                        var durations = new List<int>();

                        var jobFamilies = new List<string>();
                        var states = new List<string>();

                        // Pull delivery method data out of database
                        var newDeliveryMethod = "";
                        var newOpportunityType = "";
                        var newProgramDuration = "";
                        var newCohorts = "";
                        var newJobFamilies = "";
                        var newStatesOfProgramDelivery = "";

                        var nationwide = false;
                        var online = false;
                        var location = false;

                        var newName = "";
                        var progName = "";

                        foreach (ProgramModel prog in subProgs)
                        {
                            if (prog.IsActive)
                            {
                                // If has singular active program at least, we include it in the export.
                                hasActiveProgram = true;

                                // Delivery Methods
                                // Check for duplicates
                                var deliveryMethodExists = false;
                                foreach (var dm in deliveryMethods)
                                {
                                    if (dm == prog.DeliveryMethod)
                                    {
                                        deliveryMethodExists = true;
                                    }
                                }

                                // If duration doesnt exist, add it to list
                                if (!deliveryMethodExists)
                                {
                                    deliveryMethods.Add(prog.DeliveryMethod);
                                }

                                // Opportunity Type
                                // Check for duplicates
                                var opportunityTypeExists = false;
                                foreach (var ot in opportunityTypes)
                                {
                                    if (ot == prog.OpportunityType)
                                    {
                                        opportunityTypeExists = true;
                                    }
                                }

                                // If duration doesnt exist, add it to list
                                if (!opportunityTypeExists)
                                {
                                    opportunityTypes.Add(prog.OpportunityType);
                                }

                                // Durations
                                // Check for duplicates
                                var durationExists = false;
                                foreach (var d in durations)
                                {
                                    if (d == prog.ProgramDuration)
                                    {
                                        durationExists = true;
                                    }
                                }

                                // If duration doesnt exist, add it to list
                                if (!durationExists)
                                {
                                    durations.Add(prog.ProgramDuration);
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

                                Console.WriteLine("checking program states for program: " + prog.ProgramName);
                                var stateSplit = prog.StatesOfProgramDelivery.Split(", ");
                                //foreach (string s in states)
                                //{
                                //Console.WriteLine("-s: " + s);
                                foreach (string ss in stateSplit)
                                {
                                    Console.WriteLine("-ss: " + ss);
                                    var stateExists = false;

                                    foreach (var s in states)
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

                                if (prog.Nationwide)
                                {
                                    nationwide = true;
                                }

                                if (prog.Online)
                                {
                                    online = true;
                                }

                                if (prog.LocationDetailsAvailable)
                                {
                                    location = true;
                                }

                                progName = prog.ProgramName;
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
                            newJson.Append("\"STATES\": \"" + newStatesOfProgramDelivery/*prog.StatesOfProgramDelivery*/ + "\",");
                            newJson.Append("\"NATIONWIDE\": " + (nationwide ? 1 : 0) + ",");
                            newJson.Append("\"ONLINE\": " + (online ? 1 : 0) + ",");
                            newJson.Append("\"COHORTS\": \"" + newCohorts + "\",");
                            newJson.Append("\"JOB_FAMILY\": \"" + newJobFamilies + "\",");
                            newJson.Append("\"LOCATION_DETAILS_AVAILABLE\": " + (location ? 1 : 0));

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
            string newJson = "var orgs = { data: " + JsonConvert.SerializeObject(programs, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) + "]};";*/

            return File(Encoding.UTF8.GetBytes(newJson.ToString()), "text/plain", "organizationsData.txt");
        }

        private string GetJobFamiliesListForProg(ProgramModel prog)
        {
            var jfs = "";

            var pjf = _db.ProgramJobFamily.Where(x => x.Program_Id == prog.Id).ToList();

            Console.WriteLine("==pjf.Count for " + prog.ProgramName + " = " + pjf.Count);

            var count = pjf.Count;
            var i = 0;

            foreach (var jf in pjf)
            {
                Console.WriteLine("jf: " + jf.Job_Family_Id);
                var fam = _db.JobFamilies.FirstOrDefault(x => x.Id == jf.Job_Family_Id);

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

            return jfs;
        }

        private string GetDeliveryMethodForProg(ProgramModel prog)
        {
            var dm = "";

            Console.WriteLine("GetDeliveryMethodForProg prog: " + prog.ProgramName + " - #" + prog.Id);

            var pdm = _db.ProgramDeliveryMethod.AsNoTracking().Where(x => x.Program_Id == prog.Id).ToList();

            var count = 0;

            foreach (var p in pdm)
            {
                if (count != 0)
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
            var pd = "";

            Console.WriteLine("Program Name: " + prog.ProgramName + " -- Duration: " + prog.ProgramDuration);
            switch (prog.ProgramDuration)
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
            var ot = "";
            var i = 0;
            var length = list.Count;

            foreach (var o in list)
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
            var pd = "";
            var i = 0;
            var length = list.Count;

            //Console.WriteLine("Program Name: " + prog.ProgramName + " -- Duration: " + prog.Program_Duration);
            foreach (var d in list)
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

                if (i < length - 1)
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
            var c = "";

            c = prog.SupportCohorts == true ? "Yes" : "No";

            return c;
        }

        [HttpGet]

        public IActionResult DownloadOrganizationsCSV()
        {
            var progs = _db.Programs;

            try
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("PROGRAM|URL|OPPORTUNITY_TYPE|DELIVERY_METHOD|PROGRAM_DURATION|STATES|NATIONWIDE|ONLINE|COHORTS|JOB_FAMILY|LOCATION_DETAILS_AVAILABLE");

                foreach (var prog in progs)
                {
                    var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);
                    var urlToDisplay = org != null ? org.Organization_Url : "";

                    stringBuilder.AppendLine($"" + $"{prog.ProgramName}|{urlToDisplay}|{prog.OpportunityType}|{prog.DeliveryMethod}|{prog.ProgramDuration}|{prog.StatesOfProgramDelivery}|{prog.Nationwide}|{prog.Online}|{prog.SupportCohorts}|{prog.JobFamily}|{prog.LocationDetailsAvailable}");
                }

                return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", "orgs-" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
            }
            catch
            {
                return Error();
            }
        }

        [HttpGet]

        public IActionResult DownloadNewLocData()
        {
            var locations = _locationDataQuery.Get();
            _serializeObjectCommand.Execute(locations, out var locFile, true);
            // hack the json case
            locFile = locFile.Replace("LOCATIONS", "locations");
            return File(Encoding.UTF8.GetBytes(locFile), "application/json", "locations.json");
        }

        [HttpGet]

        public IActionResult DownloadNewLocData_Old()  // Generates the list of JSON data that will be used for the live site locations page
        {
            var orgs = _db.Organizations.AsNoTracking();
            var progs = _db.Programs.AsNoTracking();
            var opps = _db.Opportunities.AsNoTracking();

            // Generate the string of JSON
            //string newJson = "var locations = { data: ";
            var newJson = new StringBuilder("{  \"locations\":");

            var i = 0;

            var oppCount = opps.ToList().Count;
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

                        if (org.Is_Active && prog.IsActive)
                        {
                            var newProgramDuration = GetProgramDurationForProg(prog);
                            var newDeliveryMethod = "";

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

                            if (!org.Name.Equals(prog.ProgramName, StringComparison.OrdinalIgnoreCase))
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

        public async Task<IActionResult> DownloadDropdownData()  // Generates the list of JSON data that will be used for the live site locations page
        {
            // new code
            var data = await _dropdownDataQuery.Get();
            _renderDropDownJsFileCommand.Execute(data, out var ddFile);
            return File(Encoding.UTF8.GetBytes(ddFile), "application/json", "dropdown-data.js");
        }

        [HttpGet]

        public async Task<IActionResult> UpdateProgramServiceValues()
        {
            // Find programs missing services values
            var flaggedProgs = _db.Programs.Where(m => string.IsNullOrEmpty(m.ServicesSupported)).ToList();

            Console.WriteLine("flaggedProgs.Count: " + flaggedProgs.Count); // Should be 1058

            var numWithServicesAlready = 0;
            var numWithNoService = 0;

            foreach (ProgramModel prog in flaggedProgs)
            {
                // Try to find a value for this program in the programsservice table
                var progServices = _db.ProgramService.Where(m => m.Program_Id == prog.Id).ToList();

                // If we have services defined, pull the optimized string to the program
                if (progServices.Count > 0)
                {
                    var optServices = GetServiceListForProg(prog);
                    prog.ServicesSupported = optServices;
                    Console.WriteLine("Service defined for program: " + prog.ProgramName + ", setting optimized value to " + optServices);
                    _db.Programs.Update(prog);
                    numWithServicesAlready++;
                }
                else   // Create the appropriate service entries for this program
                {
                    var newService = new ProgramService
                    {
                        Program_Id = prog.Id,
                        Service_Id = 1
                    };

                    _db.ProgramService.Add(newService);

                    var optServices = "All Services";
                    prog.ServicesSupported = optServices;
                    Console.WriteLine("No services defined for program: " + prog.ProgramName + ", setting optimized value to All Services");
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

            var changes = 0;

            foreach (var opp in flaggedOpps)
            {
                // Try to find a value for this program in the programsservice table
                var progServices = _db.ProgramService.Where(m => m.Program_Id == opp.Program_Id).ToList();
                var prog = _db.Programs.SingleOrDefault(m => m.Id == opp.Program_Id);

                // If we have services defined, pull the optimized string to the program
                if (progServices.Count > 0)
                {
                    string optServices = prog.ServicesSupported;
                    opp.Service = optServices;
                    Console.WriteLine("Service defined for program: " + prog.ProgramName + ", setting opp (" + opp.Id + ") optimized value to " + optServices);
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
                    prog.ServicesSupported = optServices;
                    Console.WriteLine("No services defined for program: " + prog.ProgramName + ", setting optimized value to All Services");
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
            var services = "";

            var ps = _db.ProgramService.Where(x => x.Program_Id == prog.Id).ToList();

            Console.WriteLine("==ps.Count for " + prog.ProgramName + " = " + ps.Count);

            var count = ps.Count;
            var i = 0;

            foreach (var s in ps)
            {
                Console.WriteLine("s: " + s.Service_Id);
                var service = _db.MilitaryBranches.FirstOrDefault(x => x.Id == s.Service_Id);

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
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("ID|GROUPID|SERVICE|PROGRAM|INSTALLATION|CITY|STATE|ZIP|EMPLOYERPOC|EMPLOYERPOCEMAIL|DATEPROGRAMINITIATED|DURATIONOFTRAINING|SUMMARYDESCRIPTION|JOBSDESCRIPTION|LOCATIONSOFPROSPECTIVEJOBSBYSTATE|TARGETMOCs|OTHER|MOUs|LAT|LONG|COST|SALARY|NATIONWIDE");

                foreach (var opp in opps)
                {
                    //var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);
                    //string urlToDisplay = org != null ? org.Organization_Url : "";

                    stringBuilder.AppendLine($"" + $"{opp.Id}|{opp.GroupId}|{opp.Service}|{opp.Program_Name}|{opp.Installation}|{opp.City}|{opp.State}|{opp.Zip}|{opp.Employer_Poc_Name}|{opp.Employer_Poc_Email}|{opp.Date_Program_Initiated}|{opp.Training_Duration}|{opp.Summary_Description}|{opp.Jobs_Description}|{opp.Locations_Of_Prospective_Jobs_By_State}|{opp.Target_Mocs}|{opp.Other}|{opp.Mous}|{opp.Lat}|{opp.Long}|{opp.Cost}|{opp.Salary}|{opp.Nationwide}");
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
            StringBuilder newJson = new StringBuilder("var spouses = { data: ");

            int i = 0;
            //int progCount = progs.ToList().Count;   // 690

            //Console.WriteLine("About to export spouse data in foreach loop");
            foreach (ProgramModel prog in progs)
            {
                //Console.WriteLine("-=-=-=-=checking program: " + prog.ProgramName + " for spouse export, i = " + i);
                // We need to check this differently than the others since we don't know the state of every programs spouse offerings...
                //if(prog.For_Spouses)
                //{
                bool wasFound = false;

                    // Make sure we arent duplicating orgs/progs in the data
                    foreach(int id in spouseOrgs)
                    {
                        if(id == prog.Organization_Id)
                        {
                            //Console.WriteLine("==Skipping program: " + prog.ProgramName + " for spouse export, similar prog already found");
                            wasFound = true;
                        }
                    }

                    if(!wasFound)
                    {
                        if(prog.IsActive)
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
                            newJson.Append("\"STATES\": \"" + prog.StatesOfProgramDelivery + "\"");

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

                    newJson += "\"PROGRAM\": \"" + prog.ProgramName + "\",";
                    newJson += "\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",";
                    newJson += "\"NATIONWIDE\": " + (prog.Nationwide == true ? 1 : 0) + ",";
                    newJson += "\"ONLINE\": " + (prog.Online == true ? 1 : 0) + ",";
                    newJson += "\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",";
                    newJson += "\"STATES\": \"" + prog.StatesOfProgramDelivery + "\"";

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
            //string newJson = "";
            var newJson = new StringBuilder("var spouses = { data: ");

            var i = 0;

            var spouseOrgs = new List<int>();

            try
            {
                foreach (var prog in progs)
                {
                    if (prog.IsActive)
                    {
                        if (prog.ForSpouses)
                        {
                            var org = orgs.FromCache().SingleOrDefault(x => x.Id == prog.OrganizationId);

                            if (org.Is_Active)
                            {
                                // Check to make sure we aren't duplicating orgs on the spouse list
                                if (!DoesOrgExistInSpouses(org.Id, spouseOrgs))
                                {
                                    var newName = "";

                                    if (org.Name.Equals(prog.ProgramName))
                                    {
                                        newName = org.Name;
                                    }
                                    else
                                    {
                                        newName = org.Name + " - " + prog.ProgramName;
                                    }

                                    var newProgramDuration = GetProgramDurationForProg(prog);

                                    if (i != 0)
                                    {
                                        newJson.Append(",");
                                    }

                                    newJson.Append("{");

                                    var newDeliveryMethod = GetDeliveryMethodForProg(prog);
                                    Console.WriteLine("newDeliverMethod: " + newDeliveryMethod);

                                    newJson.Append("\"PROGRAM\": \"" + newName + "\",");
                                    newJson.Append("\"URL\": \"" + (org != null ? org.Organization_Url : "") + "\",");
                                    newJson.Append("\"NATIONWIDE\":" + (prog.Nationwide ? 1 : 0) + ",");
                                    newJson.Append("\"ONLINE\":" + (prog.Online ? 1 : 0) + ",");
                                    newJson.Append("\"DELIVERY_METHOD\": \"" + newDeliveryMethod + "\",");
                                    newJson.Append("\"STATES\": \"" + prog.StatesOfProgramDelivery + "\"");

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
            var exists = false;

            foreach (var i in spouseOrgs)
            {
                if (i == id)
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
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("PROGRAM|URL|NATIONWIDE|ONLINE|DELIVERY_METHOD|STATES");

                foreach (var prog in progs)
                {
                    var org = _db.Organizations.SingleOrDefault(x => x.Id == prog.Id);

                    var urlToDisplay = org != null ? org.Organization_Url : "";

                    stringBuilder.AppendLine($"" + $"{prog.ProgramName}|{urlToDisplay}|{prog.Nationwide}|{prog.Online}|{prog.DeliveryMethod}|{prog.StatesOfProgramDelivery}");
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
                var org = orgs.FirstOrDefault(m => m.Id == p.OrganizationId);
                var mou = _db.Mous.FirstOrDefault(m => m.Id == org.Mou_Id);

                Console.WriteLine("Should update program... exp date: " + mou.Expiration_Date + " and create date: " + mou.Creation_Date);
                p.MouExpirationDate = mou.Expiration_Date;
                p.MouCreationDate = mou.Creation_Date;
            }

            _db.SaveChanges();

            // Update Opportunities
            foreach (var o in opps)
            {
                var prog = progs.FirstOrDefault(m => m.Id == o.Program_Id);

                Console.WriteLine("Shoud update opportunity... exp date: " + prog.MouExpirationDate);
                o.Mou_Expiration_Date = prog.MouExpirationDate;
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
            foreach (var o in orgs)
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

            var newSource = "OrgIngestTest14";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = "|",
                MissingFieldFound = null
            };

            var logMessage = "";

            // Remove existing records from Org Table
            //DeleteAllMOUs();
            DeleteAllMOUs();

            using (var reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + newSource + ".csv"))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                var i = 0;

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

                    var tempId = int.Parse(csv.GetField("MOU_ID"));

                    //var mou = _db.Mous.SingleOrDefault(x => x.Legacy_MOU_Id == tempId));

                    Console.Write("Parent Organization Name: " + csv.GetField("Parent Organization Name").ToString());

                    if (csv.GetField("MOU_Parent").ToString() == "TRUE")
                    {
                        // Remove (Historical Import) strings from dates
                        var newCreatedDate = csv.GetField("Date Authorized").ToString();
                        newCreatedDate = newCreatedDate.Replace("Unknown", "");

                        var newExpirationDate = csv.GetField("MOU Expiration Date").ToString();
                        newExpirationDate = newExpirationDate.Replace("Unknown", "");

                        DateTime output1;
                        var isValidDateTime1 = DateTime.TryParse(newCreatedDate, out output1);

                        DateTime output2;
                        var isValidDateTime2 = DateTime.TryParse(newExpirationDate, out output2);

                        var isOSD = true;

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
                var strFileName = "SB-Mou-Ingest-Log.txt";

                try
                {
                    var objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), strFileName), FileMode.Create, FileAccess.Write);
                    var objStreamWriter = new StreamWriter((Stream)objFilestream);
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

            var newSource = "OrgIngestTest14";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = "|",
                MissingFieldFound = null
            };

            var logMessage = "";

            // Remove existing records from Org Table
            //DeleteAllMOUs();
            DeleteAllOrganizations();

            using (var reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + newSource + ".csv"))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                var i = 0;

                while (csv.Read())
                {
                    //var record = csv.GetRecord<OrganizationModel>();
                    // Do something with the record.

                    // Remove (Historical Import) strings from dates
                    var newCreatedDate = csv.GetField("Date Added").ToString();
                    newCreatedDate = newCreatedDate.Replace(" (Historical Import)", "");

                    var newUpdatedDate = csv.GetField("Last Updated").ToString();
                    newUpdatedDate = newUpdatedDate.Replace(" (Historical Import)", "");

                    var newIsActive = false;
                    var newDeactivatedDate = csv.GetField("Program Status").ToString();
                    if (newDeactivatedDate.Contains("Closed"))
                    {
                        if (!newDeactivatedDate.Contains("one-off") && !newDeactivatedDate.Contains("Duplicate Provider"))
                        {
                            if (newDeactivatedDate.Contains("(") && newDeactivatedDate.Contains(")"))
                            {
                                var pFrom = newDeactivatedDate.IndexOf("(") + 1;
                                var pTo = newDeactivatedDate.LastIndexOf(")");

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

                    var newOrgType = 0;
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
                        if (csv.GetField("Type") == "Profit")
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

                    var newOrg = new OrganizationModel
                    {
                        Name = csv.GetField("Organization Name"),
                        Parent_Organization_Name = csv.GetField("Parent Organization Name"),
                        //Name = csv.GetField("Parent Organization Name"),
                        Poc_First_Name = csv.GetField("Admin POC First Name 1"),
                        Poc_Last_Name = csv.GetField("Admin POC Last Name 1"),
                        Poc_Email = csv.GetField("Admin POC Email Address 1"),
                        Poc_Phone = csv.GetField("Admin POC Phone Number 1"),
                        Date_Deactivated = newDeactivatedDate != "" ? DateTime.Parse(newDeactivatedDate) : new DateTime(),
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
                var strFileName = "SB-Org-Ingest-Log.txt";

                try
                {
                    var objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), strFileName), FileMode.Create, FileAccess.Write);
                    var objStreamWriter = new StreamWriter((Stream)objFilestream);
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
            var foundJF = false;
            var programs = _db.Programs; // define query
            foreach (var newProg in programs) // query executed and data obtained from database
            {
                if (newProg.JobFamily.Contains("Architecture and Engineering"))
                {
                    var newId = 1;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Arts, Design, Entertainment, Sports, and Media"))
                {
                    var newId = 2;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Building and Grounds Cleaning and Maintenance"))
                {
                    var newId = 3;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Business and Financial Operations"))
                {
                    var newId = 4;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Community and Social Service"))
                {
                    var newId = 5;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Computer and Mathematical"))
                {
                    var newId = 6;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Construction and Extraction"))
                {
                    var newId = 7;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Education, Training, and Library"))
                {
                    var newId = 8;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Farming, Fishing, and Forestry"))
                {
                    var newId = 9;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Food Preparation and Serving Related"))
                {
                    var newId = 10;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Healthcare Practitioners and Technical"))
                {
                    var newId = 11;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Healthcare Support"))
                {
                    var newId = 12;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Installation, Maintenance, and Repair"))
                {
                    var newId = 13;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Legal"))
                {
                    var newId = 14;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Life, Physical, and Social Science"))
                {
                    var newId = 15;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Management"))
                {
                    var newId = 16;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Military Specific"))
                {
                    var newId = 17;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Protective Service"))
                {
                    var newId = 18;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Sales and Related"))
                {
                    var newId = 19;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Transportation and Material Moving"))
                {
                    var newId = 20;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Other"))
                {
                    var newId = 21;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Office and Administrative Support"))
                {
                    var newId = 22;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Personal Care and Service"))
                {
                    var newId = 23;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }
                if (newProg.JobFamily.Contains("Production"))
                {
                    var newId = 24;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                    foundJF = true;
                }

                if (foundJF == false)
                {
                    Console.WriteLine("\nCouldn't find Job Family ID in known IDs for: " + newProg.JobFamily);
                    //jfMsg += "\nCouldn't find Job Family ID in known IDs for: " + jf;
                    var newId = 21;
                    var j = new ProgramJobFamily
                    {
                        Job_Family_Id = newId,
                        Program_Id = newProg.Id
                    };
                    _db.ProgramJobFamily.Add(j);
                }
            }
            var result = await _db.SaveChangesAsync();

            if (result > 0)
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

            var newSource = "OrgIngestTest14";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = "|",
                MissingFieldFound = null
            };

            var logMessage = "";
            var jfMsg = "";

            // Remove existing groups from Groups table
            DeleteAllOpportunityGroups();

            // Remove existing records from Opp Table
            DeleteAllOpportunities();

            // Remove existing records from Prog Table
            DeleteAllPrograms();

            var dict = new List<ProgramLookup>();

            using (var reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + "ProgIngestTest14" + ".csv"))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var progId = csv.GetField("Program Unique ID");
                    var durationFromFile = csv.GetField("Program Duration");

                    var item = new ProgramLookup();

                    var active = false;
                    if (csv.GetField("Program Status").Contains("Active"))
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
                            //item.Program_Duration = durationsi] != null ? durationsi] : " ";

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

                var i = 0;

                while (csv.Read())
                {
                    //var record = csv.GetRecord<ProgramModel>();
                    // Do something with the record.

                    // Remove un-parsable strings from dates, replace with current date
                    var newDateAuthorized = csv.GetField("Date Authorized").ToString();
                    newDateAuthorized = newDateAuthorized.Replace(" (Historical Import)", "");
                    if (newDateAuthorized.Contains("Unknown") || newDateAuthorized.Contains("N/A") || newDateAuthorized.Contains("") || newDateAuthorized.Contains(" "))
                    {
                        newDateAuthorized = DateTime.Now.ToString("MM/dd/yyyy");
                    }

                    var newMouCreation = csv.GetField("Date Authorized").ToString();
                    newMouCreation = newMouCreation.Replace(" (Historical Import)", "");
                    if (newMouCreation.Contains("Unknown") || newMouCreation.Contains("N/A") || newMouCreation.Contains("") || newMouCreation.Contains(" "))
                    {
                        newMouCreation = DateTime.Now.ToString("MM/dd/yyyy");
                    }

                    var newMouExpiration = csv.GetField("MOU Expiration Date").ToString();
                    newMouExpiration = newMouExpiration.Replace(" (Historical Import)", "");
                    if (newMouExpiration.Contains("Unknown") || newMouExpiration.Contains("N/A") || newMouExpiration.Contains("") || newMouExpiration.Contains(" "))
                    {
                        newMouExpiration = DateTime.Now.ToString("MM/dd/yyyy");
                    }

                    var newCreatedDate = csv.GetField("Date Added").ToString();
                    newCreatedDate = newCreatedDate.Replace(" (Historical Import)", "");
                    if (newCreatedDate.Contains("Unknown") || newCreatedDate.Contains("N/A") || newCreatedDate.Contains("") || newCreatedDate.Contains(" "))
                    {
                        newCreatedDate = DateTime.Now.ToString();
                    }

                    var newUpdatedDate = csv.GetField("Last Updated").ToString();
                    newUpdatedDate = newUpdatedDate.Replace(" (Historical Import)", "");
                    if (newUpdatedDate.Contains("Unknown") || newUpdatedDate.Contains("N/A") || newUpdatedDate.Contains("") || newUpdatedDate.Contains(" "))
                    {
                        newUpdatedDate = DateTime.Now.ToString();
                    }

                    var newIsActive = false;
                    var newDeactivatedDate = csv.GetField("Program Status").ToString();
                    if (newDeactivatedDate.Contains("Closed"))
                    {
                        if (!newDeactivatedDate.Contains("one-off") && !newDeactivatedDate.Contains("Duplicate Provider"))
                        {
                            if (newDeactivatedDate.Contains("(") && newDeactivatedDate.Contains(")"))
                            {
                                var pFrom = newDeactivatedDate.IndexOf("(") + 1;
                                var pTo = newDeactivatedDate.LastIndexOf(")");

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

                    var newHasIntake = csv.GetField("Have Intake");
                    bool hasIntake;
                    if (newHasIntake == "Y" || newHasIntake == "Yes")
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

                    var newLocationDetails = int.Parse(csv.GetField("Location Details Available"));
                    bool locationDetails;
                    if (newLocationDetails == 1)
                    {
                        locationDetails = true;
                    }
                    else
                    {
                        locationDetails = false;
                    }

                    var newHasConsent = csv.GetField("Have Consent");
                    bool hasConsent;
                    if (newHasConsent == "Y" || newHasConsent == "Yes")
                    {
                        hasConsent = true;
                    }
                    else
                    {
                        hasConsent = false;
                    }

                    var newHasMultipleLocations = csv.GetField("Multiple Locations");
                    bool hasMultipleLocations;
                    if (newHasMultipleLocations == "Y" || newHasMultipleLocations == "Yes")
                    {
                        hasMultipleLocations = true;
                    }
                    else
                    {
                        hasMultipleLocations = false;
                    }

                    var newReportingForm2020 = csv.GetField("2020 Reporting Form");
                    bool hasReportingForm2020;
                    if (newReportingForm2020 == "Y" || newReportingForm2020 == "Yes")
                    {
                        hasReportingForm2020 = true;
                    }
                    else
                    {
                        hasReportingForm2020 = false;
                    }

                    var newNationwide = int.Parse(csv.GetField("Nationwide"));
                    bool nationwide;
                    if (newNationwide == 1)
                    {
                        nationwide = true;
                    }
                    else
                    {
                        nationwide = false;
                    }

                    var newOnline = int.Parse(csv.GetField("Online"));
                    bool online;
                    if (newOnline == 1)
                    {
                        online = true;
                    }
                    else
                    {
                        online = false;
                    }

                    var newCohorts = csv.GetField("Support Cohorts");
                    bool cohorts;
                    if (newCohorts == "Yes" || newCohorts == "Y")
                    {
                        cohorts = true;
                    }
                    else
                    {
                        cohorts = false;
                    }

                    var newProgramStatus = csv.GetField("Program Status");
                    bool programStatus;
                    if (newProgramStatus.Contains("Active"))
                    {
                        programStatus = true;
                    }
                    else
                    {
                        programStatus = false;
                    }

                    // Get rid of N/As in id cols
                    var newLhnIntakeTicketId = csv.GetField("LHN Intake Ticket Number").ToString();
                    newLhnIntakeTicketId.Replace("Email contact update ", "");
                    if (newLhnIntakeTicketId.Contains("N/A"))
                    {
                        newLhnIntakeTicketId = "";
                    }

                    if (newLhnIntakeTicketId.Contains("-"))
                    {
                        var start = newLhnIntakeTicketId.IndexOf("-");
                        newLhnIntakeTicketId.Remove(start, 1);
                    }

                    if (newLhnIntakeTicketId.Contains("\r\n"))
                    {
                        var start = newLhnIntakeTicketId.IndexOf("\r\n");
                        newLhnIntakeTicketId.Remove(start, 1);
                    }

                    // TAKING THE LAST ONE IN THE LIST CURRENTLY    **NEEDS TO BE UPDATED **
                    if (newLhnIntakeTicketId.Contains(" "))
                    {
                        var vals = newLhnIntakeTicketId.Split(" ");

                        newLhnIntakeTicketId = vals[vals.Length - 1];
                    }

                    // Get the new Organization ID based off the legacy one
                    int newOrgId;
                    if (csv.GetField("Provider Unique ID") != null)
                    {
                        var legacyId = int.Parse(csv.GetField("Provider Unique ID"));

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
                    var dm = csv.GetField("Delivery Method");

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

                    var newForSpouses = false;

                    var pps = csv.GetField("Participation Populations");
                    Console.WriteLine("Prog Name: " + csv.GetField("Program Name") + " with PP of " + pps);

                    var splitPops = pps.Split(", ");

                    if (pps.Contains("spouses"))
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

                    var progId = csv.GetField("Program Unique ID");

                    // THIS NEEDS TO MOVE TO WRAP THE ENTIRETY OF CREATING A PROGRAM INSTEAD OF CHECKING HERE... MULTIPLE PROGRAMS NEED TO BE MADE FOR ITEMS WITH COMMAS
                    if (progId.Contains(","))
                    {
                        var ids = progId.Replace(" ", "").Split(',').Select(int.Parse).ToList();

                        foreach (var id in ids)
                        {
                            //Console.WriteLine("-=-=-=-=-=-=newCreatedDate: " + newCreatedDate);
                            //Console.WriteLine("-=-=-=-=-=-=newUpdatedDate: " + newUpdatedDate);

                            //Console.WriteLine("Program ID: " + newProgId);
                            Console.WriteLine("Provider ID: " + int.Parse(csv.GetField("Provider Unique ID")));

                            // Program Duration
                            int newProgramDuration;
                            var pd = "";// = csv.GetField("Program Duration");

                            var newProgramName = "";

                            // Get the specific progam duration if there's multiples listed in the org doc
                            foreach (var pl in dict)
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

                            var newSS = csv.GetField("Services Supported");

                            if (string.IsNullOrEmpty(newSS))
                            {
                                newSS = "All Services";
                            }

                            var tempProg = new ProgramModel
                            {
                                ProgramName = newProgramName,
                                OrganizationName = csv.GetField("Organization Name"),
                                OrganizationId = newOrgId,
                                LhnIntakeTicketId = newLhnIntakeTicketId,
                                HasIntake = hasIntake,
                                IntakeFormVersion = csv.GetField("Intake Form Version"),
                                QpIntakeSubmissionId = csv.GetField("QP Intake ID"),
                                //HasLocations = hasLocations,
                                LocationDetailsAvailable = locationDetails,
                                HasConsent = hasConsent,
                                QpLocationSubmissionId = csv.GetField("QP Locations/Refresh ID"),
                                LhnLocationTicketId = csv.GetField("LHN Location Ticket Number"),
                                HasMultipleLocations = hasMultipleLocations,
                                ReportingForm2020 = hasReportingForm2020,
                                DateAuthorized = DateTime.Parse(newDateAuthorized),
                                MouLink = csv.GetField("MOU Packet Link"),
                                MouCreationDate = DateTime.Parse(newMouCreation),
                                MouExpirationDate = DateTime.Parse(newMouExpiration),
                                Nationwide = nationwide,
                                Online = online,
                                ParticipationPopulations = csv.GetField("Participation Populations"),
                                //DeliveryMethod = newDeliveryMethod,//csv.GetField("Delivery Method"),
                                StatesOfProgramDelivery = csv.GetField("State(s) of Program Delivery"),
                                ProgramDuration = newProgramDuration,//csv.GetField("Program Duration"),
                                SupportCohorts = cohorts,
                                OpportunityType = csv.GetField("Opportunity Type"),
                                JobFamily = csv.GetField("Functional Area / Job Family"),
                                ServicesSupported = newSS,// csv.GetField("Services Supported"),
                                EnrollmentDates = csv.GetField("Enrollment Dates"),
                                DateCreated = DateTime.Parse(newCreatedDate),
                                DateUpdated = DateTime.Parse(newUpdatedDate),
                                DateDeactivated = newDeactivatedDate != "" ? DateTime.Parse(newDeactivatedDate) : new DateTime(),
                                IsActive = newIsActive,
                                CreatedBy = "Ingest", // Set this so no errors occur
                                UpdatedBy = "Ingest", // Set this so no errors occur
                                ProgramUrl = csv.GetField("URL"),
                                ProgramStatus = programStatus,
                                AdminPocFirstName = csv.GetField("Admin POC First Name 1"),
                                AdminPocLastName = csv.GetField("Admin POC Last Name 1"),
                                AdminPocEmail = csv.GetField("Admin POC Email Address 1"),
                                AdminPocPhone = csv.GetField("Admin POC Phone Number 1"),
                                PublicPocName = csv.GetField("POC for Site - Name"),
                                PublicPocEmail = csv.GetField("POC for Site - Email"),
                                Notes = csv.GetField("Notes"),
                                ForSpouses = newForSpouses,
                                LegacyProgramId = id,
                                LegacyProviderId = int.Parse(csv.GetField("Provider Unique ID"))
                            };

                            _db.Programs.Add(tempProg);
                            //_db.SaveChanges();

                            var result = await _db.SaveChangesAsync();

                            if (result >= 1)
                            {
                                // Generate Participation Popluation Entries for this program
                                //string pps = csv.GetField("Participation Populations");

                                //var splitPops = pps.Split(", ");

                                foreach (string s in splitPops)
                                {
                                    //Console.WriteLine("s: " + s);
                                    var newPop = FindProgramParticipationPopulationIdFromName(s);

                                    if (newPop != -1)
                                    {
                                        var pp = new ProgramParticipationPopulation
                                        {
                                            Program_Id = tempProg.Id,
                                            Participation_Population_Id = newPop
                                        };

                                        _db.ProgramParticipationPopulation.Add(pp);
                                    }
                                }

                                // Generate Services Supported Entries for this program
                                var ss = csv.GetField("Services Supported");

                                var splitServices = ss.Split(", ");

                                foreach (string s in splitServices)
                                {
                                    Console.WriteLine("ss: " + s);
                                    var newService = FindProgramServiceIdFromName(s);

                                    if (newService != -1)
                                    {
                                        var ps = new ProgramService
                                        {
                                            Program_Id = tempProg.Id,
                                            Service_Id = newService
                                        };

                                        _db.ProgramService.Add(ps);
                                    }
                                }

                                // Generate Delivery Method Entries for this program
                                var dms = csv.GetField("Delivery Method");

                                if (dms == "Hybrid (in-person and online) and In-person" || dms == "Hybrid (In-person and online) and In-person")
                                {
                                    var newMethod = FindProgramDeliveryMethodIdFromName("Hybrid (In-person and online)");

                                    if (newMethod != -1)
                                    {
                                        var pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }

                                    newMethod = FindProgramDeliveryMethodIdFromName("In-person");

                                    if (newMethod != -1)
                                    {
                                        var pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }
                                }
                                else if (dms == "Hybrid (in-person and online) and Online" || dms == "Hybrid (In-person and online) and Online")
                                {
                                    var newMethod = FindProgramDeliveryMethodIdFromName("Hybrid (In-person and online)");

                                    if (newMethod != -1)
                                    {
                                        var pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }

                                    newMethod = FindProgramDeliveryMethodIdFromName("Online");

                                    if (newMethod != -1)
                                    {
                                        var pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }
                                }
                                else if (dms == "In-person and Online")
                                {
                                    var newMethod = FindProgramDeliveryMethodIdFromName("In-person");

                                    if (newMethod != -1)
                                    {
                                        var pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }

                                    newMethod = FindProgramDeliveryMethodIdFromName("Online");

                                    if (newMethod != -1)
                                    {
                                        var pdm = new ProgramDeliveryMethod
                                        {
                                            Program_Id = tempProg.Id,
                                            Delivery_Method_Id = newMethod
                                        };

                                        _db.ProgramDeliveryMethod.Add(pdm);
                                    }
                                }
                                else
                                {
                                    var splitMethods = dms.Split(", ");

                                    foreach (string m in splitMethods)
                                    {
                                        Console.WriteLine("dm: " + m);
                                        var newMethod = FindProgramDeliveryMethodIdFromName(m);

                                        if (newMethod != -1)
                                        {
                                            var pdm = new ProgramDeliveryMethod
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
                            logMessage += "\nProgram_Name " + tempProg.ProgramName;
                            logMessage += "\nOrganizationName " + tempProg.OrganizationName;
                            logMessage += "\nOrganizationId " + tempProg.OrganizationId;
                            logMessage += "\nLhnIntakeTicketId " + tempProg.LhnIntakeTicketId;
                            logMessage += "\nHasIntake " + tempProg.HasIntake;
                            logMessage += "\nIntakeFormVersion " + tempProg.IntakeFormVersion;
                            logMessage += "\nQpIntakeSubmissionId " + tempProg.QpIntakeSubmissionId;
                            //logMessage += "\nHasLocations " + newProg.HasLocations;
                            logMessage += "\nLocationDetailsAvailable " + tempProg.LocationDetailsAvailable;
                            logMessage += "\nHasConsent " + tempProg.HasConsent;
                            logMessage += "\nQpLocationSubmissionId " + tempProg.QpLocationSubmissionId;
                            logMessage += "\nLhnLocationTicketId " + tempProg.LhnLocationTicketId;
                            logMessage += "\nHasMultipleLocations " + tempProg.HasMultipleLocations;
                            logMessage += "\nReportingForm2020 " + tempProg.ReportingForm2020;
                            logMessage += "\nDateAuthorized " + tempProg.DateAuthorized;
                            logMessage += "\nMouLink " + tempProg.MouLink;
                            logMessage += "\nMouCreationDate " + tempProg.MouCreationDate;
                            logMessage += "\nMouExpirationDate " + tempProg.MouExpirationDate;
                            logMessage += "\nNationwide " + tempProg.Nationwide;
                            logMessage += "\nOnline " + tempProg.Online;
                            logMessage += "\nParticipationPopulations " + tempProg.ParticipationPopulations;
                            logMessage += "\nDeliveryMethod " + tempProg.DeliveryMethod;
                            logMessage += "\nStatesOfProgramDelivery " + tempProg.StatesOfProgramDelivery;
                            logMessage += "\nProgramDuration " + tempProg.ProgramDuration;
                            logMessage += "\nSupportCohorts " + tempProg.SupportCohorts;
                            logMessage += "\nOpportunityType " + tempProg.OpportunityType;
                            logMessage += "\nJobFamily " + tempProg.JobFamily;
                            logMessage += "\nServicesSupported " + tempProg.ServicesSupported;
                            logMessage += "\nEnrollmentDates " + tempProg.EnrollmentDates;
                            logMessage += "\nDateCreated " + tempProg.DateCreated;
                            logMessage += "\nDateUpdated " + tempProg.DateUpdated;
                            logMessage += "\nCreatedBy " + tempProg.CreatedBy; // Set this so no errors occur
                            logMessage += "\nUpdatedBy " + tempProg.UpdatedBy; // Set this so no errors occur
                            logMessage += "\nProgramUrl " + tempProg.ProgramUrl;
                            logMessage += "\nProgramStatus " + tempProg.ProgramStatus;
                            logMessage += "\nAdminPocFirstName " + tempProg.AdminPocFirstName;
                            logMessage += "\nAdminPocLastName " + tempProg.AdminPocLastName;
                            logMessage += "\nAdminPocEmail " + tempProg.AdminPocEmail;
                            logMessage += "\nAdminPocPhone " + tempProg.AdminPocPhone;
                            logMessage += "\nPublicPocName " + tempProg.PublicPocName;
                            logMessage += "\nPublicPocEmail " + tempProg.PublicPocEmail;
                            logMessage += "\nNotes " + tempProg.Notes;
                            logMessage += "\nForSpouses " + tempProg.ForSpouses;
                            logMessage += "\nLegacyProgramId " + tempProg.LegacyProgramId;
                            logMessage += "\nLegacyProviderId " + tempProg.LegacyProviderId;
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
                        var pd = "";// = csv.GetField("Program Duration");

                        // Get the specific progam duration if there's multiples listed in the org doc
                        foreach (var pl in dict)
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

                        var newProg = new ProgramModel
                        {
                            ProgramName = csv.GetField("Program Name"),
                            OrganizationName = csv.GetField("Organization Name"),
                            OrganizationId = newOrgId,
                            LhnIntakeTicketId = newLhnIntakeTicketId,
                            HasIntake = hasIntake,
                            IntakeFormVersion = csv.GetField("Intake Form Version"),
                            QpIntakeSubmissionId = csv.GetField("QP Intake ID"),
                            IsActive = newIsActive,
                            //HasLocations = hasLocations,
                            LocationDetailsAvailable = locationDetails,
                            HasConsent = hasConsent,
                            QpLocationSubmissionId = csv.GetField("QP Locations/Refresh ID"),
                            LhnLocationTicketId = csv.GetField("LHN Location Ticket Number"),
                            HasMultipleLocations = hasMultipleLocations,
                            ReportingForm2020 = hasReportingForm2020,
                            DateAuthorized = DateTime.Parse(newDateAuthorized),
                            MouLink = csv.GetField("MOU Packet Link"),
                            MouCreationDate = DateTime.Parse(newMouCreation),
                            MouExpirationDate = DateTime.Parse(newMouExpiration),
                            Nationwide = nationwide,
                            Online = online,
                            ParticipationPopulations = csv.GetField("Participation Populations"),
                            //DeliveryMethod = newDeliveryMethod,//csv.GetField("Delivery Method"),
                            StatesOfProgramDelivery = csv.GetField("State(s) of Program Delivery"),
                            ProgramDuration = newProgramDuration, //csv.GetField("Program Duration"),
                            SupportCohorts = cohorts,
                            OpportunityType = csv.GetField("Opportunity Type"),
                            JobFamily = csv.GetField("Functional Area / Job Family"),
                            ServicesSupported = csv.GetField("Services Supported"),
                            EnrollmentDates = csv.GetField("Enrollment Dates"),
                            DateCreated = DateTime.Parse(newCreatedDate),
                            DateUpdated = DateTime.Parse(newUpdatedDate),
                            CreatedBy = "Ingest", // Set this so no errors occur
                            UpdatedBy = "Ingest", // Set this so no errors occur
                            ProgramUrl = csv.GetField("URL"),
                            ProgramStatus = programStatus,
                            AdminPocFirstName = csv.GetField("Admin POC First Name 1"),
                            AdminPocLastName = csv.GetField("Admin POC Last Name 1"),
                            AdminPocEmail = csv.GetField("Admin POC Email Address 1"),
                            AdminPocPhone = csv.GetField("Admin POC Phone Number 1"),
                            PublicPocName = csv.GetField("POC for Site - Name"),
                            PublicPocEmail = csv.GetField("POC for Site - Email"),
                            Notes = csv.GetField("Notes"),
                            ForSpouses = newForSpouses,
                            LegacyProgramId = newProgId,
                            LegacyProviderId = int.Parse(csv.GetField("Provider Unique ID"))
                        };

                        _db.Programs.Add(newProg);
                        var result = await _db.SaveChangesAsync();

                        if (result >= 1)
                        {
                            // Generate Participation Popluation Entries for this program
                            //string pps = csv.GetField("Participation Populations");

                            //var splitPops = pps.Split(", ");

                            foreach (string s in splitPops)
                            {
                                //Console.WriteLine("s: " + s);
                                var newPop = FindProgramParticipationPopulationIdFromName(s);

                                if (newPop != -1)
                                {
                                    var pp = new ProgramParticipationPopulation
                                    {
                                        Program_Id = newProg.Id,
                                        Participation_Population_Id = newPop
                                    };

                                    _db.ProgramParticipationPopulation.Add(pp);
                                }
                            }

                            // Generate Services Supported Entries for this program
                            var ss = csv.GetField("Services Supported");

                            var splitServices = ss.Split(", ");

                            foreach (string s in splitServices)
                            {
                                Console.WriteLine("ss: " + s);
                                var newService = FindProgramServiceIdFromName(s);

                                if (newService != -1)
                                {
                                    var ps = new ProgramService
                                    {
                                        Program_Id = newProg.Id,
                                        Service_Id = newService
                                    };

                                    _db.ProgramService.Add(ps);
                                }
                            }

                            // Generate Delivery Method Entries for this program
                            var dms = csv.GetField("Delivery Method");

                            if (dms == "Hybrid (in-person and online) and In-person" || dms == "Hybrid (In-person and online) and In-person")
                            {
                                var newMethod = FindProgramDeliveryMethodIdFromName("Hybrid (In-person and online)");

                                if (newMethod != -1)
                                {
                                    var pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }

                                newMethod = FindProgramDeliveryMethodIdFromName("In-person");

                                if (newMethod != -1)
                                {
                                    var pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }
                            }
                            else if (dms == "Hybrid (in-person and online) and Online" || dms == "Hybrid (In-person and online) and Online")
                            {
                                var newMethod = FindProgramDeliveryMethodIdFromName("Hybrid (In-person and online)");

                                if (newMethod != -1)
                                {
                                    var pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }

                                newMethod = FindProgramDeliveryMethodIdFromName("Online");

                                if (newMethod != -1)
                                {
                                    var pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }
                            }
                            else if (dms == "In-person and Online")
                            {
                                var newMethod = FindProgramDeliveryMethodIdFromName("In-person");

                                if (newMethod != -1)
                                {
                                    var pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }

                                newMethod = FindProgramDeliveryMethodIdFromName("Online");

                                if (newMethod != -1)
                                {
                                    var pdm = new ProgramDeliveryMethod
                                    {
                                        Program_Id = newProg.Id,
                                        Delivery_Method_Id = newMethod
                                    };

                                    _db.ProgramDeliveryMethod.Add(pdm);
                                }
                            }
                            else
                            {
                                var splitMethods = dms.Split(", ");

                                foreach (string m in splitMethods)
                                {
                                    Console.WriteLine("dm: " + m);
                                    var newMethod = FindProgramDeliveryMethodIdFromName(m);

                                    if (newMethod != -1)
                                    {
                                        var pdm = new ProgramDeliveryMethod
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

                            var splitMethods = dms.Split(", ");

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
                        logMessage += "\nProgram_Name " + newProg.ProgramName;
                        logMessage += "\nOrganizationId " + newProg.OrganizationId;
                        logMessage += "\nLhnIntakeTicketId " + newProg.LhnIntakeTicketId;
                        logMessage += "\nHasIntake " + newProg.HasIntake;
                        logMessage += "\nIntakeFormVersion " + newProg.IntakeFormVersion;
                        logMessage += "\nQpIntakeSubmissionId " + newProg.QpIntakeSubmissionId;
                        //logMessage += "\nHasLocations " + newProg.HasLocations;
                        logMessage += "\nLocationDetailsAvailable " + newProg.LocationDetailsAvailable;
                        logMessage += "\nHasConsent " + newProg.HasConsent;
                        logMessage += "\nQpLocationSubmissionId " + newProg.QpLocationSubmissionId;
                        logMessage += "\nLhnLocationTicketId " + newProg.LhnLocationTicketId;
                        logMessage += "\nHasMultipleLocations " + newProg.HasMultipleLocations;
                        logMessage += "\nReportingForm2020 " + newProg.ReportingForm2020;
                        logMessage += "\nDateAuthorized " + newProg.DateAuthorized;
                        logMessage += "\nMouLink " + newProg.MouLink;
                        logMessage += "\nMouCreationDate " + newProg.MouCreationDate;
                        logMessage += "\nMouExpirationDate " + newProg.MouExpirationDate;
                        logMessage += "\nNationwide " + newProg.Nationwide;
                        logMessage += "\nOnline " + newProg.Online;
                        logMessage += "\nParticipationPopulations " + newProg.ParticipationPopulations;
                        logMessage += "\nDeliveryMethod " + newProg.DeliveryMethod;
                        logMessage += "\nStatesOfProgramDelivery " + newProg.StatesOfProgramDelivery;
                        logMessage += "\nProgramDuration " + newProg.ProgramDuration;
                        logMessage += "\nSupportCohorts " + newProg.SupportCohorts;
                        logMessage += "\nOpportunityType " + newProg.OpportunityType;
                        logMessage += "\nJobFamily " + newProg.JobFamily;
                        logMessage += "\nServicesSupported " + newProg.ServicesSupported;
                        logMessage += "\nEnrollmentDates " + newProg.EnrollmentDates;
                        logMessage += "\nDateCreated " + newProg.DateCreated;
                        logMessage += "\nDateUpdated " + newProg.DateUpdated;
                        logMessage += "\nCreatedBy " + newProg.CreatedBy; // Set this so no errors occur
                        logMessage += "\nUpdatedBy " + newProg.UpdatedBy; // Set this so no errors occur
                        logMessage += "\nProgramUrl " + newProg.ProgramUrl;
                        logMessage += "\nProgramStatus " + newProg.ProgramStatus;
                        logMessage += "\nAdminPocFirstName " + newProg.AdminPocFirstName;
                        logMessage += "\nAdminPocLastName " + newProg.AdminPocLastName;
                        logMessage += "\nAdminPocEmail " + newProg.AdminPocEmail;
                        logMessage += "\nAdminPocPhone " + newProg.AdminPocPhone;
                        logMessage += "\nPublicPocName " + newProg.PublicPocName;
                        logMessage += "\nPublicPocEmail " + newProg.PublicPocEmail;
                        logMessage += "\nNotes " + newProg.Notes;
                        logMessage += "\nForSpouses " + newProg.ForSpouses;
                        logMessage += "\nLegacyProgramId " + newProg.LegacyProgramId;
                        logMessage += "\nLegacyProviderId " + newProg.LegacyProviderId;
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
                var strFileName = "SB-Prog-Ingest-Log.txt";

                try
                {
                    var objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), strFileName), FileMode.Create, FileAccess.Write);
                    var objStreamWriter = new StreamWriter((Stream)objFilestream);
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

            var newSource = "OppIngestTest14";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = "|",
                MissingFieldFound = null
            };

            var logMessage = "";
            var badRefsMessage = "";

            // Remove existing groups from Groups table
            DeleteAllOpportunityGroups();

            // Remove existing records from Opp Table
            DeleteAllOpportunities();

            using (var reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + newSource + ".csv"))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                var i = 0;

                while (csv.Read())
                {
                    //var record = csv.GetRecord<OpportunityModel>();
                    // Do something with the record.

                    // Remove (Historical Import) strings from dates
                    var newCreatedDate = csv.GetField("DATEENTERED").ToString();
                    newCreatedDate = newCreatedDate.Replace(" (Historical Import)", "");

                    var newUpdatedDate = csv.GetField("Date Updated").ToString();
                    newUpdatedDate = newUpdatedDate.Replace(" (Historical Import)", "");

                    var newInitDate = csv.GetField("DATEPROGRAMINITIATED").ToString();
                    newInitDate = newInitDate.Replace("Unknown", "");
                    if (newInitDate == "")
                    {
                        newInitDate = DateTime.Now.ToString();
                    }

                    var newMultLoc = csv.GetField("Multiple Locations");
                    bool multLoc;
                    if (newMultLoc == "Y")
                    {
                        multLoc = true;
                    }
                    else
                    {
                        multLoc = false;
                    }

                    var newCohorts = csv.GetField("Cohorts");
                    bool cohorts;
                    if (newCohorts == "Y" || newCohorts == "Yes")
                    {
                        cohorts = true;
                    }
                    else
                    {
                        cohorts = false;
                    }

                    var newMous = csv.GetField("MOUs");
                    bool mous;
                    if (newMous == "Y")
                    {
                        mous = true;
                    }
                    else
                    {
                        mous = false;
                    }

                    var newNationwide = csv.GetField("NATIONWIDE");
                    bool nationwide;
                    if (newNationwide == "1")
                    {
                        nationwide = true;
                    }
                    else
                    {
                        nationwide = false;
                    }

                    var newOnline = csv.GetField("ONLINE");
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
                    var newOrgId = -1;
                    var newProgId = -1;
                    var newMouLink = "";
                    var newMouExpirationDate = new DateTime();
                    var newAdminPocFirstName = "";
                    var newAdminPocLastName = "";
                    var newAdminPocEmail = "";
                    if (csv.GetField("Provider Unique ID") != null)
                    {
                        var legacyId = int.Parse(csv.GetField("Provider Unique ID"));

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

                            var prog = _db.Programs.SingleOrDefault(x => x.LegacyProgramId == legacyProgId &&
                            x.LegacyProviderId == legacyId);

                            if (prog != null)
                            {
                                newProgId = prog.Id;  // This is the ID in the table of the programs organization, based off of the legacy IDs

                                newMouLink = prog.MouLink;
                                newMouExpirationDate = prog.MouExpirationDate;
                                newAdminPocFirstName = prog.AdminPocFirstName;
                                newAdminPocLastName = prog.AdminPocLastName;
                                newAdminPocEmail = prog.AdminPocEmail;
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

                    var newDeliveryMethod = "0";

                    if (csv.GetField("Delivery Method") == "In-person")
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

                    var newIsActive = false;
                    var newDeactivatedDate = csv.GetField("Program Status").ToString();
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

                    var newOpp = new OpportunityModel
                    {
                        GroupId = newGroupId,
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
                        Mou_Link = newMouLink,
                        Mou_Expiration_Date = newMouExpirationDate,
                        Admin_Poc_First_Name = newAdminPocFirstName,
                        Admin_Poc_Last_Name = newAdminPocLastName,
                        Admin_Poc_Email = newAdminPocEmail,
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
                    logMessage += "\nGroup_Id " + newOpp.GroupId;
                    logMessage += "\nLegacy_Program_Id " + newOpp.Legacy_Program_Id;
                    logMessage += "\nLegacy_Provider_Id " + newOpp.Legacy_Provider_Id;
                    logMessage += "\nLegacy_Opportunity_Id " + newOpp.Legacy_Opportunity_Id;
                    logMessage += "\n====================================================";

                    i++;
                }

                // Write the log file
                var strFileName = "SB-Opp-Ingest-Log.txt";

                try
                {
                    var objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), strFileName), FileMode.Create, FileAccess.Write);
                    var objStreamWriter = new StreamWriter((Stream)objFilestream);
                    objStreamWriter.WriteLine(logMessage);
                    objStreamWriter.Close();
                    objFilestream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                }

                // Write the bad reference log file
                var refFileName = "SB-Opp-Ingest-Bad-Refs.txt";

                try
                {
                    var objFilestream = new FileStream(string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), refFileName), FileMode.Create, FileAccess.Write);
                    var objStreamWriter = new StreamWriter((Stream)objFilestream);
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
            foreach (var prog in _db.Programs)
            {
                //ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == opp.Program_Id);
                var opps = _db.Opportunities.Where(e => e.Program_Id == prog.Id).ToList();
                var org = _db.Organizations.FirstOrDefault(e => e.Id == prog.OrganizationId);

                //await UpdateStatesOfProgramDelivery(prog, opps);
                // Update Program
                var newStateList = "";
                var num = 0;

                var states = new List<string>();

                Console.WriteLine("Program Name: " + prog.ProgramName + "=====================");

                if (opps.Count > 0)
                {
                    // Make sure there aren't duplicate states in list
                    foreach (var o in opps)
                    {
                        var found = false;

                        foreach (var s in states)
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
                foreach (var s in states)
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

                //await _db.SaveChangesAsync();

                //await UpdateOrgStatesOfProgramDelivery(org);

                Console.WriteLine("Setting Program states for " + prog.ProgramName + " to " + newStateList);

                Console.WriteLine("End Program =======================================");

                //await _db.SaveChangesAsync();
            }

            /*This is the right save*/
            await _db.SaveChangesAsync();

            foreach (var org in _db.Organizations)
            {
                Console.WriteLine("Org Name: " + org.Name + "=====================");
                // Get all programs from org
                List<ProgramModel> progs = _db.Programs.Where(e => e.OrganizationId == org.Id).ToList();

                var states2 = new List<string>();

                foreach (var p in progs)
                {
                    var progStates = "";
                    progStates = p.StatesOfProgramDelivery;
                    //Console.WriteLine("progStates: " + progStates);

                    // Split out each programs states of program delivery, and add them to the states array
                    progStates = progStates.Replace(" ", "");
                    var splitStates = progStates.Split(",");

                    foreach (string s in splitStates)
                    {
                        if (s != "" && s != " ")
                        {
                            //Console.WriteLine("s in splitstates: " + s);
                            var found = false;

                            foreach (var st in states2)
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
                var count = 0;
                var orgStates = "";

                foreach (var s in states2)
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
            foreach (var org in orgs)
            {
                if (org.Is_Active == false)
                {
                    // Get child programs
                    var relatedProgs = progs.Where(x => x.OrganizationId == org.Id);

                    // Set each child program to disabled
                    foreach (var p in relatedProgs)
                    {
                        p.IsActive = false;

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
                if (p.IsActive == false)
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
            List<ProgramModel> progs = _db.Programs.Where(e => e.OrganizationId == org.Id).ToList();

            var states = new List<string>();

            foreach (var p in progs)
            {
                var progStates = "";
                progStates = p.StatesOfProgramDelivery;
                //Console.WriteLine("progStates: " + progStates);

                // Split out each programs states of program delivery, and add them to the states array
                progStates = progStates.Replace(" ", "");
                var splitStates = progStates.Split(",");

                foreach (string s in splitStates)
                {
                    if (s != "" && s != " ")
                    {
                        //Console.WriteLine("s in splitstates: " + s);
                        var found = false;

                        foreach (var st in states)
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
            var count = 0;
            var orgStates = "";

            foreach (var s in states)
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
            var newStateList = "";
            var num = 0;

            var states = new List<string>();

            // Make sure there aren't duplicate states in list
            foreach (var o in opps)
            {
                var found = false;

                foreach (var s in states)
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
            foreach (var s in states)
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

            await _db.SaveChangesAsync();
        }

        public int FindProgramParticipationPopulationIdFromName(string name)
        {
            var id = -1;

            var pop = _db.ParticipationPopulations.FirstOrDefault(e => e.Name.ToLower().Equals(name.ToLower()));

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
            var name = "";

            var service = _db.MilitaryBranches.FirstOrDefault(e => e.Id == id);

            if (service != null)
            {
                name = service.Name;
            }

            return name;
        }

        public int FindProgramServiceIdFromName(string name)
        {
            var id = -1;

            var service = _db.MilitaryBranches.FirstOrDefault(e => e.Name.ToLower().Equals(name.ToLower()));

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
            var id = -1;

            var method = _db.DeliveryMethods.FirstOrDefault(e => e.Name.ToLower().Equals(name.ToLower()));

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
            if (_db.Organizations.Any())
            {
                var orgs = new List<OrganizationModel>();

                foreach (var org in _db.Organizations)
                {
                    orgs.Add(org);
                }

                if (orgs.Count > 0)
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
                var progs = new List<ProgramModel>();

                foreach (var prog in _db.Programs)
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
                var opps = new List<OpportunityModel>();

                foreach (var opp in _db.Opportunities)
                {
                    opps.Add(opp);
                }

                if (opps.Count > 0)
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
                var pps = new List<ProgramParticipationPopulation>();

                foreach (var pp in _db.ProgramParticipationPopulation)
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
                var pps = new List<PendingProgramParticipationPopulation>();

                foreach (var pp in _db.PendingProgramParticipationPopulation)
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
                var jfs = new List<ProgramJobFamily>();

                foreach (var jf in _db.ProgramJobFamily)
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
                var jfs = new List<PendingProgramJobFamily>();

                foreach (var jf in _db.PendingProgramJobFamily)
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