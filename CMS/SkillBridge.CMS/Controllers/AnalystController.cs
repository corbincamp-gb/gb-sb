using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SkillBridge.CMS.ViewModel;
using SkillBridge.Business.Command;
using SkillBridge.Business.Data;
using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Model.Db.TrainingPlans;
using SkillBridge.Business.Query;
using SkillBridge.Business.Repository;
using Taku.Core.Global;

namespace SkillBridge.CMS.Controllers
{
    [Authorize(Roles = "Admin, Analyst")]
    public class AnalystController : Controller
    {
        private readonly ILogger<AnalystController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AnalystController(ILogger<AnalystController> logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext db, IEmailSender emailSender)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
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

        [HttpGet]
        public IActionResult EditHomepageNotification()
        {
            var config = _db.SiteConfiguration.FirstOrDefault(m => m.Id == 1);
            // Identity / Account / Login
            // return RedirectToAction("ListOpportunities", "Opportunities");
            ViewBag.NotificationType = config.NotificationType;
            ViewBag.NotificationHTML = config.NotificationHTML;

            return View(config);
        }

        [HttpPost]
        public async Task<IActionResult> EditHomepageNotification(SiteConfigurationModel model)
        {
            // Get the config record (there's only one in the table)
            var config = _db.SiteConfiguration.FirstOrDefault(m => m.Id == 1);

            // Remove the script and style tags from the HTML to be saved
            var removeScriptRegex = new Regex(
               "(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)",
               RegexOptions.Singleline | RegexOptions.IgnoreCase
            );

            if(model.NotificationHTML == null || model.NotificationHTML == "")
            {
                model.NotificationHTML = "";
            }

            string output = removeScriptRegex.Replace(model.NotificationHTML, "");

            config.NotificationType = model.NotificationType;
            config.NotificationHTML = output;

            // Save it to the DB
            var result = await _db.SaveChangesAsync();

            if (result >= 1)    // RESULT IS ACTUALLY THE NUMBER OF RECORDS UPDATED -- MAY NEED TO CHANGE THIS EVERYWHERE
            {
                return RedirectToAction("EditHomepageNotification");
            }
            else
            {

            }

            return RedirectToAction("EditHomepageNotification");
        }


        [HttpGet]
        public IActionResult ListPendingOrganizationChanges()
        {
            var orgs = _db.PendingOrganizationChanges;
            return View(orgs);
        }

        [HttpGet]
        public IActionResult ReviewPendingOrganizationChange(string id, string orgId)
        {
            // Find this specific pending change
            var pendingChange = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Id == int.Parse(id));

            // Find the existing Organization in the current database from the Organization_Id
            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == pendingChange.Organization_Id);

            if (org == null)
            {
                ViewBag.ErrorMessage = $"Organization with id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditOrganizationModel
            {
                Is_Active = org.Is_Active,
                Id = orgId,
                Name = org.Name,
                Poc_First_Name = org.Poc_First_Name,
                Poc_Last_Name = org.Poc_Last_Name,
                Poc_Email = org.Poc_Email,
                Poc_Phone = org.Poc_Phone,
                Date_Created = org.Date_Created,
                Date_Updated = org.Date_Updated,
                Created_By = org.Created_By,
                Updated_By = org.Updated_By,
                Organization_Url = org.Organization_Url,
                Organization_Type = org.Organization_Type,
                Notes = org.Notes,
                Legacy_Provider_Id = org.Legacy_Provider_Id,
                Pending_Change_Status = pendingChange.Pending_Change_Status,
                Pending_Fields = new List<string>(),
                Rejection_Reason = pendingChange.Rejection_Reason
            };

            // If we have a pending change we want a way to notify the user
            if (pendingChange != null)
            {
                ViewBag.hasPendingChange = true;

                ViewBag.Original_Is_Active = org.Is_Active;
                ViewBag.Original_Name = org.Name;
                ViewBag.Original_Poc_First_Name = org.Poc_First_Name;
                ViewBag.Original_Poc_Last_Name = org.Poc_Last_Name;
                ViewBag.Original_Poc_Email = org.Poc_Email;
                ViewBag.Original_Poc_Phone = org.Poc_Phone;
                ViewBag.Original_Organization_Url = org.Organization_Url;
                ViewBag.Original_Organization_Type = org.Organization_Type;
                ViewBag.Original_Notes = org.Notes;

                ViewBag.ShowOSDNotice = pendingChange.Requires_OSD_Review;

                // Update the model with data from the pending change
                model = UpdateOrgModelWithPendingChanges(model, pendingChange);
            }
            else
            {
                ViewBag.hasPendingChange = false;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListPendingProgramChanges()
        {
            var progs = _db.PendingProgramChanges;
            return View(progs);
        }

        [HttpGet]
        public IActionResult ListPendingProgramAdditions()
        {
            var progs = _db.PendingProgramAdditions;
            return View(progs);
        }

        [HttpGet]
        public async Task<IActionResult> ReviewPendingProgramChange(string id, string progId)
        {
            // Find this specific pending change
            PendingProgramChangeModel pendingChange = _db.PendingProgramChanges.FirstOrDefault(e => e.Id == int.Parse(id));

            // Find the existing Program in the current database from the Program_Id
            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == pendingChange.Program_Id);

            // Populate Dropdown info for Participation Population Dropdown
            List<ParticipationPopulation> pops = new List<ParticipationPopulation>();

            foreach (ParticipationPopulation pop in _db.ParticipationPopulations)
            {
                pops.Add(pop);
            };

            pops = pops.OrderBy(o => o.Name).ToList();

            ViewBag.Participation_Population_List = pops;

            // Populate original Participation Populations
            List<ProgramParticipationPopulation> originalPopsList = _db.ProgramParticipationPopulation.Where(e => e.Program_Id == int.Parse(progId)).ToList();
            List<int> originalSelectedPops = new List<int>();
            foreach (ProgramParticipationPopulation p in originalPopsList)
            {
                //Console.WriteLine("===== ADDING ORIG PARTICIPATION POP: " + p.Participation_Population_Id);
                originalSelectedPops.Add(p.Participation_Population_Id);
                //Console.WriteLine("Adding participation population to original selected pops w id: " + p.Participation_Population_Id);
            }

            // Populate original Participation Populations
            List<PendingProgramParticipationPopulation> newPopsList = _db.PendingProgramParticipationPopulation.Where(e => e.Program_Id == int.Parse(progId) && e.Pending_Program_Id == pendingChange.Id).ToList();
            List<int> newSelectedPops = new List<int>();
            foreach (PendingProgramParticipationPopulation p in newPopsList)
            {
                //Console.WriteLine("===== ADDING NEW PARTICIPATION POP: " + p.Participation_Population_Id);
                newSelectedPops.Add(p.Participation_Population_Id);
                //Console.WriteLine("Adding participation population to original selected pops w id: " + p.Participation_Population_Id);
            }

            // Job Family dropdown
            List<JobFamily> jfs = new List<JobFamily>();

            foreach (JobFamily jf in _db.JobFamilies)
            {
                jfs.Add(jf);
            };

            jfs = jfs.OrderBy(o => o.Name).ToList();

            ViewBag.Job_Family_List = jfs;

            // Populate original Job Families
            List<ProgramJobFamily> originalJfList = _db.ProgramJobFamily.Where(e => e.Program_Id == int.Parse(progId)).ToList();
            List<int> originalSelectedJfs = new List<int>();
            foreach (ProgramJobFamily j in originalJfList)
            {
                originalSelectedJfs.Add(j.Job_Family_Id);
                //Console.WriteLine("===== ADDING ORIG JOB FAMILY: " + j.Job_Family_Id);
            }

            // Populate selected Job Families
            List<PendingProgramJobFamily> newJfList = _db.PendingProgramJobFamily.Where(e => e.Program_Id == int.Parse(progId) && e.Pending_Program_Id == pendingChange.Id).ToList();
            List<int> newSelectedJfs = new List<int>();
            foreach (PendingProgramJobFamily j in newJfList)
            {
                newSelectedJfs.Add(j.Job_Family_Id);
                //Console.WriteLine("===== ADDING NEW JOB FAMILY: " + j.Job_Family_Id);
            }

            // Services Supported dropdown
            List<Service> ss = new List<Service>();

            foreach (Service s in _db.Services)
            {
                ss.Add(s);
            };

            //ss = ss.OrderBy(o => o.Name).ToList();

            ViewBag.Services_Supported_List = ss;

            // Populate original Services
            List<ProgramService> originalSsList = _db.ProgramService.Where(e => e.Program_Id == int.Parse(progId)).ToList();
            List<int> originalSelectedSs = new List<int>();
            foreach (ProgramService s in originalSsList)
            {
                originalSelectedSs.Add(s.Service_Id);
                Console.WriteLine("===== ADDING ORIG SERVICE: " + s.Service_Id);
            }

            // Populate selected Services
            List<PendingProgramService> newSsList = _db.PendingProgramService.Where(e => e.Program_Id == int.Parse(progId) && e.Pending_Program_Id == pendingChange.Id).ToList();
            List<int> newSelectedSs = new List<int>();
            foreach (PendingProgramService s in newSsList)
            {
                newSelectedSs.Add(s.Service_Id);
                Console.WriteLine("===== ADDING NEW SERVICE: " + s.Service_Id);
            }

            // Delivery Method dropdown
            List<DeliveryMethod> dm = new List<DeliveryMethod>();

            foreach (DeliveryMethod m in _db.DeliveryMethods)
            {
                dm.Add(m);
            };

            //ss = ss.OrderBy(o => o.Name).ToList();

            ViewBag.Delivery_Method_List = dm;

            // Populate original Delivery Methods
            List<ProgramDeliveryMethod> originalDmsList = _db.ProgramDeliveryMethod.Where(e => e.Program_Id == int.Parse(progId)).ToList();
            List<int> originalSelectedDms = new List<int>();
            foreach (ProgramDeliveryMethod m in originalDmsList)
            {
                originalSelectedDms.Add(m.Delivery_Method_Id);
                Console.WriteLine("===== ADDING ORIG DELIVERY METHOD: " + m.Delivery_Method_Id);
            }

            Console.WriteLine("-==--=-=-==-=-=-=-=-=-=-=-=--==-=-=-=- originalSelectedDms.Count: " + originalSelectedDms.Count);

            // Populate selected Delivery Methods
            List<PendingProgramDeliveryMethod> newDmsList = _db.PendingProgramDeliveryMethod.Where(e => e.Program_Id == int.Parse(progId) && e.Pending_Program_Id == pendingChange.Id).ToList();
            List<int> newSelectedDms = new List<int>();
            foreach (PendingProgramDeliveryMethod m in newDmsList)
            {
                newSelectedDms.Add(m.Delivery_Method_Id);
                Console.WriteLine("===== ADDING NEW DELIVERY METHOD: " + m.Delivery_Method_Id);
            }

            if (prog == null)
            {
                ViewBag.ErrorMessage = $"Program with id = {id} cannot be found";
                return View("NotFound");
            }

            Console.WriteLine("===== ORIG PROGRAM ID: " + prog.Program_Duration);
            Console.WriteLine("===== NEW PROGRAM ID: " + pendingChange.Program_Duration);

            Console.WriteLine("==== prog.Program_Duration: " + prog.Program_Duration);
            Console.WriteLine("==== pendingChange.Program_Duration: " + pendingChange.Program_Duration);

            var model = new EditProgramModel
            {
                Is_Active = prog.Is_Active,
                Id = progId,
                Program_Name = prog.Program_Name,
                Program_Id = prog.Id,
                Organization_Name = prog.Organization_Name,
                Organization_Id = prog.Organization_Id,
                Lhn_Intake_Ticket_Id = prog.Lhn_Intake_Ticket_Id,
                Has_Intake = prog.Has_Intake,
                Intake_Form_Version = prog.Intake_Form_Version,
                Qp_Intake_Submission_Id = prog.Qp_Intake_Submission_Id,
                Location_Details_Available = prog.Location_Details_Available,
                Has_Consent = prog.Has_Consent,
                Qp_Location_Submission_Id = prog.Qp_Location_Submission_Id,
                Lhn_Location_Ticket_Id = prog.Lhn_Location_Ticket_Id,
                Has_Multiple_Locations = prog.Has_Multiple_Locations,
                Reporting_Form_2020 = prog.Reporting_Form_2020,
                Date_Authorized = prog.Date_Authorized,
                Mou_Link = prog.Mou_Link,
                Mou_Creation_Date = prog.Mou_Creation_Date,
                Mou_Expiration_Date = prog.Mou_Expiration_Date,
                Nationwide = prog.Nationwide,
                Online = prog.Online,
                Participation_Populations = prog.Participation_Populations,
                //Delivery_Method = prog.Delivery_Method,
                States_Of_Program_Delivery = prog.States_Of_Program_Delivery,
                Program_Duration = prog.Program_Duration,
                Support_Cohorts = prog.Support_Cohorts,
                Opportunity_Type = prog.Opportunity_Type,
                Job_Family = prog.Job_Family,
                Services_Supported = prog.Services_Supported,
                Enrollment_Dates = prog.Enrollment_Dates,
                Date_Created = prog.Date_Created,
                Date_Updated = prog.Date_Updated,
                Created_By = prog.Created_By,
                Updated_By = prog.Updated_By,
                Program_Url = prog.Program_Url,
                Program_Status = prog.Program_Status,
                Admin_Poc_First_Name = prog.Admin_Poc_First_Name,
                Admin_Poc_Last_Name = prog.Admin_Poc_Last_Name,
                Admin_Poc_Email = prog.Admin_Poc_Email,
                Admin_Poc_Phone = prog.Admin_Poc_Phone,
                Public_Poc_Name = prog.Public_Poc_Name,
                Public_Poc_Email = prog.Public_Poc_Email,
                Notes = prog.Notes,
                For_Spouses = prog.For_Spouses,
                Legacy_Program_Id = prog.Legacy_Program_Id,
                Legacy_Provider_Id = prog.Legacy_Provider_Id,
                Pending_Change_Status = pendingChange.Pending_Change_Status,
                Pending_Fields = new List<string>(),
                Populations_List = newSelectedPops,
                Original_Populations_List = originalSelectedPops,
                Job_Family_List = newSelectedJfs,
                Original_Job_Family_List = originalSelectedJfs,
                Services_Supported_List = newSelectedSs,
                Original_Services_Supported_List = originalSelectedSs,
                Delivery_Method_List = newSelectedDms,
                Original_Delivery_Method_List = originalSelectedDms,
                Rejection_Reason = pendingChange.Rejection_Reason,
                SerializedTrainingPlan = pendingChange.SerializedTrainingPlan,
            };

            //Console.WriteLine("originalSelectedDms.Count: " + originalSelectedDms.Count);

            // If we have a pending change we want a way to notify the user
            if (pendingChange != null)
            {
                ViewBag.hasPendingChange = true;

                ViewBag.Original_Is_Active = prog.Is_Active;
                ViewBag.Original_Program_Name = prog.Program_Name;
                ViewBag.Original_Organization_Name = prog.Organization_Name;
                ViewBag.Original_Program_Id = prog.Id;
                ViewBag.Original_Organization_Id = prog.Organization_Id;
                ViewBag.Original_Lhn_Intake_Ticket_Id = prog.Lhn_Intake_Ticket_Id;
                ViewBag.Original_Has_Intake = prog.Has_Intake;
                ViewBag.Original_Intake_Form_Version = prog.Intake_Form_Version;
                ViewBag.Original_Qp_Intake_Submission_Id = prog.Qp_Intake_Submission_Id;
                ViewBag.Original_Location_Details_Available = prog.Location_Details_Available;
                ViewBag.Original_Has_Consent = prog.Has_Consent;
                ViewBag.Original_Qp_Location_Submission_Id = prog.Qp_Location_Submission_Id;
                ViewBag.Original_Lhn_Location_Ticket_Id = prog.Lhn_Location_Ticket_Id;
                ViewBag.Original_Has_Multiple_Locations = prog.Has_Multiple_Locations;
                ViewBag.Original_Reporting_Form_2020 = prog.Reporting_Form_2020;
                ViewBag.Original_Date_Authorized = prog.Date_Authorized;
                ViewBag.Original_Mou_Link = prog.Mou_Link;
                ViewBag.Original_Mou_Creation_Date = prog.Mou_Creation_Date;
                ViewBag.Original_Mou_Expiration_Date = prog.Mou_Expiration_Date;
                ViewBag.Original_Nationwide = prog.Nationwide;
                ViewBag.Original_Online = prog.Online;
                ViewBag.Original_Participation_Populations = originalSelectedPops;
                ViewBag.Original_Delivery_Method = originalSelectedDms;
                ViewBag.Original_States_Of_Program_Delivery = prog.States_Of_Program_Delivery;
                ViewBag.Original_Program_Duration = prog.Program_Duration;
                ViewBag.Original_Support_Cohorts = prog.Support_Cohorts;
                ViewBag.Original_Opportunity_Type = prog.Opportunity_Type;
                ViewBag.Original_Job_Family = originalSelectedJfs;
                ViewBag.Original_Services_Supported = prog.Services_Supported;
                ViewBag.Original_Enrollment_Dates = prog.Enrollment_Dates;
                ViewBag.Original_Date_Created = prog.Date_Created;
                ViewBag.Original_Date_Updated = prog.Date_Updated;
                ViewBag.Original_Created_By = prog.Created_By;
                ViewBag.Original_Updated_By = prog.Updated_By;
                ViewBag.Original_Program_Url = prog.Program_Url;
                ViewBag.Original_Program_Status = prog.Program_Status;
                ViewBag.Original_Admin_Poc_First_Name = prog.Admin_Poc_First_Name;
                ViewBag.Original_Admin_Poc_Last_Name = prog.Admin_Poc_Last_Name;
                ViewBag.Original_Admin_Poc_Email = prog.Admin_Poc_Email;
                ViewBag.Original_Admin_Poc_Phone = prog.Admin_Poc_Phone;
                ViewBag.Original_Public_Poc_Name = prog.Public_Poc_Name;
                ViewBag.Original_Public_Poc_Email = prog.Public_Poc_Email;
                ViewBag.Original_Notes = prog.Notes;
                ViewBag.Original_For_Spouses = prog.For_Spouses;
                ViewBag.Original_Legacy_Program_Id = prog.Legacy_Program_Id;
                ViewBag.Original_Legacy_Provider_Id = prog.Legacy_Provider_Id;

                ViewBag.ShowOSDNotice = pendingChange.Requires_OSD_Review;

                // Update the model with data from the pending change
                model = UpdateProgModelWithPendingChanges(model, pendingChange);
            }
            else
            {
                ViewBag.hasPendingChange = false;
            }

            if (!String.IsNullOrWhiteSpace(model.SerializedTrainingPlan))
            {
                var trainingPlan = Newtonsoft.Json.JsonConvert.DeserializeObject<TrainingPlan>(model.SerializedTrainingPlan);

                ViewBag.TrainingPlan = trainingPlan;

                if (trainingPlan.Id > 0)
                {
                    var repository = new TrainingPlanRepository(_db);
                    var originalTrainingPlan = await repository.GetTrainingPlanAsync(trainingPlan.Id);
                    ViewBag.OriginalTrainingPlan = originalTrainingPlan;
                }

                ViewBag.InstructionalMethodsList = await _db.InstructionalMethods.OrderBy(o => o.SortOrder).ToListAsync();
                ViewBag.TrainingPlanLengthsList = await _db.TrainingPlanLengths.OrderBy(o => o.SortOrder).ToListAsync();
            }

            Console.WriteLine("Setting original program duration to: " + ViewBag.Original_Program_Duration);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ReviewPendingProgramAddition(string id)
        {
            // Populate Dropdown info for Participation Population Dropdown
            List<ParticipationPopulation> pops = new List<ParticipationPopulation>();

            foreach (ParticipationPopulation pop in _db.ParticipationPopulations)
            {
                pops.Add(pop);
            };

            pops = pops.OrderBy(o => o.Name).ToList();

            ViewBag.Participation_Population_List = pops;

            // Populate selected Participation Populations

            List<PendingProgramAdditionParticipationPopulation> popsList = _db.PendingProgramAdditionsParticipationPopulation.Where(e => e.Pending_Program_Id == int.Parse(id)).ToList();

            List<int> selectedPops = new List<int>();

            foreach (PendingProgramAdditionParticipationPopulation p in popsList)
            {
                selectedPops.Add(p.Participation_Population_Id);
                Console.WriteLine("Adding participation population to selected pops w id: " + p.Participation_Population_Id);
            }

            // Populate Dropdown info for Job Family Dropdown
            List<JobFamily> jfs = new List<JobFamily>();

            foreach (JobFamily jf in _db.JobFamilies)
            {
                jfs.Add(jf);
            };

            jfs = jfs.OrderBy(o => o.Name).ToList();

            ViewBag.Job_Family_List = jfs;

            // Populate selected Job Family

            List<PendingProgramAdditionJobFamily> jfsList = _db.PendingProgramAdditionsJobFamily.Where(e => e.Pending_Program_Id == int.Parse(id)).ToList();

            List<int> selectedJfs = new List<int>();

            foreach (PendingProgramAdditionJobFamily j in jfsList)
            {
                selectedJfs.Add(j.Job_Family_Id);
                Console.WriteLine("Adding job family to selected jfs w id: " + j.Job_Family_Id);
            }

            // Populate Dropdown info for Service Dropdown
            List<Service> ss = new List<Service>();

            foreach (Service s in _db.Services)
            {
                ss.Add(s);
            };

            //ss = ss.OrderBy(o => o.Name).ToList();

            ViewBag.Services_Supported_List = ss;

            // Populate selected Services

            List<PendingProgramAdditionService> ssList = _db.PendingProgramAdditionsService.Where(e => e.Pending_Program_Id == int.Parse(id)).ToList();

            List<int> selectedSs = new List<int>();

            foreach (PendingProgramAdditionService s in ssList)
            {
                selectedSs.Add(s.Service_Id);
                Console.WriteLine("Adding service to selected ss w id: " + s.Service_Id);
            }

            // Populate Dropdown info for Delivery Method Dropdown
            List<DeliveryMethod> dms = new List<DeliveryMethod>();

            foreach (DeliveryMethod dm in _db.DeliveryMethods)
            {
                dms.Add(dm);
            };

            dms = dms.OrderBy(o => o.Name).ToList();

            ViewBag.Delivery_Method_List = dms;

            // Populate selected Delivery Method

            List<PendingProgramAdditionDeliveryMethod> dmsList = _db.PendingProgramAdditionsDeliveryMethod.Where(e => e.Pending_Program_Id == int.Parse(id)).ToList();

            List<int> selectedDms = new List<int>();

            foreach (PendingProgramAdditionDeliveryMethod m in dmsList)
            {
                selectedDms.Add(m.Delivery_Method_Id);
                Console.WriteLine("Adding delivery method to selected dms w id: " + m.Delivery_Method_Id);
            }

            //ViewBag.Selected_Participation_Populations = selectedPops;
            //Populations_List = selectedPops;

            // Find the existing Program in the current database
            PendingProgramAdditionModel prog = _db.PendingProgramAdditions.FirstOrDefault(e => e.Id == int.Parse(id));

            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == prog.Organization_Id);

            if (prog == null)
            {
                ViewBag.ErrorMessage = $"Program addition with id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditProgramModel
            {
                Id = prog.Id.ToString(),
                Program_Name = prog.Program_Name,
                Is_Active = prog.Is_Active,
                Organization_Name = prog.Organization_Name,
                Organization_Id = prog.Organization_Id,
                Program_Id = prog.Id,
                Lhn_Intake_Ticket_Id = prog.Lhn_Intake_Ticket_Id,  // LHN Intake Ticket Number
                Has_Intake = prog.Has_Intake,        // Do we have a completed QuestionPro intake form from them
                Intake_Form_Version = prog.Intake_Form_Version,  // Which version of the QuestionPro intake form did they fill out
                Qp_Intake_Submission_Id = prog.Qp_Intake_Submission_Id, // The ID of the QuestionPro intake form submission
                Location_Details_Available = prog.Location_Details_Available, // From col O of master spreadsheet
                Has_Consent = prog.Has_Consent,
                Qp_Location_Submission_Id = prog.Qp_Location_Submission_Id,
                Lhn_Location_Ticket_Id = prog.Lhn_Location_Ticket_Id,
                Has_Multiple_Locations = prog.Has_Multiple_Locations,
                Reporting_Form_2020 = prog.Reporting_Form_2020,
                Date_Authorized = prog.Date_Authorized,  // Date the 
                Mou_Link = prog.Mou_Link,      // URL link to actual MOU packet
                Mou_Creation_Date = prog.Mou_Creation_Date,
                Mou_Expiration_Date = prog.Mou_Expiration_Date,
                Nationwide = prog.Nationwide,
                Online = prog.Online,
                Participation_Populations = prog.Participation_Populations, // Might want enum for this
                //Delivery_Method = prog.Delivery_Method,
                States_Of_Program_Delivery = prog.States_Of_Program_Delivery,
                Program_Duration = prog.Program_Duration,
                Support_Cohorts = prog.Support_Cohorts,
                Opportunity_Type = prog.Opportunity_Type,
                Job_Family = prog.Job_Family,
                Services_Supported = prog.Services_Supported,
                Enrollment_Dates = prog.Enrollment_Dates,
                Date_Created = prog.Date_Created, // Date program was created in system
                Date_Updated = prog.Date_Updated, // Date program was last edited/updated in the system
                Created_By = prog.Created_By,
                Updated_By = prog.Updated_By,
                Program_Url = prog.Program_Url,
                Program_Status = prog.Program_Status, // 0 is disabled, 1 is enabled
                Admin_Poc_First_Name = prog.Admin_Poc_First_Name,
                Admin_Poc_Last_Name = prog.Admin_Poc_Last_Name,
                Admin_Poc_Email = prog.Admin_Poc_Email,
                Admin_Poc_Phone = prog.Admin_Poc_Phone,
                Public_Poc_Name = prog.Public_Poc_Name,
                Public_Poc_Email = prog.Public_Poc_Email,
                Notes = prog.Notes,
                For_Spouses = prog.For_Spouses,
                Pending_Change_Status = prog.Pending_Change_Status,
                Pending_Fields = new List<string>(),
                Populations_List = selectedPops,
                Job_Family_List = selectedJfs,
                Services_Supported_List = selectedSs,
                Delivery_Method_List = selectedDms,
                Rejection_Reason = prog.Rejection_Reason,
                SerializedTrainingPlan = prog.SerializedTrainingPlan,
            };

            if (!String.IsNullOrWhiteSpace(model.SerializedTrainingPlan))
            {
                var trainingPlan = Newtonsoft.Json.JsonConvert.DeserializeObject<TrainingPlan>(model.SerializedTrainingPlan);

                ViewBag.TrainingPlan = trainingPlan;

                if (trainingPlan.Id > 0)
                {
                    var repository = new TrainingPlanRepository(_db);
                    var originalTrainingPlan = await repository.GetTrainingPlanAsync(trainingPlan.Id);
                    ViewBag.OriginalTrainingPlan = originalTrainingPlan;
                }

                ViewBag.InstructionalMethodsList = await _db.InstructionalMethods.OrderBy(o => o.SortOrder).ToListAsync();
                ViewBag.TrainingPlanLengthsList = await _db.TrainingPlanLengths.OrderBy(o => o.SortOrder).ToListAsync();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListPendingOpportunityChanges()
        {
            var opps = _db.PendingOpportunityChanges;
            return View("~/Views/Analyst/ListPendingOpportunityChanges.cshtml", opps);
        }

        [HttpGet]
        public IActionResult ListPendingOpportunityAdditions()
        {
            var opps = _db.PendingOpportunityAdditions;
            return View("~/Views/Analyst/ListPendingOpportunityAdditions.cshtml", opps);
        }

        [HttpGet]
        public async Task<IActionResult> ReviewPendingOpportunityChange(string id, string oppId)
        {
            // Find this specific pending change
            var pendingChange = await _db.PendingOpportunityChanges.FirstOrDefaultAsync(e => e.Id == int.Parse(id));

            // Find the existing Program in the current database from the Program_Id
            OpportunityModel opp = await _db.Opportunities.FirstOrDefaultAsync(e => e.Id == pendingChange.Opportunity_Id);

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

            var model = new EditOpportunityModel
            {
                Id = oppId,
                Group_Id = opp.Group_Id,
                Opportunity_Id = opp.Id,
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
                Pending_Change_Status = pendingChange.Pending_Change_Status,
                Pending_Fields = new List<string>(),
                Rejection_Reason = pendingChange.Rejection_Reason
            };

            // If we have a pending change we want a way to notify the user
            if (pendingChange != null)
            {
                ViewBag.hasPendingChange = true;

                ViewBag.Original_Group_Id = opp.Group_Id;
                ViewBag.Original_Opportunity_Id = opp.Id;
                ViewBag.Original_Program_Name = opp.Program_Name;
                ViewBag.Original_Is_Active = opp.Is_Active;
                ViewBag.Original_Opportunity_Url = opp.Opportunity_Url;
                ViewBag.Original_Date_Program_Initiated = opp.Date_Program_Initiated;
                ViewBag.Original_Date_Created = opp.Date_Created; // Date opportunity was created in system
                ViewBag.Original_Date_Updated = opp.Date_Updated; // Date opportunity was last edited/updated in the system
                ViewBag.Original_Employer_Poc_Name = opp.Employer_Poc_Name;
                ViewBag.Original_Employer_Poc_Email = opp.Employer_Poc_Email;
                ViewBag.Original_Training_Duration = opp.Training_Duration;
                ViewBag.Original_Service = opp.Service;
                ViewBag.Original_Delivery_Method = opp.Delivery_Method;
                ViewBag.Original_Multiple_Locations = opp.Multiple_Locations;
                ViewBag.Original_Program_Type = opp.Program_Type;
                ViewBag.Original_Job_Families = opp.Job_Families;
                ViewBag.Original_Participation_Populations = opp.Participation_Populations;
                ViewBag.Original_Support_Cohorts = opp.Support_Cohorts;
                ViewBag.Original_Enrollment_Dates = opp.Enrollment_Dates;
                ViewBag.Original_Mous = opp.Mous;
                ViewBag.Original_Num_Locations = opp.Num_Locations;
                ViewBag.Original_Installation = opp.Installation;
                ViewBag.Original_City = opp.City;
                ViewBag.Original_State = opp.State;
                ViewBag.Original_Zip = opp.Zip;
                ViewBag.Original_Lat = opp.Lat;
                ViewBag.Original_Long = opp.Long;
                ViewBag.Original_Nationwide = opp.Nationwide;
                ViewBag.Original_Online = opp.Online;
                ViewBag.Original_Summary_Description = opp.Summary_Description;
                ViewBag.Original_Jobs_Description = opp.Jobs_Description;
                ViewBag.Original_Links_To_Prospective_Jobs = opp.Links_To_Prospective_Jobs;
                ViewBag.Original_Locations_Of_Prospective_Jobs_By_State = opp.Locations_Of_Prospective_Jobs_By_State;
                ViewBag.Original_Salary = opp.Salary;
                ViewBag.Original_Prospective_Job_Labor_Demand = opp.Prospective_Job_Labor_Demand;
                ViewBag.Original_Target_Mocs = opp.Target_Mocs;
                ViewBag.Original_Other_Eligibility_Factors = opp.Other_Eligibility_Factors;
                ViewBag.Original_Cost = opp.Cost;
                ViewBag.Original_Other = opp.Other;
                ViewBag.Original_Notes = opp.Notes;
                ViewBag.Original_Created_By = opp.Created_By;
                ViewBag.Original_Updated_By = opp.Updated_By;
                ViewBag.Original_For_Spouses = opp.For_Spouses;
                ViewBag.Original_Legacy_Opportunity_Id = opp.Legacy_Opportunity_Id;
                ViewBag.Original_Legacy_Program_Id = opp.Legacy_Program_Id;
                ViewBag.Original_Legacy_Provider_Id = opp.Legacy_Provider_Id;
                ViewBag.Original_Pending_Change_Status = pendingChange.Pending_Change_Status;

                ViewBag.ShowOSDNotice = pendingChange.Requires_OSD_Review;

                // Update the model with data from the pending change
                model = UpdateOppModelWithPendingChanges(model, pendingChange);
            }
            else
            {
                ViewBag.hasPendingChange = false;
            }

            return View(model);
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

        [HttpGet]
        public async Task<IActionResult> ReviewPendingOpportunityAddition(string id)
        {
            // Find this specific pending change
            var pendingChange = await _db.PendingOpportunityAdditions.FirstOrDefaultAsync(e => e.Id == int.Parse(id));
            //SB_PendingOpportunityAddition opp = _db.PendingOpportunityAdditions.FirstOrDefault(e => e.Id == int.Parse(id));

            var model = new EditOpportunityModel
            {
                Id = id,
                Group_Id = pendingChange.Group_Id,
                Opportunity_Id = pendingChange.Id,
                Is_Active = pendingChange.Is_Active,
                Program_Name = pendingChange.Program_Name,
                Opportunity_Url = pendingChange.Opportunity_Url,
                Date_Program_Initiated = pendingChange.Date_Program_Initiated,
                Date_Created = pendingChange.Date_Created, // Date opportunity was created in system
                Date_Updated = pendingChange.Date_Updated, // Date opportunity was last edited/updated in the system
                Employer_Poc_Name = pendingChange.Employer_Poc_Name,
                Employer_Poc_Email = pendingChange.Employer_Poc_Email,
                Training_Duration = pendingChange.Training_Duration,
                Service = pendingChange.Service,
                Delivery_Method = pendingChange.Delivery_Method,
                Multiple_Locations = pendingChange.Multiple_Locations,
                Program_Type = pendingChange.Program_Type,
                Job_Families = pendingChange.Job_Families,
                Participation_Populations = pendingChange.Participation_Populations,
                Support_Cohorts = pendingChange.Support_Cohorts,
                Enrollment_Dates = pendingChange.Enrollment_Dates,
                Mous = pendingChange.Mous,
                Num_Locations = pendingChange.Num_Locations,
                Installation = pendingChange.Installation,
                City = pendingChange.City,
                State = pendingChange.State,
                Zip = pendingChange.Zip,
                Lat = pendingChange.Lat,
                Long = pendingChange.Long,
                Nationwide = pendingChange.Nationwide,
                Online = pendingChange.Online,
                Summary_Description = pendingChange.Summary_Description,
                Jobs_Description = pendingChange.Jobs_Description,
                Links_To_Prospective_Jobs = pendingChange.Links_To_Prospective_Jobs,
                Locations_Of_Prospective_Jobs_By_State = pendingChange.Locations_Of_Prospective_Jobs_By_State,
                Salary = pendingChange.Salary,
                Prospective_Job_Labor_Demand = pendingChange.Prospective_Job_Labor_Demand,
                Target_Mocs = pendingChange.Target_Mocs,
                Other_Eligibility_Factors = pendingChange.Other_Eligibility_Factors,
                Cost = pendingChange.Cost,
                Other = pendingChange.Other,
                Notes = pendingChange.Notes,
                Created_By = pendingChange.Created_By,
                Updated_By = pendingChange.Updated_By,
                For_Spouses = pendingChange.For_Spouses,
                Legacy_Opportunity_Id = pendingChange.Legacy_Opportunity_Id,
                Legacy_Program_Id = pendingChange.Legacy_Program_Id,
                Legacy_Provider_Id = pendingChange.Legacy_Provider_Id,
                Pending_Change_Status = pendingChange.Pending_Change_Status,
                Rejection_Reason = pendingChange.Rejection_Reason,
                Pending_Fields = new List<string>()
            };

            List<SelectListItem> dropdownList = new List<SelectListItem>();

            List<ProgramDropdownItem> programLookupList = new List<ProgramDropdownItem>();

            // Populate Dropdown info for States Dropdown
            List<State> states = new List<State>();

            foreach (State s in _db.States)
            {
                states.Add(s);
            };

            states = states.OrderBy(o => o.Code).ToList();

            ViewBag.States = states;

            ViewBag.GroupIds = dropdownList;
            ViewBag.ProgramLookup = JsonConvert.SerializeObject(programLookupList);

            ViewBag.Organization_Id = pendingChange.Organization_Id;
            ViewBag.Program_Id = pendingChange.Program_Id;

            // If we have a pending change we want a way to notify the user
            /*if (pendingChange != null)
            {
                ViewBag.hasPendingChange = true;

                ViewBag.Original_Group_Id = opp.Group_Id;
                ViewBag.Original_Opportunity_Id = opp.Id;
                ViewBag.Original_Program_Name = opp.Program_Name;
                ViewBag.Original_Is_Active = opp.Is_Active;
                ViewBag.Original_Opportunity_Url = opp.Opportunity_Url;
                ViewBag.Original_Date_Program_Initiated = opp.Date_Program_Initiated;
                ViewBag.Original_Date_Created = opp.Date_Created; // Date opportunity was created in system
                ViewBag.Original_Date_Updated = opp.Date_Updated; // Date opportunity was last edited/updated in the system
                ViewBag.Original_Employer_Poc_Name = opp.Employer_Poc_Name;
                ViewBag.Original_Employer_Poc_Email = opp.Employer_Poc_Email;
                ViewBag.Original_Training_Duration = opp.Training_Duration;
                ViewBag.Original_Service = opp.Service;
                ViewBag.Original_Delivery_Method = opp.Delivery_Method;
                ViewBag.Original_Multiple_Locations = opp.Multiple_Locations;
                ViewBag.Original_Program_Type = opp.Program_Type;
                ViewBag.Original_Job_Families = opp.Job_Families;
                ViewBag.Original_Participation_Populations = opp.Participation_Populations;
                ViewBag.Original_Support_Cohorts = opp.Support_Cohorts;
                ViewBag.Original_Enrollment_Dates = opp.Enrollment_Dates;
                ViewBag.Original_Mous = opp.Mous;
                ViewBag.Original_Num_Locations = opp.Num_Locations;
                ViewBag.Original_Installation = opp.Installation;
                ViewBag.Original_City = opp.City;
                ViewBag.Original_State = opp.State;
                ViewBag.Original_Zip = opp.Zip;
                ViewBag.Original_Lat = opp.Lat;
                ViewBag.Original_Long = opp.Long;
                ViewBag.Original_Nationwide = opp.Nationwide;
                ViewBag.Original_Online = opp.Online;
                ViewBag.Original_Summary_Description = opp.Summary_Description;
                ViewBag.Original_Jobs_Description = opp.Jobs_Description;
                ViewBag.Original_Links_To_Prospective_Jobs = opp.Links_To_Prospective_Jobs;
                ViewBag.Original_Locations_Of_Prospective_Jobs_By_State = opp.Locations_Of_Prospective_Jobs_By_State;
                ViewBag.Original_Salary = opp.Salary;
                ViewBag.Original_Prospective_Job_Labor_Demand = opp.Prospective_Job_Labor_Demand;
                ViewBag.Original_Target_Mocs = opp.Target_Mocs;
                ViewBag.Original_Other_Eligibility_Factors = opp.Other_Eligibility_Factors;
                ViewBag.Original_Cost = opp.Cost;
                ViewBag.Original_Other = opp.Other;
                ViewBag.Original_Notes = opp.Notes;
                ViewBag.Original_Created_By = opp.Created_By;
                ViewBag.Original_Updated_By = opp.Updated_By;
                ViewBag.Original_For_Spouses = opp.For_Spouses;
                ViewBag.Original_Legacy_Opportunity_Id = opp.Legacy_Opportunity_Id;
                ViewBag.Original_Legacy_Program_Id = opp.Legacy_Program_Id;
                ViewBag.Original_Legacy_Provider_Id = opp.Legacy_Provider_Id;
                ViewBag.Original_Pending_Change_Status = pendingChange.Pending_Change_Status;

                // Update the model with data from the pending change
                model = UpdateOppModelWithPendingChanges(model, pendingChange);
            }
            else
            {
                ViewBag.hasPendingChange = false;
            }*/

            return View(model);
        }

        public EditOrganizationModel UpdateOrgModelWithPendingChanges(EditOrganizationModel model, PendingOrganizationChangeModel pendingChange)
        {
            if (model.Is_Active != pendingChange.Is_Active) { model.Is_Active = pendingChange.Is_Active; model.Pending_Fields.Add("Is_Active"); }

            if (model.Name != pendingChange.Name) {
                if ((String.IsNullOrEmpty(model.Name) == true && String.IsNullOrEmpty(pendingChange.Name) == true) == false)
                {
                    model.Name = pendingChange.Name; model.Pending_Fields.Add("Name");
                }
            }
            if (model.Poc_First_Name != pendingChange.Poc_First_Name) {
                if ((String.IsNullOrEmpty(model.Poc_First_Name) == true && String.IsNullOrEmpty(pendingChange.Poc_First_Name) == true) == false)
                {
                    model.Poc_First_Name = pendingChange.Poc_First_Name; model.Pending_Fields.Add("Poc_First_Name");
                }
            }
            if (model.Poc_Last_Name != pendingChange.Poc_Last_Name)
            {
                if ((String.IsNullOrEmpty(model.Poc_Last_Name) == true && String.IsNullOrEmpty(pendingChange.Poc_Last_Name) == true) == false)
                {
                    model.Poc_Last_Name = pendingChange.Poc_Last_Name; model.Pending_Fields.Add("Poc_Last_Name");
                }
            }
            if (model.Poc_Email != pendingChange.Poc_Email)
            {
                if ((String.IsNullOrEmpty(model.Poc_Email) == true && String.IsNullOrEmpty(pendingChange.Poc_Email) == true) == false)
                { 
                    model.Poc_Email = pendingChange.Poc_Email; model.Pending_Fields.Add("Poc_Email"); 
                }
            }
            if (model.Poc_Phone != pendingChange.Poc_Phone)
            {
                if ((String.IsNullOrEmpty(model.Poc_Phone) == true && String.IsNullOrEmpty(pendingChange.Poc_Phone) == true) == false)
                {
                    model.Poc_Phone = pendingChange.Poc_Phone; model.Pending_Fields.Add("Poc_Phone");
                }
            }
            if (model.Date_Created != pendingChange.Date_Created) { model.Date_Created = pendingChange.Date_Created; model.Pending_Fields.Add("Date_Created"); }
            if (model.Date_Updated != pendingChange.Date_Updated) { model.Date_Updated = pendingChange.Date_Updated; model.Pending_Fields.Add("Date_Updated"); }
            if (model.Created_By != pendingChange.Created_By)
            {
                if ((String.IsNullOrEmpty(model.Created_By) == true && String.IsNullOrEmpty(pendingChange.Created_By) == true) == false)
                {
                    model.Created_By = pendingChange.Created_By; model.Pending_Fields.Add("Created_By");
                }
            }
            if (model.Updated_By != pendingChange.Updated_By)
            {
                if ((String.IsNullOrEmpty(model.Updated_By) == true && String.IsNullOrEmpty(pendingChange.Updated_By) == true) == false)
                {
                    model.Updated_By = pendingChange.Updated_By; model.Pending_Fields.Add("Updated_By");
                }
            }
            if (model.Organization_Url != pendingChange.Organization_Url)
            {
                if ((String.IsNullOrEmpty(model.Organization_Url) == true && String.IsNullOrEmpty(pendingChange.Organization_Url) == true) == false)
                {
                    model.Organization_Url = pendingChange.Organization_Url; model.Pending_Fields.Add("Organization_Url");
                }
            }
            if (model.Organization_Type != pendingChange.Organization_Type) { model.Organization_Type = pendingChange.Organization_Type; model.Pending_Fields.Add("Organization_Type"); }
            if (model.Notes != pendingChange.Notes)
            {
                if ((String.IsNullOrEmpty(model.Notes) == true && String.IsNullOrEmpty(pendingChange.Notes) == true) == false)
                {
                    model.Notes = pendingChange.Notes; model.Pending_Fields.Add("Notes");
                }
            }
            if (model.Legacy_Provider_Id != pendingChange.Legacy_Provider_Id) { model.Legacy_Provider_Id = pendingChange.Legacy_Provider_Id; model.Pending_Fields.Add("Legacy_Provider_Id"); }

            return model;
        }

        public EditProgramModel UpdateProgModelWithPendingChanges(EditProgramModel model, PendingProgramChangeModel pendingChange)
        {
            if (model.Program_Name != pendingChange.Program_Name)
            {
                if ((String.IsNullOrEmpty(model.Program_Name) == true && String.IsNullOrEmpty(pendingChange.Program_Name) == true) == false)
                { model.Program_Name = pendingChange.Program_Name; model.Pending_Fields.Add("Program_Name"); }
            }

            if (model.Is_Active != pendingChange.Is_Active) { model.Is_Active = pendingChange.Is_Active; model.Pending_Fields.Add("Is_Active"); }

            if (model.Organization_Id != pendingChange.Organization_Id) { model.Organization_Id = pendingChange.Organization_Id; model.Pending_Fields.Add("Organization_Id"); }

            /*Console.WriteLine("model.Lhn_Intake_Ticket_Id is null: " + model.Lhn_Intake_Ticket_Id == null);
            Console.WriteLine("model.Lhn_Intake_Ticket_Id is '': " + model.Lhn_Intake_Ticket_Id == "");
            Console.WriteLine("model.Lhn_Intake_Ticket_Id is ' ': " + model.Lhn_Intake_Ticket_Id == " ");
            Console.WriteLine("model.Lhn_Intake_Ticket_Id String.IsNullOrEmpty(s): " + String.IsNullOrEmpty(model.Lhn_Intake_Ticket_Id));
            Console.WriteLine("model.Lhn_Intake_Ticket_Id: " + model.Lhn_Intake_Ticket_Id);
            Console.WriteLine("pendingChange.Lhn_Intake_Ticket_Id is null: " + pendingChange.Lhn_Intake_Ticket_Id == null);
            Console.WriteLine("pendingChange.Lhn_Intake_Ticket_Id is '': " + pendingChange.Lhn_Intake_Ticket_Id == "");
            Console.WriteLine("pendingChange.Lhn_Intake_Ticket_Id is ' ': " + pendingChange.Lhn_Intake_Ticket_Id == " ");
            Console.WriteLine("pendingChange.Lhn_Intake_Ticket_Id String.IsNullOrEmpty(s): " + String.IsNullOrEmpty(pendingChange.Lhn_Intake_Ticket_Id));
            Console.WriteLine("pendingChange.Lhn_Intake_Ticket_Id: " + pendingChange.Lhn_Intake_Ticket_Id);

            Console.WriteLine("Equals();: " + Equals(model.Lhn_Intake_Ticket_Id, pendingChange.Lhn_Intake_Ticket_Id));
            Console.WriteLine("ReferenceEquals();: " + ReferenceEquals(model.Lhn_Intake_Ticket_Id, pendingChange.Lhn_Intake_Ticket_Id));*/


            if (model.Lhn_Intake_Ticket_Id != pendingChange.Lhn_Intake_Ticket_Id)
            {
                if ((String.IsNullOrEmpty(model.Lhn_Intake_Ticket_Id) == true && String.IsNullOrEmpty(pendingChange.Lhn_Intake_Ticket_Id) == true) == false)
                {
                    model.Lhn_Intake_Ticket_Id = pendingChange.Lhn_Intake_Ticket_Id; model.Pending_Fields.Add("Lhn_Intake_Ticket_Id");
                }
            }

            if (model.Has_Intake != pendingChange.Has_Intake) { model.Has_Intake = pendingChange.Has_Intake; model.Pending_Fields.Add("Has_Intake"); }
            if (model.Intake_Form_Version != pendingChange.Intake_Form_Version)
            {
                if ((String.IsNullOrEmpty(model.Intake_Form_Version) == true && String.IsNullOrEmpty(pendingChange.Intake_Form_Version) == true) == false)
                { model.Intake_Form_Version = pendingChange.Intake_Form_Version; model.Pending_Fields.Add("Intake_Form_Version"); }
            }

            if (model.Qp_Intake_Submission_Id != pendingChange.Qp_Intake_Submission_Id)
            {
                if ((String.IsNullOrEmpty(model.Qp_Intake_Submission_Id) == true && String.IsNullOrEmpty(pendingChange.Qp_Intake_Submission_Id) == true) == false)
                {
                    model.Qp_Intake_Submission_Id = pendingChange.Qp_Intake_Submission_Id; model.Pending_Fields.Add("Qp_Intake_Submission_Id");
                }
            }

            if (model.Location_Details_Available != pendingChange.Location_Details_Available) { model.Location_Details_Available = pendingChange.Location_Details_Available; model.Pending_Fields.Add("Location_Details_Available"); }
            if (model.Has_Consent != pendingChange.Has_Consent) { model.Has_Consent = pendingChange.Has_Consent; model.Pending_Fields.Add("Has_Consent"); }
            if (model.Qp_Location_Submission_Id != pendingChange.Qp_Location_Submission_Id)
            {
                if ((String.IsNullOrEmpty(model.Qp_Location_Submission_Id) == true && String.IsNullOrEmpty(pendingChange.Qp_Location_Submission_Id) == true) == false)
                { model.Qp_Location_Submission_Id = pendingChange.Qp_Location_Submission_Id; model.Pending_Fields.Add("Qp_Location_Submission_Id"); }
            }
            if (model.Lhn_Location_Ticket_Id != pendingChange.Lhn_Location_Ticket_Id)
            {
                if ((String.IsNullOrEmpty(model.Lhn_Location_Ticket_Id) == true && String.IsNullOrEmpty(pendingChange.Lhn_Location_Ticket_Id) == true) == false)
                { model.Lhn_Location_Ticket_Id = pendingChange.Lhn_Location_Ticket_Id; model.Pending_Fields.Add("Lhn_Location_Ticket_Id"); }
            }
            if (model.Has_Multiple_Locations != pendingChange.Has_Multiple_Locations) { model.Has_Multiple_Locations = pendingChange.Has_Multiple_Locations; model.Pending_Fields.Add("Has_Multiple_Locations"); }
            if (model.Reporting_Form_2020 != pendingChange.Reporting_Form_2020) { model.Reporting_Form_2020 = pendingChange.Reporting_Form_2020; model.Pending_Fields.Add("Reporting_Form_2020"); }
            if (model.Date_Authorized != pendingChange.Date_Authorized) { model.Date_Authorized = pendingChange.Date_Authorized; model.Pending_Fields.Add("Date_Authorized"); }
            if (model.Mou_Link != pendingChange.Mou_Link)
            {
                if ((String.IsNullOrEmpty(model.Mou_Link) == true && String.IsNullOrEmpty(pendingChange.Mou_Link) == true) == false)
                { model.Mou_Link = pendingChange.Mou_Link; model.Pending_Fields.Add("Mou_Link"); }
            }
            if (model.Mou_Creation_Date != pendingChange.Mou_Creation_Date) { model.Mou_Creation_Date = pendingChange.Mou_Creation_Date; model.Pending_Fields.Add("Mou_Creation_Date"); }
            if (model.Mou_Expiration_Date != pendingChange.Mou_Expiration_Date) { model.Mou_Expiration_Date = pendingChange.Mou_Expiration_Date; model.Pending_Fields.Add("Mou_Expiration_Date"); }
            if (model.Nationwide != pendingChange.Nationwide) { model.Nationwide = pendingChange.Nationwide; model.Pending_Fields.Add("Nationwide"); }
            if (model.Online != pendingChange.Online) { model.Online = pendingChange.Online; model.Pending_Fields.Add("Online"); }

            // Populate selected Participation Populations
            List<PendingProgramParticipationPopulation> popsList = _db.PendingProgramParticipationPopulation.Where(e => e.Program_Id == int.Parse(model.Id) && e.Pending_Program_Id == pendingChange.Id).ToList();

            List<int> selectedPops = new List<int>();

            foreach (PendingProgramParticipationPopulation p in popsList)
            {
                selectedPops.Add(p.Participation_Population_Id);
                //Console.WriteLine("Adding pending participation population to selected pops w id: " + p.Participation_Population_Id);
            }

            List<ProgramParticipationPopulation> oldPopsList = _db.ProgramParticipationPopulation.Where(e => e.Program_Id == int.Parse(model.Id)).ToList();

            List<int> oldPops = new List<int>();

            foreach (ProgramParticipationPopulation p in oldPopsList)
            {
                oldPops.Add(p.Participation_Population_Id);
                //Console.WriteLine("Adding pending participation population to selected pops w id: " + p.Participation_Population_Id);
            }

            //Console.WriteLine("==============Checking part pop count in pending change, its " + selectedPops.Count);

            selectedPops.Sort();
            oldPops.Sort();

            List<int> dups = selectedPops.Intersect(oldPops).ToList();
            List<int> distinct = selectedPops.Except(oldPops).ToList();

            //Console.WriteLine("distinct.count in analystController for PP: " + distinct.Count);

            foreach (int i in dups)
            {
                //Console.WriteLine("duplicate item: " + i);
            }

            foreach (int j in distinct)
            {
                //Console.WriteLine("distinct item: " + j);
            }

            if (selectedPops.Count == 0)    // If there are no participation pops in the updated version
            {
                //Console.WriteLine("There are NO CHANGES to the participation pop for this program, it should be");

                if (oldPops.Count != selectedPops.Count)
                {
                    model.Pending_Fields.Add("Populations_List");
                }

                model.Populations_List = null;
            }
            else if (distinct.Count > 0 || selectedPops.Count != oldPops.Count) // If there is a difference between the old and new list of ints
            {
                model.Populations_List = selectedPops; model.Pending_Fields.Add("Populations_List");
            }

            // Populate selected Job Family
            List<PendingProgramJobFamily> jfList = _db.PendingProgramJobFamily.Where(e => e.Program_Id == int.Parse(model.Id) && e.Pending_Program_Id == pendingChange.Id).ToList();

            List<int> selectedJfs = new List<int>();

            foreach (PendingProgramJobFamily j in jfList)
            {
                selectedJfs.Add(j.Job_Family_Id);
                //Console.WriteLine("Adding pending job family to selected jf w id: " + j.Job_Family_Id);
            }

            List<ProgramJobFamily> oldJFList = _db.ProgramJobFamily.Where(e => e.Program_Id == int.Parse(model.Id)).ToList();

            List<int> oldJFs = new List<int>();

            foreach (ProgramJobFamily p in oldJFList)
            {
                oldJFs.Add(p.Job_Family_Id);
                //Console.WriteLine("Adding pending participation population to selected pops w id: " + p.Participation_Population_Id);
            }

            //Console.WriteLine("==============Checking jf count in pending change, its " + selectedJfs.Count);

            selectedJfs = selectedJfs.Distinct().ToList();
            oldJFs = oldJFs.Distinct().ToList();
            selectedJfs.Sort();
            oldJFs.Sort();

            foreach(int i in selectedJfs)
            {
                //Console.WriteLine("selectedJfs in analystController for JF: " + i);
            }

            foreach (int j in oldJFs)
            {
                //Console.WriteLine("oldJFs in analystController for JF: " + j);
            }

            // Duplicate vals
            List<int> dups2 = selectedJfs.Intersect(oldJFs).ToList();
            // Distinct vals
            List<int> distinct2 = selectedJfs.Except(oldJFs).ToList();

            //Console.WriteLine("distinct2.count in analystController for JF: " + distinct2.Count);

            if (selectedJfs.Count == 0)    // If there are no job family in the updated version
            {
                //Console.WriteLine("There are NO CHANGES to the job family for this program, it should be");

                if (oldJFs.Count != selectedJfs.Count)
                {
                    model.Pending_Fields.Add("Job_Family_List");
                }

                model.Job_Family_List = null;
            }
            else if (distinct2.Count > 0 || selectedJfs.Count != oldJFs.Count) // If there is a difference between the old and new list of ints
            {
                model.Job_Family_List = selectedJfs; model.Pending_Fields.Add("Job_Family_List");
            }

            // Populate selected Supported Services
            List<PendingProgramService> ssList = _db.PendingProgramService.Where(e => e.Program_Id == int.Parse(model.Id) && e.Pending_Program_Id == pendingChange.Id).ToList();

            List<int> selectedSs = new List<int>();

            foreach (PendingProgramService s in ssList)
            {
                selectedSs.Add(s.Service_Id);
                //Console.WriteLine("Adding pending job family to selected jf w id: " + j.Job_Family_Id);
            }

            List<ProgramService> oldSsList = _db.ProgramService.Where(e => e.Program_Id == int.Parse(model.Id)).ToList();

            List<int> oldSs = new List<int>();

            foreach (ProgramService s in oldSsList)
            {
                oldSs.Add(s.Service_Id);
                //Console.WriteLine("Adding pending participation population to selected pops w id: " + p.Participation_Population_Id);
            }

            //Console.WriteLine("==============Checking jf count in pending change, its " + selectedJfs.Count);

            selectedSs.Sort();
            oldSs.Sort();

            // Duplicate vals
            List<int> dups3 = selectedSs.Intersect(oldSs).ToList();
            // Distinct vals
            List<int> distinct3 = selectedSs.Except(oldSs).ToList();

            //Console.WriteLine("distinct2.count in analystController for JF: " + distinct2.Count);

            if (selectedSs.Count == 0)    // If there are no job family in the updated version
            {
                //Console.WriteLine("There are NO CHANGES to the job family for this program, it should be");

                if (oldSs.Count != selectedSs.Count)
                {
                    model.Pending_Fields.Add("Services_Supported_List");
                }

                model.Services_Supported_List = null;
            }
            else if (distinct3.Count > 0 || selectedSs.Count != oldSs.Count) // If there is a difference between the old and new list of ints
            {
                model.Services_Supported_List = selectedSs; model.Pending_Fields.Add("Services_Supported_List");
            }

            // Populate selected Delivery Methods
            List<PendingProgramDeliveryMethod> dmsList = _db.PendingProgramDeliveryMethod.Where(e => e.Program_Id == int.Parse(model.Id) && e.Pending_Program_Id == pendingChange.Id).ToList();

            List<int> selectedDms = new List<int>();

            foreach (PendingProgramDeliveryMethod p in dmsList)
            {
                selectedDms.Add(p.Delivery_Method_Id);
                //Console.WriteLine("Adding pending participation population to selected pops w id: " + p.Participation_Population_Id);
            }

            List<ProgramDeliveryMethod> oldDmsList = _db.ProgramDeliveryMethod.Where(e => e.Program_Id == int.Parse(model.Id)).ToList();

            List<int> oldDms = new List<int>();

            foreach (ProgramDeliveryMethod m in oldDmsList)
            {
                oldDms.Add(m.Delivery_Method_Id);
                //Console.WriteLine("Adding pending participation population to selected pops w id: " + p.Participation_Population_Id);
            }

            //Console.WriteLine("==============Checking part pop count in pending change, its " + selectedPops.Count);

            selectedDms.Sort();
            oldDms.Sort();

            List<int> dups4 = selectedDms.Intersect(oldDms).ToList();
            List<int> distinct4 = selectedDms.Except(oldDms).ToList();

            //Console.WriteLine("distinct.count in analystController for PP: " + distinct.Count);

            foreach (int i in dups4)
            {
                //Console.WriteLine("duplicate item: " + i);
            }

            foreach (int j in distinct4)
            {
                //Console.WriteLine("distinct item: " + j);
            }

            if (selectedDms.Count == 0)    // If there are no participation pops in the updated version
            {
                //Console.WriteLine("There are NO CHANGES to the participation pop for this program, it should be");

                if (oldDms.Count != selectedDms.Count)
                {
                    model.Pending_Fields.Add("Delivery_Method_List");
                }

                model.Delivery_Method_List = null;
            }
            else if (distinct4.Count > 0 || selectedDms.Count != oldDms.Count) // If there is a difference between the old and new list of ints
            {
                model.Delivery_Method_List = selectedDms; model.Pending_Fields.Add("Delivery_Method_List");
            }

            //Console.WriteLine("==========Checking for delivery method differences==========");
            //Console.WriteLine("model.Delivery_Method: " + model.Delivery_Method);
            //Console.WriteLine("pendingChange.Delivery_Method: " + pendingChange.Delivery_Method);

            /*if (model.Delivery_Method != pendingChange.Delivery_Method) { model.Delivery_Method = pendingChange.Delivery_Method; model.Pending_Fields.Add("Delivery_Method"); }*/

            /*if (model.Delivery_Method != pendingChange.Delivery_Method)
            {
                if ((String.IsNullOrEmpty(model.Delivery_Method) == true && String.IsNullOrEmpty(pendingChange.Delivery_Method) == true) == false)
                { model.Delivery_Method = pendingChange.Delivery_Method; model.Pending_Fields.Add("Delivery_Method"); }
            }*/
            if (model.States_Of_Program_Delivery != pendingChange.States_Of_Program_Delivery)
            {
                if ((String.IsNullOrEmpty(model.States_Of_Program_Delivery) == true && String.IsNullOrEmpty(pendingChange.States_Of_Program_Delivery) == true) == false)
                { model.States_Of_Program_Delivery = pendingChange.States_Of_Program_Delivery; model.Pending_Fields.Add("States_Of_Program_Delivery"); }
            }
            /*if (model.Program_Duration != pendingChange.Program_Duration)
            {
                if ((String.IsNullOrEmpty(model.Program_Duration) == true && String.IsNullOrEmpty(pendingChange.Program_Duration) == true) == false)
                { model.Program_Duration = pendingChange.Program_Duration; model.Pending_Fields.Add("Program_Duration"); }
            }*/
            if (model.Program_Duration != pendingChange.Program_Duration) { model.Program_Duration = pendingChange.Program_Duration; model.Pending_Fields.Add("Program_Duration"); }
            if (model.Support_Cohorts != pendingChange.Support_Cohorts) { model.Support_Cohorts = pendingChange.Support_Cohorts; model.Pending_Fields.Add("Support_Cohorts"); }
            if (model.Opportunity_Type != pendingChange.Opportunity_Type)
            {
                if ((String.IsNullOrEmpty(model.Opportunity_Type) == true && String.IsNullOrEmpty(pendingChange.Opportunity_Type) == true) == false)
                { model.Opportunity_Type = pendingChange.Opportunity_Type; model.Pending_Fields.Add("Opportunity_Type"); }
            }
            if (model.Job_Family != pendingChange.Job_Family)
            {
                if ((String.IsNullOrEmpty(model.Job_Family) == true && String.IsNullOrEmpty(pendingChange.Job_Family) == true) == false)
                { model.Job_Family = pendingChange.Job_Family; model.Pending_Fields.Add("Job_Family"); }
            }
            /*if (model.Services_Supported != pendingChange.Services_Supported)
            {
                if ((String.IsNullOrEmpty(model.Services_Supported) == true && String.IsNullOrEmpty(pendingChange.Services_Supported) == true) == false)
                { model.Services_Supported = pendingChange.Services_Supported; model.Pending_Fields.Add("Services_Supported"); }
            }*/
            if (model.Enrollment_Dates != pendingChange.Enrollment_Dates)
            {
                if ((String.IsNullOrEmpty(model.Enrollment_Dates) == true && String.IsNullOrEmpty(pendingChange.Enrollment_Dates) == true) == false)
                { model.Enrollment_Dates = pendingChange.Enrollment_Dates; model.Pending_Fields.Add("Enrollment_Dates"); }
            }
            if (model.Date_Created != pendingChange.Date_Created) { model.Date_Created = pendingChange.Date_Created; model.Pending_Fields.Add("Date_Created"); }
            if (model.Date_Updated != pendingChange.Date_Updated) { model.Date_Updated = pendingChange.Date_Updated; model.Pending_Fields.Add("Date_Updated"); }
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
            if (model.Program_Url != pendingChange.Program_Url)
            {
                if ((String.IsNullOrEmpty(model.Program_Url) == true && String.IsNullOrEmpty(pendingChange.Program_Url) == true) == false)
                { model.Program_Url = pendingChange.Program_Url; model.Pending_Fields.Add("Program_Url"); }
            }
            if (model.Program_Status != pendingChange.Program_Status) { model.Program_Status = pendingChange.Program_Status; model.Pending_Fields.Add("Program_Status"); }
            if (model.Admin_Poc_First_Name != pendingChange.Admin_Poc_First_Name)
            {
                if ((String.IsNullOrEmpty(model.Admin_Poc_First_Name) == true && String.IsNullOrEmpty(pendingChange.Admin_Poc_First_Name) == true) == false)
                { model.Admin_Poc_First_Name = pendingChange.Admin_Poc_First_Name; model.Pending_Fields.Add("Admin_Poc_First_Name"); }
            }
            if (model.Admin_Poc_Last_Name != pendingChange.Admin_Poc_Last_Name)
            {
                if ((String.IsNullOrEmpty(model.Admin_Poc_Last_Name) == true && String.IsNullOrEmpty(pendingChange.Admin_Poc_Last_Name) == true) == false)
                { model.Admin_Poc_Last_Name = pendingChange.Admin_Poc_Last_Name; model.Pending_Fields.Add("Admin_Poc_Last_Name"); }
            }
            if (model.Admin_Poc_Email != pendingChange.Admin_Poc_Email)
            {
                if ((String.IsNullOrEmpty(model.Admin_Poc_Email) == true && String.IsNullOrEmpty(pendingChange.Admin_Poc_Email) == true) == false)
                { model.Admin_Poc_Email = pendingChange.Admin_Poc_Email; model.Pending_Fields.Add("Admin_Poc_Email"); }
            }
            if (model.Admin_Poc_Phone != pendingChange.Admin_Poc_Phone)
            {
                if ((String.IsNullOrEmpty(model.Admin_Poc_Phone) == true && String.IsNullOrEmpty(pendingChange.Admin_Poc_Phone) == true) == false)
                { model.Admin_Poc_Phone = pendingChange.Admin_Poc_Phone; model.Pending_Fields.Add("Admin_Poc_Phone"); }
            }
            if (model.Public_Poc_Name != pendingChange.Public_Poc_Name)
            {
                if ((String.IsNullOrEmpty(model.Public_Poc_Name) == true && String.IsNullOrEmpty(pendingChange.Public_Poc_Name) == true) == false)
                { model.Public_Poc_Name = pendingChange.Public_Poc_Name; model.Pending_Fields.Add("Public_Poc_Name"); }
            }
            if (model.Public_Poc_Email != pendingChange.Public_Poc_Email)
            {
                if ((String.IsNullOrEmpty(model.Public_Poc_Email) == true && String.IsNullOrEmpty(pendingChange.Public_Poc_Email) == true) == false)
                { model.Public_Poc_Email = pendingChange.Public_Poc_Email; model.Pending_Fields.Add("Public_Poc_Email"); }
            }
            if (model.Notes != pendingChange.Notes)
            {
                if ((String.IsNullOrEmpty(model.Notes) == true && String.IsNullOrEmpty(pendingChange.Notes) == true) == false)
                { model.Notes = pendingChange.Notes; model.Pending_Fields.Add("Notes"); }
            }
            if (model.For_Spouses != pendingChange.For_Spouses) { model.For_Spouses = pendingChange.For_Spouses; model.Pending_Fields.Add("For_Spouses"); }

            return model;
        }

        public EditOpportunityModel UpdateOppModelWithPendingChanges(EditOpportunityModel model, PendingOpportunityChangeModel pendingChange)
        {
            if (model.Group_Id != pendingChange.Group_Id) { model.Group_Id = pendingChange.Group_Id; model.Pending_Fields.Add("Group_Id"); }

            if (model.Is_Active != pendingChange.Is_Active) { model.Is_Active = pendingChange.Is_Active; model.Pending_Fields.Add("Is_Active"); }

            if (model.Program_Name != pendingChange.Program_Name)
            {
                if (pendingChange.Program_Name != null && pendingChange.Program_Name != "")
                {
                    model.Program_Name = pendingChange.Program_Name; model.Pending_Fields.Add("Program_Name");
                }
            }

            if (model.Opportunity_Url != pendingChange.Opportunity_Url)
            {
                model.Opportunity_Url = pendingChange.Opportunity_Url; model.Pending_Fields.Add("Opportunity_Url");
            }

            if (model.Date_Program_Initiated != pendingChange.Date_Program_Initiated) { model.Date_Program_Initiated = pendingChange.Date_Program_Initiated; model.Pending_Fields.Add("Date_Program_Initiated"); }

            if (model.Date_Created != pendingChange.Date_Created) { model.Date_Created = pendingChange.Date_Created; model.Pending_Fields.Add("Date_Created"); }

            if (model.Date_Updated != pendingChange.Date_Updated) { model.Date_Updated = pendingChange.Date_Updated; model.Pending_Fields.Add("Date_Updated"); }

            if (model.Employer_Poc_Name != pendingChange.Employer_Poc_Name)
            {
                model.Employer_Poc_Name = pendingChange.Employer_Poc_Name; model.Pending_Fields.Add("Employer_Poc_Name");
            }

            if (model.Employer_Poc_Email != pendingChange.Employer_Poc_Email)
            {
                model.Employer_Poc_Email = pendingChange.Employer_Poc_Email; model.Pending_Fields.Add("Employer_Poc_Email");
            }

            if (model.Training_Duration != pendingChange.Training_Duration)
            {
                model.Training_Duration = pendingChange.Training_Duration; model.Pending_Fields.Add("Training_Duration");
            }

            if (model.Service != pendingChange.Service)
            {
                model.Service = pendingChange.Service; model.Pending_Fields.Add("Service");
            }

            if (model.Delivery_Method != pendingChange.Delivery_Method)
            {
                model.Delivery_Method = pendingChange.Delivery_Method; model.Pending_Fields.Add("Delivery_Method");
            }

            if (model.Multiple_Locations != pendingChange.Multiple_Locations) { model.Multiple_Locations = pendingChange.Multiple_Locations; model.Pending_Fields.Add("Multiple_Locations"); }

            if (model.Program_Type != pendingChange.Program_Type)
            {
                model.Program_Type = pendingChange.Program_Type; model.Pending_Fields.Add("Program_Type");
            }

            if (model.Job_Families != pendingChange.Job_Families)
            {
                model.Job_Families = pendingChange.Job_Families; model.Pending_Fields.Add("Job_Families");
            }

            if (model.Participation_Populations != pendingChange.Participation_Populations)
            {
                model.Participation_Populations = pendingChange.Participation_Populations; model.Pending_Fields.Add("Participation_Populations");
            }

            if (model.Support_Cohorts != pendingChange.Support_Cohorts) { model.Support_Cohorts = pendingChange.Support_Cohorts; model.Pending_Fields.Add("Support_Cohorts"); }

            if (model.Enrollment_Dates != pendingChange.Enrollment_Dates)
            {
                model.Enrollment_Dates = pendingChange.Enrollment_Dates; model.Pending_Fields.Add("Enrollment_Dates");
            }

            if (model.Mous != pendingChange.Mous) { model.Mous = pendingChange.Mous; model.Pending_Fields.Add("Mous"); }

            if (model.Num_Locations != pendingChange.Num_Locations) { model.Num_Locations = pendingChange.Num_Locations; model.Pending_Fields.Add("Num_Locations"); }

            if (model.Installation != pendingChange.Installation)
            {
                model.Installation = pendingChange.Installation; model.Pending_Fields.Add("Installation");
            }

            if (model.City != pendingChange.City)
            {
                model.City = pendingChange.City; model.Pending_Fields.Add("City");
            }

            if (model.State != pendingChange.State)
            {
                model.State = pendingChange.State; model.Pending_Fields.Add("State");
            }

            if (model.Zip != pendingChange.Zip)
            {
                model.Zip = pendingChange.Zip; model.Pending_Fields.Add("Zip");
            }

            if (model.Lat != pendingChange.Lat) { model.Lat = pendingChange.Lat; model.Pending_Fields.Add("Lat"); }

            if (model.Long != pendingChange.Long) { model.Long = pendingChange.Long; model.Pending_Fields.Add("Long"); }

            if (model.Nationwide != pendingChange.Nationwide) { model.Nationwide = pendingChange.Nationwide; model.Pending_Fields.Add("Nationwide"); }

            if (model.Online != pendingChange.Online) { model.Online = pendingChange.Online; model.Pending_Fields.Add("Online"); }

            if (model.Summary_Description != pendingChange.Summary_Description)
            {
                model.Summary_Description = pendingChange.Summary_Description; model.Pending_Fields.Add("Summary_Description");
            }

            if (model.Jobs_Description != pendingChange.Jobs_Description)
            {
                model.Jobs_Description = pendingChange.Jobs_Description; model.Pending_Fields.Add("Jobs_Description");
            }

            if (model.Links_To_Prospective_Jobs != pendingChange.Links_To_Prospective_Jobs)
            {
                model.Links_To_Prospective_Jobs = pendingChange.Links_To_Prospective_Jobs; model.Pending_Fields.Add("Links_To_Prospective_Jobs");
            }

            if (model.Locations_Of_Prospective_Jobs_By_State != pendingChange.Locations_Of_Prospective_Jobs_By_State)
            {
                model.Locations_Of_Prospective_Jobs_By_State = pendingChange.Locations_Of_Prospective_Jobs_By_State; model.Pending_Fields.Add("Locations_Of_Prospective_Jobs_By_State");
            }

            if (model.Salary != pendingChange.Salary)
            {
                model.Salary = pendingChange.Salary; model.Pending_Fields.Add("Salary");
            }

            if (model.Prospective_Job_Labor_Demand != pendingChange.Prospective_Job_Labor_Demand)
            {
                model.Prospective_Job_Labor_Demand = pendingChange.Prospective_Job_Labor_Demand; model.Pending_Fields.Add("Prospective_Job_Labor_Demand");
            }

            if (model.Target_Mocs != pendingChange.Target_Mocs)
            {
                model.Target_Mocs = pendingChange.Target_Mocs; model.Pending_Fields.Add("Target_Mocs");
            }

            if (model.Other_Eligibility_Factors != pendingChange.Other_Eligibility_Factors)
            {
                model.Other_Eligibility_Factors = pendingChange.Other_Eligibility_Factors; model.Pending_Fields.Add("Other_Eligibility_Factors");
            }

            if (model.Cost != pendingChange.Cost)
            {
                model.Cost = pendingChange.Cost; model.Pending_Fields.Add("Cost");
            }

            if (model.Other != pendingChange.Other)
            {
                model.Other = pendingChange.Other; model.Pending_Fields.Add("Other");
            }

            if (model.Notes != pendingChange.Notes)
            {
                model.Notes = pendingChange.Notes; model.Pending_Fields.Add("Notes");
            }

            if (model.For_Spouses != pendingChange.For_Spouses) { model.For_Spouses = pendingChange.For_Spouses; model.Pending_Fields.Add("For_Spouses"); }

            if (model.Legacy_Opportunity_Id != pendingChange.Legacy_Opportunity_Id) { model.Legacy_Opportunity_Id = pendingChange.Legacy_Opportunity_Id; model.Pending_Fields.Add("Legacy_Opportunity_Id"); }

            if (model.Legacy_Program_Id != pendingChange.Legacy_Program_Id) { model.Legacy_Program_Id = pendingChange.Legacy_Program_Id; model.Pending_Fields.Add("Legacy_Program_Id"); }

            if (model.Legacy_Provider_Id != pendingChange.Legacy_Provider_Id) { model.Legacy_Provider_Id = pendingChange.Legacy_Provider_Id; model.Pending_Fields.Add("Legacy_Provider_Id"); }

            return model;
        }

        /*public EditOpportunityModel UpdateOppModelWithPendingChanges(EditOrganizationModel model, PendingOrganizationModel pendingChange)
        {
            if (model.Name != pendingChange.Name) { model.Name = pendingChange.Name; model.Pending_Fields.Add("Name"); }
        }*/

        // Accept the reviewed change
        [HttpPost]
        public async Task<IActionResult> ReviewPendingOrganizationChange(EditOrganizationModel model, string orgId)
        {
            // Find this specific pending change
            var pendingChange = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Organization_Id == int.Parse(orgId) && e.Pending_Change_Status == 0);

            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == int.Parse(orgId));

            string userName = HttpContext.User.Identity.Name;

            bool updateEnabledFields = false;
            bool updateOptimizationFields = false;

            // Set is_active fields to update on child records if we have is_active set to false
            if (model.Is_Active == false)
            {
                //Console.WriteLine("The posted model was set to disabled");
                updateEnabledFields = true;
            }

            // Set optimization fields to update on child program/oppotunity records if we have a name change
            if (!String.Equals(org.Name, model.Name, StringComparison.Ordinal))
            {
                updateOptimizationFields = true;
            }

            // Update the organization with the data from the reviewed change
            if (org != null)
            {
                org.Is_Active = model.Is_Active;
                org.Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Name));
                org.Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Poc_First_Name));
                org.Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Poc_Last_Name));
                org.Poc_Email = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Poc_Email));
                org.Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Poc_Phone));
                org.Date_Updated = DateTime.Now;
                org.Updated_By = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Updated_By));
                org.Organization_Url = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Organization_Url));
                org.Organization_Type = model.Organization_Type;
                org.Notes = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Notes));
            }

            _db.Organizations.Update(org);

            var result1 = await _db.SaveChangesAsync();

            if(result1 > 0)
            {
                // Update pending change
                pendingChange.Is_Active = model.Is_Active;
                pendingChange.Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Name));
                pendingChange.Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Poc_First_Name));
                pendingChange.Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Poc_Last_Name));
                pendingChange.Poc_Email = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Poc_Email));
                pendingChange.Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Poc_Phone));
                pendingChange.Organization_Url = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Organization_Url));
                pendingChange.Organization_Type = model.Organization_Type;
                pendingChange.Notes = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Notes));
                pendingChange.Pending_Change_Status = 1;

                // 6/22/2023 RSS: There are some instances where the updated date is 0/0/0001. If this is the case, mark the updated date to "now"
                if (pendingChange.Date_Updated == DateTime.MinValue)
                {
                    pendingChange.Date_Updated = DateTime.Now;
                }
                pendingChange.Last_Admin_Action_User = userName;
                pendingChange.Last_Admin_Action_Time = DateTime.Now;
                pendingChange.Last_Admin_Action_Type = "Approved";
                pendingChange.Rejection_Reason = "";
                _db.PendingOrganizationChanges.Update(pendingChange);
                //_db.PendingOrganizationChanges.Remove(pendingChange);

                var result2 = await _db.SaveChangesAsync();

                if(result2 > 0)
                {
                    // Identify if we have a valid email to notify
                    if (pendingChange.Updated_By != "Ingest")
                    {
                        // Get email address of user in updated_by field
                        ApplicationUser u = await _userManager.FindByNameAsync(pendingChange.Updated_By);
                        if (u == null)
                        {
                            u = await _userManager.FindByEmailAsync(pendingChange.Updated_By);
                        }

                        string email = u.Email;

                        if (u != null)
                        {
                            if (IsValidEmail(email))
                            {
                                //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                                await _emailSender.SendEmailAsync(email, "SkillBridge System Organization Change Accepted", "The update to your organization's information has been made.<br/><br/>The SkillBridge website should reflect the change after the next site update, which should be no later than Friday, at 5:00 PM EST");
                            }
                        }
                    }

                    if (updateEnabledFields)
                    {
                        var progsToUpdate = _db.Programs.Where(p => p.Organization_Id == int.Parse(orgId));
                        var oppsToUpdate = _db.Opportunities.Where(p => p.Organization_Id == int.Parse(orgId));

                        if (progsToUpdate.ToList<ProgramModel>().Count > 0)
                        {
                            Console.WriteLine("There are programs to update on disable");
                            foreach (ProgramModel p in progsToUpdate)
                            {
                                p.Is_Active = false;
                                p.Date_Deactivated = DateTime.Now;
                                _db.Programs.Update(p);
                            }
                        }

                        if(oppsToUpdate.ToList<OpportunityModel>().Count > 0)
                        {
                            Console.WriteLine("There are opportunities to update on disable");
                            foreach (OpportunityModel o in oppsToUpdate)
                            {
                                o.Is_Active = false;
                                o.Date_Deactivated = DateTime.Now;
                                _db.Opportunities.Update(o);
                            }
                        }
                    }

                    // We need to update the optimization fields for programs and opportunities now if the name changed
                    if(updateOptimizationFields)
                    { 
                        var progsToUpdate = _db.Programs.Where(p => p.Organization_Id == int.Parse(orgId));
                        var oppsToUpdate = _db.Opportunities.Where(p => p.Organization_Id == int.Parse(orgId));

                        if(progsToUpdate.ToList<ProgramModel>().Count > 0 || oppsToUpdate.ToList<OpportunityModel>().Count > 0)
                        {
                            foreach (ProgramModel p in progsToUpdate)
                            {
                                p.Organization_Name = org.Name;
                                _db.Programs.Update(p);
                            }

                            foreach (OpportunityModel o in oppsToUpdate)
                            {
                                o.Organization_Name = org.Name;
                                _db.Opportunities.Update(o);
                            }                            
                        }                        
                    }

                    var result3 = await _db.SaveChangesAsync();

                    if (result3 > 0)
                    {
                        return RedirectToAction("ListPendingOrganizationChanges");
                    }
                    else
                    {
                        return RedirectToAction("ListPendingOrganizationChanges");
                    }
                }
            }
            else
            {

            }

            return RedirectToAction("ListPendingOrganizationChanges", "Analyst");
        }

        public string GetParticipationPopNameFromId(int id)
        {
            foreach (ParticipationPopulation pop in _db.ParticipationPopulations)
            {
                if (id == pop.Id)
                {
                    return pop.Name;
                }
            };
            return "";
        }

        // Accept the reviewed change
        [HttpPost]
        public async Task<IActionResult> ReviewPendingProgramChange(EditProgramModel model, TrainingPlan tp, string progId)
        {
            // Find this specific pending change
            PendingProgramChangeModel pendingChange = _db.PendingProgramChanges.FirstOrDefault(e => e.Program_Id == int.Parse(progId) && e.Pending_Change_Status == 0);

            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == int.Parse(progId));

            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == prog.Organization_Id);

            bool newForSpouses = false;

            string userName = HttpContext.User.Identity.Name;

            bool updateEnabledFields = false;
            bool updateOptimizationFields = false;
            bool updateOnlineNationwideFields = false;

            if(model.Populations_List != null)
            {
                foreach (int p in model.Populations_List)
                {
                    if (GetParticipationPopNameFromId(p).Equals("Military Spouses"))
                    {
                        newForSpouses = true;
                        Console.WriteLine("newForSpouses being set to true");
                    }
                }
            }

            // Set is_active fields to update on child records if we have is_active set to false
            if (prog.Is_Active == false)
            {
                updateEnabledFields = true;
            }

            // Set optimization fields to update on child program/oppotunity records if we have a name change
            if (!String.Equals(prog.Program_Name, model.Program_Name, StringComparison.Ordinal))
            {
                updateOptimizationFields = true;
            }

            // If Online or Nationwide has changed, we will need to update opps
            if (pendingChange.Online != model.Online || pendingChange.Nationwide != model.Nationwide)
            {
                updateOnlineNationwideFields = true;
            }

            // Update the organization with the data from the reviewed change
            if (prog != null)
            {

                //int numStatesFound = GlobalFunctions.FindNumStatesInProgram(prog, _db);
                //TODO: Inject Query class
                var numStatesFound = new NumberOfStatesInProgramQuery().Get(prog, _db);

                prog.Is_Active = model.Is_Active;
                prog.Program_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Name));
                prog.Organization_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Organization_Name));
                prog.Organization_Id = model.Organization_Id;
                prog.Lhn_Intake_Ticket_Id = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Lhn_Intake_Ticket_Id));
                prog.Has_Intake = model.Has_Intake;
                prog.Intake_Form_Version = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Intake_Form_Version));
                prog.Qp_Intake_Submission_Id = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Qp_Intake_Submission_Id));
                prog.Location_Details_Available = model.Location_Details_Available;
                prog.Has_Consent = model.Has_Consent;
                prog.Qp_Location_Submission_Id = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Qp_Location_Submission_Id));
                prog.Lhn_Location_Ticket_Id = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Lhn_Location_Ticket_Id));
                prog.Has_Multiple_Locations = numStatesFound > 1 ? true : false;
                prog.Reporting_Form_2020 = model.Reporting_Form_2020;
                prog.Date_Authorized = model.Date_Authorized;
                prog.Mou_Link = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Mou_Link));
                prog.Mou_Creation_Date = model.Mou_Creation_Date;
                prog.Mou_Expiration_Date = model.Mou_Expiration_Date;
                prog.Nationwide = model.Online == true || numStatesFound >= GlobalFunctions.MIN_STATES_FOR_NATIONWIDE ? true : false; // Nationwide should be calculated on if it's an online program or if sum of child opportunities are offered 3 or more collective states;
                prog.Online = model.Online;
                prog.Participation_Populations = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Participation_Populations));
                //prog.Delivery_Method = model.Delivery_Method;//PreventNullString(model.Delivery_Method);
                prog.States_Of_Program_Delivery = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.States_Of_Program_Delivery));
                prog.Program_Duration = model.Program_Duration;
                prog.Support_Cohorts = model.Support_Cohorts;
                prog.Opportunity_Type = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Opportunity_Type));
                prog.Job_Family = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Job_Family));
                prog.Services_Supported = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Services_Supported));
                prog.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Enrollment_Dates));
                prog.Date_Created = model.Date_Created;
                prog.Date_Updated = DateTime.Now;
                prog.Created_By = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Created_By));
                prog.Updated_By = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Updated_By));
                prog.Program_Url = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Url));
                prog.Program_Status = model.Program_Status;
                prog.Admin_Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Admin_Poc_First_Name));
                prog.Admin_Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Admin_Poc_Last_Name));
                prog.Admin_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Admin_Poc_Email));
                prog.Admin_Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Admin_Poc_Phone));
                prog.Public_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Public_Poc_Name));
                prog.Public_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Public_Poc_Email));
                prog.Notes = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Notes));
                //prog.For_Spouses = model.For_Spouses;
                //Console.WriteLine("program for spouses being set to: " + newForSpouses);
                prog.For_Spouses = newForSpouses;
                prog.Legacy_Program_Id = model.Legacy_Program_Id;
                prog.Legacy_Provider_Id = model.Legacy_Provider_Id;

                //prog.Populations_List = selectedPops;
            }

            Console.WriteLine("prog.Organization_Id: " + prog.Organization_Id);
            _db.Programs.Update(prog);

            var result1 = await _db.SaveChangesAsync();

            Console.WriteLine("result1: " + result1);

            if (result1 > 0)
            {
                // Save training plan, if it exists
                if (!String.IsNullOrWhiteSpace(pendingChange.SerializedTrainingPlan))
                {
                    var repository = new TrainingPlanRepository(_db);

                    // Set the training plan ID to zero so it always gets added
                    tp.Id = 0;

                    // Pull in any changes from the form
                    foreach (var instructionalMethodId in Request.Form["TrainingPlanInstructionalMethods[].InstructionalMethodId"])
                    {
                        tp.TrainingPlanInstructionalMethods.Add(new TrainingPlanInstructionalMethod
                        {
                            InstructionalMethodId = int.Parse(instructionalMethodId),
                            OtherText = (instructionalMethodId == "5" ? Request.Form["TrainingPlanInstructionalMethods[].OtherText"] : String.Empty)
                        });
                    }

                    for (var i = 0; i < tp.BreakdownCount; i++)
                    {
                        tp.TrainingPlanBreakdowns.Add(new TrainingPlanBreakdown
                        {
                            RowId = int.Parse(Request.Form[$"TrainingPlanBreakdowns[{i + 1}].RowId"]),
                            TrainingModuleTitle = Request.Form[$"TrainingPlanBreakdowns[{i + 1}].TrainingModuleTitle"],
                            LearningObjective = Request.Form[$"TrainingPlanBreakdowns[{i + 1}].LearningObjective"],
                            TotalHours = decimal.Parse(Request.Form[$"TrainingPlanBreakdowns[{i + 1}].TotalHours"]),
                        });
                    }


                    // Save the training plan
                    tp = await repository.SaveTrainingPlanAsync(tp, userName);
                    pendingChange.SerializedTrainingPlan = Newtonsoft.Json.JsonConvert.SerializeObject(tp);

                    // Assign the training plan to the program
                    await repository.SaveTrainingPlanToProgramAsync(prog.Id, tp.Id, userName);
                }

                // Remove existing pending changes for this dropdown before adding any
                List<ProgramParticipationPopulation> popsList = _db.ProgramParticipationPopulation.Where(e => e.Program_Id == prog.Id).ToList();

                // If we have existing population items for this pending change, remove them first
                if (popsList.Count > 0)
                {
                    _db.ProgramParticipationPopulation.RemoveRange(popsList);

                    var result2 = await _db.SaveChangesAsync();
                    Console.WriteLine("-------------Removing existing pps from pending change");
                }

                // If the new version of this has values, add them to db
                if (model.Populations_List != null)
                {
                    // Check each population type and add if it doesnt exist for this pending change
                    foreach (int p in model.Populations_List)
                    {
                        //Console.WriteLine("adding population to pending program change: " + p);

                        ProgramParticipationPopulation pp = new ProgramParticipationPopulation
                        {
                            Program_Id = prog.Id,
                            Participation_Population_Id = p
                        };

                        _db.ProgramParticipationPopulation.Add(pp);
                    }
                }

                // Remove existing pending changes for this dropdown before adding any
                List<ProgramJobFamily> jfsList = _db.ProgramJobFamily.Where(e => e.Program_Id == prog.Id).ToList();

                // If we have existing population items for this pending change, remove them first
                if (jfsList.Count > 0)
                {
                    _db.ProgramJobFamily.RemoveRange(jfsList);

                    var result3 = await _db.SaveChangesAsync();
                    Console.WriteLine("-------------Removing existing pps from pending change");
                }

                if (model.Job_Family_List != null)
                {
                    // Check each population type and add if it doesnt exist for this pending change
                    foreach (int j in model.Job_Family_List)
                    {
                        //Console.WriteLine("adding jf to pending program change: " + j);

                        ProgramJobFamily jf = new ProgramJobFamily
                        {
                            Program_Id = prog.Id,
                            Job_Family_Id = j
                        };

                        _db.ProgramJobFamily.Add(jf);
                    }
                }

                // Remove existing pending changes for this dropdown before adding any
                List<ProgramService> ssList = _db.ProgramService.Where(e => e.Program_Id == prog.Id).ToList();

                // If we have existing population items for this pending change, remove them first
                if (ssList.Count > 0)
                {
                    _db.ProgramService.RemoveRange(ssList);

                    var result4 = await _db.SaveChangesAsync();
                    //Console.WriteLine("-------------Removing existing ss from pending change");
                }

                if (model.Services_Supported_List != null)
                {
                    // Check each population type and add if it doesnt exist for this pending change
                    foreach (int s in model.Services_Supported_List)
                    {
                        Console.WriteLine("adding ss to pending program change: " + s);

                        ProgramService ss = new ProgramService
                        {
                            Program_Id = prog.Id,
                            Service_Id = s
                        };

                        _db.ProgramService.Add(ss);
                    }
                }

                // Remove existing pending changes for this dropdown before adding any
                List<ProgramDeliveryMethod> dmList = _db.ProgramDeliveryMethod.Where(e => e.Program_Id == prog.Id).ToList();

                // If we have existing population items for this pending change, remove them first
                if (dmList.Count > 0)
                {
                    _db.ProgramDeliveryMethod.RemoveRange(dmList);

                    var result5 = await _db.SaveChangesAsync();
                    //Console.WriteLine("-------------Removing existing dm from pending change");
                }

                if (model.Delivery_Method_List != null)
                {
                    // Check each population type and add if it doesnt exist for this pending change
                    foreach (int m in model.Delivery_Method_List)
                    {
                        //Console.WriteLine("adding dm to pending program change: " + m);

                        ProgramDeliveryMethod dm = new ProgramDeliveryMethod
                        {
                            Program_Id = prog.Id,
                            Delivery_Method_Id = m
                        };

                        _db.ProgramDeliveryMethod.Add(dm);
                    }
                }

                // Update pending change and the updated dropdown values
                pendingChange.Is_Active = model.Is_Active;
                pendingChange.Program_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Name));
                pendingChange.Organization_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Organization_Name));
                pendingChange.Lhn_Intake_Ticket_Id = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Lhn_Intake_Ticket_Id));
                pendingChange.Has_Intake = model.Has_Intake;
                pendingChange.Intake_Form_Version = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Intake_Form_Version));
                pendingChange.Qp_Intake_Submission_Id = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Qp_Intake_Submission_Id));
                pendingChange.Location_Details_Available = model.Location_Details_Available;
                pendingChange.Has_Consent = model.Has_Consent;
                pendingChange.Qp_Location_Submission_Id = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Qp_Location_Submission_Id));
                pendingChange.Lhn_Location_Ticket_Id = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Lhn_Location_Ticket_Id));
                pendingChange.Has_Multiple_Locations = model.Has_Multiple_Locations;
                pendingChange.Reporting_Form_2020 = model.Reporting_Form_2020;
                pendingChange.Nationwide = model.Nationwide;
                pendingChange.Online = model.Online;
                pendingChange.Participation_Populations = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Participation_Populations));
                //prog.Delivery_Method = model.Delivery_Method;//PreventNullString(model.Delivery_Method);
                pendingChange.States_Of_Program_Delivery = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.States_Of_Program_Delivery));
                pendingChange.Program_Duration = model.Program_Duration;
                pendingChange.Support_Cohorts = model.Support_Cohorts;
                pendingChange.Opportunity_Type = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Opportunity_Type));
                pendingChange.Job_Family = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Job_Family));
                pendingChange.Services_Supported = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Services_Supported));
                pendingChange.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Enrollment_Dates));
                pendingChange.Program_Url = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Url));
                pendingChange.Program_Status = model.Program_Status;
                pendingChange.Admin_Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Admin_Poc_First_Name));
                pendingChange.Admin_Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Admin_Poc_Last_Name));
                pendingChange.Admin_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Admin_Poc_Email));
                pendingChange.Admin_Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Admin_Poc_Phone));
                pendingChange.Public_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Public_Poc_Name));
                pendingChange.Public_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Public_Poc_Email));
                pendingChange.Notes = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Notes));
                //prog.For_Spouses = model.For_Spouses;
                //Console.WriteLine("program for spouses being set to: " + newForSpouses);
                pendingChange.For_Spouses = newForSpouses;
                pendingChange.Pending_Change_Status = 1;
                pendingChange.Last_Admin_Action_User = userName;
                pendingChange.Last_Admin_Action_Time = DateTime.Now;
                pendingChange.Last_Admin_Action_Type = "Approved";
                pendingChange.Rejection_Reason = "";
                _db.PendingProgramChanges.Update(pendingChange);

                var result6 = await _db.SaveChangesAsync();

                if (result6 > 0)
                {
                    // Identify if we have a valid email to notify
                    if (pendingChange.Updated_By != "Ingest")
                    {
                        // Get email address of user in updated_by field
                        ApplicationUser u = await _userManager.FindByNameAsync(pendingChange.Updated_By);
                        if (u == null)
                        {
                            u = await _userManager.FindByEmailAsync(pendingChange.Updated_By);
                        }

                        string email = u.Email;

                        if (u != null)
                        {
                            if (IsValidEmail(email))
                            {
                                //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                                await _emailSender.SendEmailAsync(email, "SkillBridge System Program Change Accepted", "The update to your organization's information has been made.<br/><br/>The SkillBridge website should reflect the change after the next site update, which should be no later than Friday, at 5:00 PM EST<br/><br/>Organization: " + org.Name + "<br/>Program: " + prog.Program_Name);
                            }
                        }
                    }

                    if (updateEnabledFields)
                    {
                        var oppsToUpdate = _db.Opportunities.Where(p => p.Program_Id == int.Parse(progId));

                        if (oppsToUpdate.ToList<OpportunityModel>().Count > 0)
                        {
                            foreach (OpportunityModel o in oppsToUpdate)
                            {
                                o.Is_Active = false;
                                o.Date_Deactivated = DateTime.Now;
                                _db.Opportunities.Update(o);
                            }
                        }
                    }

                    // We need to update the optimization fields for programs and opportunities now if the name changed
                    if (updateOptimizationFields)
                    {
                        var oppsToUpdate = _db.Opportunities.Where(p => p.Program_Id == int.Parse(progId));

                        if (oppsToUpdate.ToList<OpportunityModel>().Count > 0)
                        {
                            foreach (OpportunityModel o in oppsToUpdate)
                            {
                                o.Program_Name = prog.Program_Name;
                                _db.Opportunities.Update(o);
                            }
                        }
                    }

                    if (updateOnlineNationwideFields)
                    {
                        var oppsToUpdate = _db.Opportunities.Where(p => p.Program_Id == int.Parse(progId));

                        if (oppsToUpdate.ToList<OpportunityModel>().Count > 0)
                        {
                            foreach (OpportunityModel o in oppsToUpdate)
                            {
                                o.Nationwide = prog.Nationwide;
                                o.Online = prog.Online;
                                _db.Opportunities.Update(o);
                            }
                        }
                    }

                    var result7 = await _db.SaveChangesAsync();

                    if (result7 > 0)
                    {
                        List<OpportunityModel> relatedOpps = _db.Opportunities.Where(p => p.Program_Id == int.Parse(progId)).ToList();
                        new UpdateStatesOfProgramDeliveryCommand().Execute(prog, relatedOpps, _db);
                        
                        new UpdateJobFamilyListForOppsCommand().Execute(prog, relatedOpps, _db);

                        return RedirectToAction("ListPendingProgramChanges");
                    }
                }
            }
            else
            {

            }

            return RedirectToAction("ListPendingProgramChanges", "Analyst");
        }

        // Accept the reviewed change
        [HttpPost]
        public async Task<IActionResult> ReviewPendingOpportunityChange(EditOpportunityModel model, TrainingPlan tp, string oppId)
        {
            // Find this specific pending change
            var pendingChange = _db.PendingOpportunityChanges.FirstOrDefault(e => e.Opportunity_Id == int.Parse(oppId) && e.Pending_Change_Status == 0);

            OpportunityModel opp = _db.Opportunities.FirstOrDefault(e => e.Id == int.Parse(oppId));

            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == opp.Program_Id);

            string userName = HttpContext.User.Identity.Name;

            // Update the organization with the data from the reviewed change
            if (opp != null)
            {
                opp.Group_Id = model.Group_Id;
                opp.Program_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Name));
                opp.Is_Active = model.Is_Active;
                opp.Opportunity_Url = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Opportunity_Url));
                opp.Date_Program_Initiated = model.Date_Program_Initiated;
                opp.Date_Created = model.Date_Created; // Date opportunity was created in system
                opp.Date_Updated = DateTime.Now; // Date opportunity was last edited/updated in the system
                opp.Employer_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Employer_Poc_Name));
                opp.Employer_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Employer_Poc_Email));
                opp.Training_Duration = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Training_Duration));
                opp.Service = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Service));
                opp.Delivery_Method = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Delivery_Method));
                opp.Multiple_Locations = model.Multiple_Locations;
                opp.Program_Type = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Type));
                opp.Job_Families = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Job_Families));
                opp.Participation_Populations = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Participation_Populations));
                opp.Support_Cohorts = model.Support_Cohorts;
                opp.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Enrollment_Dates));
                opp.Mous = model.Mous;
                opp.Num_Locations = model.Num_Locations;
                opp.Installation = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Installation));
                opp.City = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.City));
                opp.State = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.State));
                opp.Zip = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Zip));
                opp.Lat = model.Lat;
                opp.Long = model.Long;
                opp.Nationwide = prog.Nationwide;
                opp.Online = prog.Online;
                opp.Summary_Description = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Summary_Description));
                opp.Jobs_Description = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Jobs_Description));
                opp.Links_To_Prospective_Jobs = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Links_To_Prospective_Jobs));
                opp.Locations_Of_Prospective_Jobs_By_State = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Locations_Of_Prospective_Jobs_By_State));
                opp.Salary = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Salary));
                opp.Prospective_Job_Labor_Demand = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Prospective_Job_Labor_Demand));
                opp.Target_Mocs = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Target_Mocs));
                opp.Other_Eligibility_Factors = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Other_Eligibility_Factors));
                opp.Cost = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Cost));
                opp.Other = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Other));
                opp.Notes = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Notes));
                opp.Created_By = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Created_By));
                opp.Updated_By = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Updated_By));
                opp.For_Spouses = model.For_Spouses;
                opp.Legacy_Opportunity_Id = model.Legacy_Opportunity_Id;
                opp.Legacy_Program_Id = model.Legacy_Program_Id;
                opp.Legacy_Provider_Id = model.Legacy_Provider_Id;
            }

            _db.Opportunities.Update(opp);

            var result1 = await _db.SaveChangesAsync();

            if (result1 > 0)
            {
                // Update pending change
                pendingChange.Program_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Name));
                pendingChange.Is_Active = model.Is_Active;
                pendingChange.Opportunity_Url = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Opportunity_Url));
                pendingChange.Date_Program_Initiated = model.Date_Program_Initiated;
                pendingChange.Employer_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Employer_Poc_Name));
                pendingChange.Employer_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Employer_Poc_Email));
                pendingChange.Training_Duration = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Training_Duration));
                pendingChange.Service = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Service));
                pendingChange.Delivery_Method = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Delivery_Method));
                pendingChange.Multiple_Locations = model.Multiple_Locations;
                pendingChange.Program_Type = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Program_Type));
                pendingChange.Job_Families = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Job_Families));
                pendingChange.Participation_Populations = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Participation_Populations));
                pendingChange.Support_Cohorts = model.Support_Cohorts;
                pendingChange.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Enrollment_Dates));
                pendingChange.Mous = model.Mous;
                pendingChange.Num_Locations = model.Num_Locations;
                pendingChange.Installation = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Installation));
                pendingChange.City = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.City));
                pendingChange.State = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.State));
                pendingChange.Zip = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Zip));
                pendingChange.Lat = model.Lat;
                pendingChange.Long = model.Long;
                pendingChange.Nationwide = prog.Nationwide;
                pendingChange.Online = prog.Online;
                pendingChange.Summary_Description = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Summary_Description));
                pendingChange.Jobs_Description = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Jobs_Description));
                pendingChange.Links_To_Prospective_Jobs = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Links_To_Prospective_Jobs));
                pendingChange.Locations_Of_Prospective_Jobs_By_State = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Locations_Of_Prospective_Jobs_By_State));
                pendingChange.Salary = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Salary));
                pendingChange.Prospective_Job_Labor_Demand = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Prospective_Job_Labor_Demand));
                pendingChange.Target_Mocs = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Target_Mocs));
                pendingChange.Other_Eligibility_Factors = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Other_Eligibility_Factors));
                pendingChange.Cost = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Cost));
                pendingChange.Other = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Other));
                pendingChange.Notes = GlobalFunctions.RemoveSpecialCharacters(PreventNullString(model.Notes));
                pendingChange.For_Spouses = model.For_Spouses;
                pendingChange.Pending_Change_Status = 1;
                pendingChange.Last_Admin_Action_User = userName;
                pendingChange.Last_Admin_Action_Time = DateTime.Now;
                pendingChange.Last_Admin_Action_Type = "Approved";
                pendingChange.Rejection_Reason = "";
                _db.PendingOpportunityChanges.Update(pendingChange);

                var result2 = await _db.SaveChangesAsync();

                if (result2 > 0)
                {
                    // Save training plan, if it exists
                    List<OpportunityModel> opps = _db.Opportunities.Where(e => e.Program_Id == opp.Program_Id).ToList();
                    OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == opp.Organization_Id);


                    /* COMMENTED FOR STATES_OF_PROGRAM_DELIVERY CHANGE BEFORE DUAL ENTRY*/
                     
                    // TODO: Inject
                    new UpdateStatesOfProgramDeliveryCommand().Execute(prog, opps, _db);

                    new UpdateOrgStatesOfProgramDeliveryCommand().Execute(org, _db);

                    // Identify if we have a valid email to notify
                    if (pendingChange.Updated_By != "Ingest")
                    {
                        // Get email address of user in updated_by field
                        ApplicationUser u = await _userManager.FindByNameAsync(pendingChange.Updated_By);
                        if (u == null)
                        {
                            u = await _userManager.FindByEmailAsync(pendingChange.Updated_By);
                        }

                        string email = u.Email;

                        if (u != null)
                        {
                            if (IsValidEmail(email))
                            {
                                //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                                await _emailSender.SendEmailAsync(email, "SkillBridge System Opportunity Change Accepted", "The update to your organization's information has been made.<br/><br/>The SkillBridge website should reflect the change after the next site update, which should be no later than Friday, at 5:00 PM EST<br/><br/>Organization: " + org.Name + "<br/>Program: " + prog.Program_Name);
                            }
                        }
                    }

                    return RedirectToAction("ListPendingOpportunityChanges");
                }
            }
            else
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors)
                           .ToList();

                ViewBag.ModelStateErrors = errors;
            }

            return ListPendingOpportunityAdditions();
        }

        // Accept the reviewed addition
        [HttpPost]
        public async Task<IActionResult> ReviewPendingProgramAddition(EditProgramModel model, TrainingPlan tp, string pendingId)
        {
            // Find this specific pending change
            PendingProgramAdditionModel pendingAddition = _db.PendingProgramAdditions.FirstOrDefault(e => e.Id == int.Parse(pendingId) && e.Pending_Change_Status == 0);
            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == model.Organization_Id);
            var mou = _db.Mous.FirstOrDefault(e => e.Id == org.Mou_Id);

            //ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == int.Parse(progId));

            bool newForSpouses = false;

            string userName = HttpContext.User.Identity.Name;

            if (ModelState.IsValid)
            {
                try
                {
                    ProgramModel prog = new ProgramModel
                    {
                        Program_Name = GlobalFunctions.RemoveSpecialCharacters(model.Program_Name),
                        Program_Status = model.Program_Status,
                        Organization_Id = model.Organization_Id,
                        Organization_Name = GlobalFunctions.RemoveSpecialCharacters(org.Name),
                        Is_Active = model.Is_Active,
                        Created_By = userName,
                        Updated_By = userName,
                        Date_Authorized = mou.Creation_Date,   // Date the 
                        Mou_Creation_Date = mou.Creation_Date,
                        Mou_Expiration_Date = mou.Expiration_Date,
                        Mou_Link = mou.Url,
                        Date_Created = DateTime.Now,  // Date program was created in system
                        Date_Updated = DateTime.Now,  // Date program was last edited/updated in the system
                        Program_Duration = model.Program_Duration,
                        Opportunity_Type = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Type),
                        Admin_Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_First_Name),
                        Admin_Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Last_Name),
                        Admin_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Email),
                        Admin_Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Phone),
                        Public_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(model.Public_Poc_Name),
                        Public_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Public_Poc_Email),
                        //Delivery_Method = model.Delivery_Method,
                        //Services_Supported = model.Services_Supported,
                        Legacy_Program_Id = -1,
                        Legacy_Provider_Id = org.Legacy_Provider_Id,
                        // Generate these
                        Participation_Populations = "",
                        Job_Family = "",
                        Services_Supported = "",
                        Intake_Form_Version = model.Intake_Form_Version != "" ? GlobalFunctions.RemoveSpecialCharacters(model.Intake_Form_Version) : "N/A",
                        Qp_Intake_Submission_Id = model.Qp_Intake_Submission_Id != "" ? GlobalFunctions.RemoveSpecialCharacters(model.Qp_Intake_Submission_Id) : "N/A",
                        Qp_Location_Submission_Id = model.Qp_Location_Submission_Id != "" ? GlobalFunctions.RemoveSpecialCharacters(model.Qp_Location_Submission_Id) : "N/A",
                        Lhn_Location_Ticket_Id = model.Lhn_Location_Ticket_Id != "" ? GlobalFunctions.RemoveSpecialCharacters(model.Lhn_Location_Ticket_Id) : "N/A",
                        Delivery_Method = "",
                        Nationwide = model.Online == true ? true : false,   // Setting up nationwide for the first time, if online is true or more than 3 states (unknown at this point)
                        Support_Cohorts = model.Support_Cohorts,
                        Online = model.Online,
                        Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(model.Enrollment_Dates),
                        Program_Url = GlobalFunctions.RemoveSpecialCharacters(model.Program_Url),
                        States_Of_Program_Delivery = GlobalFunctions.RemoveSpecialCharacters(model.States_Of_Program_Delivery),
                        Has_Consent = model.Has_Consent,
                        Has_Multiple_Locations = model.Has_Multiple_Locations,
                        Reporting_Form_2020 = model.Reporting_Form_2020,
                        Has_Intake = model.Has_Intake,
                        Location_Details_Available = model.Location_Details_Available,
                        For_Spouses = model.For_Spouses,
                        Notes = model.Notes
                    };

                    // Save to DB
                    if (model.Lhn_Intake_Ticket_Id == null) { prog.Lhn_Intake_Ticket_Id = ""; }

                    if (model.Intake_Form_Version == null) { prog.Intake_Form_Version = ""; }

                    if (model.Qp_Intake_Submission_Id == null) { prog.Qp_Intake_Submission_Id = ""; }

                    if (model.Qp_Intake_Submission_Id == null) { prog.Qp_Intake_Submission_Id = ""; }

                    if (model.Qp_Location_Submission_Id == null) { prog.Qp_Location_Submission_Id = ""; }

                    if (model.Lhn_Location_Ticket_Id == null) { prog.Lhn_Location_Ticket_Id = ""; }

                    if (model.Mou_Link == null) { prog.Mou_Link = ""; }

                    if (model.States_Of_Program_Delivery == null) { prog.States_Of_Program_Delivery = ""; }

                    if (model.Job_Family == null) { prog.Job_Family = ""; }

                    if (model.Services_Supported == null) { prog.Services_Supported = ""; }

                    if (model.Enrollment_Dates == null) { prog.Enrollment_Dates = ""; }

                    if (model.Program_Url == null) { prog.Program_Url = ""; }

                    if (model.Notes == null) { prog.Notes = ""; }

                    _db.Programs.Add(prog);
                    var result = await _db.SaveChangesAsync();

                    if (result >= 1)
                    {
                        if (!String.IsNullOrWhiteSpace(pendingAddition.SerializedTrainingPlan))
                        {
                            var repository = new TrainingPlanRepository(_db);

                            // Set the training plan ID to zero so it always gets added
                            tp.Id = 0;

                            // Pull in any changes from the form
                            foreach (var instructionalMethodId in Request.Form["TrainingPlanInstructionalMethods[].InstructionalMethodId"])
                            {
                                tp.TrainingPlanInstructionalMethods.Add(new TrainingPlanInstructionalMethod
                                {
                                    InstructionalMethodId = int.Parse(instructionalMethodId),
                                    OtherText = (instructionalMethodId == "5" ? Request.Form["TrainingPlanInstructionalMethods[].OtherText"] : String.Empty)
                                });
                            }

                            for (var i = 0; i < tp.BreakdownCount; i++)
                            {
                                tp.TrainingPlanBreakdowns.Add(new TrainingPlanBreakdown
                                {
                                    RowId = int.Parse(Request.Form[$"TrainingPlanBreakdowns[{i + 1}].RowId"]),
                                    TrainingModuleTitle = Request.Form[$"TrainingPlanBreakdowns[{i + 1}].TrainingModuleTitle"],
                                    LearningObjective = Request.Form[$"TrainingPlanBreakdowns[{i + 1}].LearningObjective"],
                                    TotalHours = decimal.Parse(Request.Form[$"TrainingPlanBreakdowns[{i + 1}].TotalHours"]),
                                });
                            }


                            // Save the training plan
                            tp = await repository.SaveTrainingPlanAsync(tp, userName);
                            pendingAddition.SerializedTrainingPlan = Newtonsoft.Json.JsonConvert.SerializeObject(tp);

                            // Assign the training plan to the program
                            await repository.SaveTrainingPlanToProgramAsync(prog.Id, tp.Id, userName);
                        }

                        // Participation Populations
                        if (model.Populations_List != null)
                        {
                            Console.WriteLine("Posted program has participation populations of: " + model.Populations_List);
                            Console.WriteLine("Posted program has participation populations count of: " + model.Populations_List.Count);

                            foreach (int p in model.Populations_List)
                            {
                                Console.WriteLine("adding population to program: " + p);

                                ProgramParticipationPopulation pp = new ProgramParticipationPopulation
                                {
                                    Program_Id = prog.Id,
                                    Participation_Population_Id = p
                                };

                                _db.ProgramParticipationPopulation.Add(pp);
                            }
                        }

                        // Job Families
                        if (model.Job_Family_List != null)
                        {
                            Console.WriteLine("Posted program has job family of: " + model.Job_Family_List);
                            Console.WriteLine("Posted program has job family count of: " + model.Job_Family_List.Count);

                            foreach (int j in model.Job_Family_List)
                            {
                                Console.WriteLine("adding job family to program: " + j);

                                ProgramJobFamily jf = new ProgramJobFamily
                                {
                                    Program_Id = prog.Id,
                                    Job_Family_Id = j
                                };

                                _db.ProgramJobFamily.Add(jf);
                            }
                        }

                        // Services Supported
                        if (model.Services_Supported_List != null)
                        {
                            Console.WriteLine("Posted program has service of: " + model.Services_Supported_List);
                            Console.WriteLine("Posted program has service count of: " + model.Services_Supported_List.Count);

                            foreach (int s in model.Services_Supported_List)
                            {
                                Console.WriteLine("adding service to program: " + s);

                                ProgramService ps = new ProgramService
                                {
                                    Program_Id = prog.Id,
                                    Service_Id = s
                                };

                                _db.ProgramService.Add(ps);
                            }
                        }

                        // Delivery Method
                        if (model.Delivery_Method_List != null)
                        {
                            Console.WriteLine("Posted program has delivery method of: " + model.Delivery_Method_List);
                            Console.WriteLine("Posted program has delivery method count of: " + model.Delivery_Method_List.Count);

                            foreach (int m in model.Delivery_Method_List)
                            {
                                Console.WriteLine("adding delivery method to program: " + m);

                                ProgramDeliveryMethod dm = new ProgramDeliveryMethod
                                {
                                    Program_Id = prog.Id,
                                    Delivery_Method_Id = m
                                };

                                _db.ProgramDeliveryMethod.Add(dm);
                            }
                        }

                        pendingAddition.Program_Status = model.Program_Status;
                        pendingAddition.Is_Active = model.Is_Active;
                        pendingAddition.Program_Duration = model.Program_Duration;
                        pendingAddition.Opportunity_Type = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Type);
                        pendingAddition.Admin_Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_First_Name);
                        pendingAddition.Admin_Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Last_Name);
                        pendingAddition.Admin_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Email);
                        pendingAddition.Admin_Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Phone);
                        pendingAddition.Public_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(model.Public_Poc_Name);
                        pendingAddition.Public_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Public_Poc_Email);
                        //Delivery_Method = model.Delivery_Method,
                        //Services_Supported = model.Services_Supported,
                        // Generate these
                        pendingAddition.Participation_Populations = "";
                        pendingAddition.Job_Family = "";
                        pendingAddition.Services_Supported = "";
                        pendingAddition.Intake_Form_Version = model.Intake_Form_Version != "" ? GlobalFunctions.RemoveSpecialCharacters(model.Intake_Form_Version) : "N/A";
                        pendingAddition.Qp_Intake_Submission_Id = model.Qp_Intake_Submission_Id != "" ? GlobalFunctions.RemoveSpecialCharacters(model.Qp_Intake_Submission_Id) : "N/A";
                        pendingAddition.Qp_Location_Submission_Id = model.Qp_Location_Submission_Id != "" ? GlobalFunctions.RemoveSpecialCharacters(model.Qp_Location_Submission_Id) : "N/A";
                        pendingAddition.Lhn_Location_Ticket_Id = model.Lhn_Location_Ticket_Id != "" ? GlobalFunctions.RemoveSpecialCharacters(model.Lhn_Location_Ticket_Id) : "N/A";
                        pendingAddition.Delivery_Method = "";
                        pendingAddition.Nationwide = model.Nationwide;
                        pendingAddition.Support_Cohorts = model.Support_Cohorts;
                        pendingAddition.Online = model.Online;
                        pendingAddition.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(model.Enrollment_Dates);
                        pendingAddition.Program_Url = GlobalFunctions.RemoveSpecialCharacters(model.Program_Url);
                        pendingAddition.States_Of_Program_Delivery = GlobalFunctions.RemoveSpecialCharacters(model.States_Of_Program_Delivery);
                        pendingAddition.Has_Consent = model.Has_Consent;
                        pendingAddition.Has_Multiple_Locations = model.Has_Multiple_Locations;
                        pendingAddition.Reporting_Form_2020 = model.Reporting_Form_2020;
                        pendingAddition.Has_Intake = model.Has_Intake;
                        pendingAddition.Location_Details_Available = model.Location_Details_Available;
                        pendingAddition.For_Spouses = model.For_Spouses;
                        pendingAddition.Notes = model.Notes;
                        pendingAddition.Pending_Change_Status = 1;
                        pendingAddition.Last_Admin_Action_User = userName;
                        pendingAddition.Last_Admin_Action_Time = DateTime.Now;
                        pendingAddition.Last_Admin_Action_Type = "Approved";
                        pendingAddition.Rejection_Reason = "";

                        _db.PendingProgramAdditions.Update(pendingAddition);

                        //Update optimized fields

                        var result1 = await _db.SaveChangesAsync();

                        if (result1 >= 1)
                        {
                            // Identify if we have a valid email to notify
                            if (pendingAddition.Updated_By != "Ingest")
                            {
                                // Get email address of user in updated_by field
                                ApplicationUser u = await _userManager.FindByNameAsync(pendingAddition.Updated_By);
                                if (u == null)
                                {
                                    u = await _userManager.FindByEmailAsync(pendingAddition.Updated_By);
                                }

                                string email = u.Email;

                                if (u != null)
                                {
                                    if (IsValidEmail(email))
                                    {
                                        //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                                        await _emailSender.SendEmailAsync(email, "SkillBridge System Program Addition Accepted", "The update to your organization's information has been made.<br/><br/>The SkillBridge website should reflect the change after the next site update, which should be no later than Friday, at 5:00 PM EST");
                                    }
                                }
                            }

                            prog.Services_Supported = GetServiceListForProg(prog);
                            prog.Job_Family = GetJobFamiliesListForProg(prog);
                            prog.Participation_Populations = GetParticipationPopulationStringFromProgram(prog);

                            pendingAddition.Services_Supported = prog.Services_Supported;
                            pendingAddition.Job_Family = prog.Job_Family;
                            pendingAddition.Participation_Populations = prog.Participation_Populations;


                            _db.Programs.Update(prog);
                            

                            var result2 = await _db.SaveChangesAsync();

                            if (result2 >= 1)
                            {
                                return RedirectToAction("ListPendingProgramAdditions", "Analyst");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " - " + ex.StackTrace);
                }
            }

            return RedirectToAction("ListPendingProgramAdditions", "Analyst");
        }

        [HttpPost]
        public async Task<IActionResult> ReviewPendingOpportunityAddition(OpportunityModel model, TrainingPlan tp, string pendingId, string organizationId, string programId)
        {
            //Console.WriteLine("Create Opportunity posted");
            //Console.WriteLine("newGroupTitle: " + newGroupTitle);
            string userName = HttpContext.User.Identity.Name;

            //Console.WriteLine("model.Program_Id: " + model.Program_Id);
            var pendingAddition = _db.PendingOpportunityAdditions.FirstOrDefault(e => e.Id == int.Parse(pendingId) && e.Pending_Change_Status == 0);
            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == int.Parse(programId));
            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == int.Parse(organizationId));
            var mou = _db.Mous.FirstOrDefault(e => e.Id == org.Mou_Id);
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
                    string newProgType = "";

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

                    model.Program_Type = newProgType;
                    model.Mou_Link = prog.Mou_Link;

                    if (prog.Legacy_Program_Id != 0 && prog.Legacy_Program_Id != -1)
                    {
                        model.Legacy_Program_Id = prog.Legacy_Program_Id;
                    }
                    else
                    {
                        model.Legacy_Program_Id = 0;
                    }

                    if (prog.Legacy_Provider_Id != 0 && prog.Legacy_Provider_Id != -1)
                    {
                        model.Legacy_Provider_Id = prog.Legacy_Provider_Id;
                    }
                    else
                    {
                        model.Legacy_Provider_Id = 0;
                    }

                    OpportunityModel opp = new OpportunityModel
                    {
                        // Save to DB
                        Created_By = userName,
                        Updated_By = userName,
                        Date_Created = DateTime.Now,
                        Date_Updated = DateTime.Now,
                        Organization_Id = int.Parse(organizationId),
                        Opportunity_Url = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Url),
                        //Organization_Name = org.Name,
                        Date_Program_Initiated = prog.Date_Created,
                        Program_Name = prog.Program_Name,
                        Program_Id = int.Parse(programId),
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
                        Admin_Poc_Email = prog.Admin_Poc_Email,
                        Admin_Poc_First_Name = prog.Admin_Poc_First_Name,
                        Admin_Poc_Last_Name = prog.Admin_Poc_Last_Name,
                        Organization_Name = org.Name,
                        Mou_Link = mou.Url
                    };

                    //model.Admin_Poc_Email = model.Admin_Poc_Email;
                    //model.Admin_Poc_Email = model.Admin_Poc_Email;
                    //model.Admin_Poc_Email = model.Admin_Poc_Email;
                    //model.Mou_Link = model.Mou_Link;
                    _db.Opportunities.Add(opp);
                    var result1 = await _db.SaveChangesAsync();

                    if (result1 > 0)
                    {
                        Console.WriteLine("Result1 returned > 0");

                        // Identify if we have a valid email to notify
                        if (pendingAddition.Updated_By != "Ingest")
                        {
                            // Get email address of user in updated_by field
                            ApplicationUser u = await _userManager.FindByNameAsync(pendingAddition.Updated_By);
                            if (u == null)
                            {
                                u = await _userManager.FindByEmailAsync(pendingAddition.Updated_By);
                            }

                            if (u != null)
                            {
                                string email = u.Email;

                                if (IsValidEmail(email))
                                {
                                    //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                                    await _emailSender.SendEmailAsync(email, "SkillBridge System Opportunity Addition Accepted", "The update to your organization's information has been made.<br/><br/>The SkillBridge website should reflect the change after the next site update, which should be no later than Friday, at 5:00 PM EST");
                                }
                            }
                        }

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
                        pendingAddition.Opportunity_Url = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Url);
                        //Organization_Name = org.Name,
                        pendingAddition.Date_Program_Initiated = prog.Date_Created;
                        pendingAddition.Job_Families = newJobFamilies;  // This is just an optimized field
                        pendingAddition.Participation_Populations = newPartPop;   // This is just an optimized field
                        pendingAddition.Employer_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Employer_Poc_Email);
                        pendingAddition.Employer_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(model.Employer_Poc_Name);
                        pendingAddition.Training_Duration = GlobalFunctions.RemoveSpecialCharacters(model.Training_Duration);
                        pendingAddition.Delivery_Method = GlobalFunctions.RemoveSpecialCharacters(model.Delivery_Method);
                        pendingAddition.Multiple_Locations = model.Multiple_Locations;
                        pendingAddition.Program_Type = GlobalFunctions.RemoveSpecialCharacters(model.Program_Type);
                        pendingAddition.Support_Cohorts = model.Support_Cohorts;
                        pendingAddition.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(model.Enrollment_Dates);
                        pendingAddition.Mous = model.Mous;
                        pendingAddition.Num_Locations = model.Num_Locations;
                        pendingAddition.Installation = GlobalFunctions.RemoveSpecialCharacters(model.Installation);
                        pendingAddition.City = GlobalFunctions.RemoveSpecialCharacters(model.City);
                        pendingAddition.State = GlobalFunctions.RemoveSpecialCharacters(model.State);
                        pendingAddition.Zip = GlobalFunctions.RemoveSpecialCharacters(model.Zip);
                        pendingAddition.Lat = model.Lat;
                        pendingAddition.Long = model.Long;
                        pendingAddition.Nationwide = prog.Nationwide;
                        pendingAddition.Online = prog.Online;
                        pendingAddition.Summary_Description = GlobalFunctions.RemoveSpecialCharacters(model.Summary_Description);
                        pendingAddition.Jobs_Description = GlobalFunctions.RemoveSpecialCharacters(model.Jobs_Description);
                        pendingAddition.Links_To_Prospective_Jobs = GlobalFunctions.RemoveSpecialCharacters(model.Links_To_Prospective_Jobs);
                        pendingAddition.Prospective_Job_Labor_Demand = GlobalFunctions.RemoveSpecialCharacters(model.Prospective_Job_Labor_Demand);
                        pendingAddition.Locations_Of_Prospective_Jobs_By_State = GlobalFunctions.RemoveSpecialCharacters(model.Locations_Of_Prospective_Jobs_By_State);
                        pendingAddition.Salary = GlobalFunctions.RemoveSpecialCharacters(model.Salary);
                        pendingAddition.Cost = GlobalFunctions.RemoveSpecialCharacters(model.Cost);
                        pendingAddition.Target_Mocs = GlobalFunctions.RemoveSpecialCharacters(model.Target_Mocs);
                        pendingAddition.Other_Eligibility_Factors = GlobalFunctions.RemoveSpecialCharacters(model.Other_Eligibility_Factors);
                        pendingAddition.Other = GlobalFunctions.RemoveSpecialCharacters(model.Other);
                        pendingAddition.Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes);
                        pendingAddition.For_Spouses = model.For_Spouses;
                        pendingAddition.Is_Active = model.Is_Active;
                        pendingAddition.Service = GlobalFunctions.RemoveSpecialCharacters(model.Service);
                        pendingAddition.Pending_Change_Status = 1;
                        pendingAddition.Last_Admin_Action_User = userName;
                        pendingAddition.Last_Admin_Action_Time = DateTime.Now;
                        pendingAddition.Last_Admin_Action_Type = "Approved";
                        pendingAddition.Rejection_Reason = "";
                        _db.PendingOpportunityAdditions.Update(pendingAddition);

                        var result2 = await _db.SaveChangesAsync();

                        if (result2 > 0)
                        {
                            //Console.WriteLine("Result2 returned > 0");
                            List<OpportunityModel> opps = _db.Opportunities.Where(e => e.Program_Id == int.Parse(programId)).ToList();
                            ProgramModel prog2 = _db.Programs.FirstOrDefault(e => e.Id == int.Parse(programId));
                            OrganizationModel org2 = _db.Organizations.FirstOrDefault(e => e.Id == int.Parse(organizationId));


                            // TODO: Inject
                            new UpdateStatesOfProgramDeliveryCommand().Execute(prog2, opps, _db);

                            new UpdateOrgStatesOfProgramDeliveryCommand().Execute(org2, _db);

                            return RedirectToAction("ListPendingOpportunityAdditions", "Analyst");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " - " + ex.StackTrace);
                }
            }
            else
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors)
                           .ToList();

                ViewBag.ModelStateErrors = errors;
            }

            return ListPendingOpportunityAdditions();
        }

        // Reject the reviewed addition, with a reason
        [HttpPost]
        public async Task<IActionResult> RejectPendingProgAddition(EditProgramModel model, string pendingId)
        {
            // Find this specific pending addition
            PendingProgramAdditionModel pendingAddition = _db.PendingProgramAdditions.FirstOrDefault(e => e.Id == int.Parse(pendingId) && e.Pending_Change_Status == 0);

            string userName = HttpContext.User.Identity.Name;

            pendingAddition.Pending_Change_Status = 2;    // Change this pending addition to rejected
            pendingAddition.Last_Admin_Action_User = userName;
            pendingAddition.Last_Admin_Action_Time = DateTime.Now;
            pendingAddition.Last_Admin_Action_Type = "Rejected";
            pendingAddition.Rejection_Reason = model.Rejection_Reason;

            _db.PendingProgramAdditions.Update(pendingAddition);

            var result = await _db.SaveChangesAsync();

            if (result > 0)
            {
                // Identify if we have a valid email to notify
                if (pendingAddition.Updated_By != "Ingest")
                {
                    // Get email address of user in updated_by field
                    ApplicationUser u = await _userManager.FindByNameAsync(pendingAddition.Updated_By);
                    if (u == null)
                    {
                        u = await _userManager.FindByEmailAsync(pendingAddition.Updated_By);
                    }

                    string email = u.Email;

                    if (u != null)
                    {
                        if (IsValidEmail(email))
                        {
                            //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                            await _emailSender.SendEmailAsync(email, "SkillBridge System Program Addition Rejected", "This email is being sent to notify you that your recent SkillBridge Program addition for the " + model.Organization_Name + " Organization has been rejected.<br/><br/>Reason:<br/>" + model.Rejection_Reason);
                        }
                    }
                }
            }

            return RedirectToAction("ListPendingProgramAdditions", "Analyst");
        }

        // Reject the reviewed addition, with a reason
        [HttpPost]
        public async Task<IActionResult> RejectPendingOppAddition(EditOpportunityModel model, string pendingId)
        {
            // Find this specific pending addition
            var pendingAddition = _db.PendingOpportunityAdditions.FirstOrDefault(e => e.Id == int.Parse(pendingId) && e.Pending_Change_Status == 0);

            string userName = HttpContext.User.Identity.Name;

            pendingAddition.Pending_Change_Status = 2;    // Change this pending addition to rejected
            pendingAddition.Last_Admin_Action_User = userName;
            pendingAddition.Last_Admin_Action_Time = DateTime.Now;
            pendingAddition.Last_Admin_Action_Type = "Rejected";
            pendingAddition.Rejection_Reason = model.Rejection_Reason;

            _db.PendingOpportunityAdditions.Update(pendingAddition);

            var result = await _db.SaveChangesAsync();

            if (result > 0) // comes back as 2
            {
                // Identify if we have a valid email to notify
                if (pendingAddition.Updated_By != "Ingest")
                {
                    // Get email address of user in updated_by field
                    ApplicationUser u = await _userManager.FindByNameAsync(pendingAddition.Updated_By);
                    if (u == null)
                    {
                        u = await _userManager.FindByEmailAsync(pendingAddition.Updated_By);
                    }

                    string email = u.Email;

                    if (u != null)
                    {
                        if (IsValidEmail(email))
                        {
                            //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                            await _emailSender.SendEmailAsync(email, "SkillBridge System Opportunity Addition Rejected", "This email is being sent to notify you that your recent SkillBridge Opportunity addition for the " + model.Program_Name + " Program has been rejected.<br/><br/>Reason:<br/>" + model.Rejection_Reason);
                        }
                    }
                }
            }

            return RedirectToAction("ListPendingOpportunityAdditions", "Analyst");
        }

        public string GetProgramServiceNameFromId(int id)
        {
            string name = "";

            Service service = _db.Services.FirstOrDefault(e => e.Id == id);

            if (service != null)
            {
                name = service.Name;
            }

            return name;
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
        /*
        private void UpdateOrgStatesOfProgramDelivery(OrganizationModel org)
        {
            // Get all programs from org
            List<ProgramModel> progs = _db.Programs.Where(e => e.Organization_Id == org.Id).ToList();

            List<string> states = new List<string>();
            

            foreach(ProgramModel p in progs)
            {
                string progStates = "";
                progStates = p.States_Of_Program_Delivery;
                Console.WriteLine("progStates: " + progStates);

                // Split out each programs states of program delivery, and add them to the states array
                progStates = progStates.Replace(" ", "");
                string[] splitStates = progStates.Split(",");

                foreach(string s in splitStates)
                {
                    if(s != "" && s != " ")
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

            foreach(string s in states)
            {
                Console.WriteLine("Checking state s: " + s);

                if(count == 0)
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
            foreach(OpportunityModel o in opps)
            {
                bool found = false;

                foreach(string s in states)
                {
                    if(s == o.State)
                    {
                        found = true;
                        continue;
                    }
                }

                if(found == false)
                {
                    if(o.State != "" && o.State != " ")
                    {
                        states.Add(o.State);
                    }
                }

                if (o.Is_Active == true)
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
            if (activeOppsCount > 1 || num > 1)
            {
                prog.Has_Multiple_Locations = true;
            }
            else
            {
                prog.Has_Multiple_Locations = false;
            }

            _db.SaveChanges();
        }*/

        public string PreventNullString(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                s = "";
            }
            return s;
        }

        // Reject the reviewed change, with a reason
        [HttpPost]
        public async Task<IActionResult> RejectPendingOrgChange(EditOrganizationModel model, string orgId)
        {
            // Find this specific pending change
            var pendingChange = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Organization_Id == int.Parse(orgId) && e.Pending_Change_Status == 0);

            string userName = HttpContext.User.Identity.Name;

            pendingChange.Pending_Change_Status = 2;    // Change this pending change to rejected
            pendingChange.Last_Admin_Action_User = userName;
            pendingChange.Last_Admin_Action_Time = DateTime.Now;
            pendingChange.Last_Admin_Action_Type = "Rejected";
            pendingChange.Rejection_Reason = model.Rejection_Reason;

            _db.PendingOrganizationChanges.Update(pendingChange);

            var result = await _db.SaveChangesAsync();

            if (result > 0)
            {
                // Identify if we have a valid email to notify
                if (pendingChange.Updated_By != "Ingest")
                {
                    // Get email address of user in updated_by field
                    ApplicationUser u = await _userManager.FindByNameAsync(pendingChange.Updated_By);
                    if (u == null)
                    {
                        u = await _userManager.FindByEmailAsync(pendingChange.Updated_By);
                    }

                    string email = u.Email;

                    if (u != null)
                    {
                        if (IsValidEmail(email))
                        {
                            //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                            await _emailSender.SendEmailAsync(email, "SkillBridge System Organization Change Rejected", "This email is being sent to notify you that your recent SkillBridge Organization update for the " + model.Name + " Organization has been rejected.<br/><br/>Reason:<br/>" + model.Rejection_Reason);
                        }
                    }
                }
            }

            return RedirectToAction("ListPendingOrganizationChanges", "Analyst");
        }

        // Reject the reviewed change, with a reason
        [HttpPost]
        public async Task<IActionResult> RejectPendingProgChange(EditProgramModel model, string progId)
        {
            // Find this specific pending change
            PendingProgramChangeModel pendingChange = _db.PendingProgramChanges.FirstOrDefault(e => e.Program_Id == int.Parse(progId) && e.Pending_Change_Status == 0);

            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == int.Parse(progId));

            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == prog.Organization_Id);

            string userName = HttpContext.User.Identity.Name;

            pendingChange.Pending_Change_Status = 2;    // Change this pending change to rejected
            pendingChange.Last_Admin_Action_User = userName;
            pendingChange.Last_Admin_Action_Time = DateTime.Now;
            pendingChange.Last_Admin_Action_Type = "Rejected";
            pendingChange.Rejection_Reason = model.Rejection_Reason;

            _db.PendingProgramChanges.Update(pendingChange);

            var result = await _db.SaveChangesAsync();

            if (result > 0)
            {
                // Identify if we have a valid email to notify
                if (pendingChange.Updated_By != "Ingest")
                {
                    // Get email address of user in updated_by field
                    ApplicationUser u = await _userManager.FindByNameAsync(pendingChange.Updated_By);
                    if (u == null)
                    {
                        u = await _userManager.FindByEmailAsync(pendingChange.Updated_By);
                    }

                    string email = u.Email;

                    if (u != null)
                    {
                        if (IsValidEmail(email))
                        {
                            //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                            await _emailSender.SendEmailAsync(email, "SkillBridge System Program Change Rejected", "This email is being sent to notify you that your recent SkillBridge Program update for the " + model.Program_Name + " Program has been rejected.<br/><br/>Reason:<br/>" + model.Rejection_Reason + "<br/><br/> Organization: " + org.Name + " < br /> Program: " + prog.Program_Name);
                        }
                    }
                }
            }

            return RedirectToAction("ListPendingProgramChanges", "Analyst");
        }

        public bool IsValidEmail(string emailaddress)
        {
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,7})+)$");
            Match match = regex.Match(emailaddress);
            if (match.Success)
                return true;
            else
                return false;
        }

        // Reject the reviewed change, with a reason
        [HttpPost]
        public async Task<IActionResult> RejectPendingOppChange(EditOpportunityModel model, string oppId)
        {
            // Find this specific pending change
            var pendingChange = _db.PendingOpportunityChanges.FirstOrDefault(e => e.Opportunity_Id == int.Parse(oppId) && e.Pending_Change_Status == 0);

            var originalOpportunity = _db.Opportunities.FirstOrDefault(e => e.Id == pendingChange.Opportunity_Id);

            var prog = _db.Programs.FirstOrDefault(e => e.Id == originalOpportunity.Program_Id);
            var org = _db.Organizations.FirstOrDefault(e => e.Id == prog.Organization_Id);

            string userName = HttpContext.User.Identity.Name;

            pendingChange.Pending_Change_Status = 2;    // Change this pending change to rejected
            pendingChange.Last_Admin_Action_User = userName;
            pendingChange.Last_Admin_Action_Time = DateTime.Now;
            pendingChange.Last_Admin_Action_Type = "Rejected";
            pendingChange.Rejection_Reason = model.Rejection_Reason;

            _db.PendingOpportunityChanges.Update(pendingChange);

            var result = await _db.SaveChangesAsync();

            if (result > 0) // comes back as 2
            {
                // Identify if we have a valid email to notify
                if (pendingChange.Updated_By != "Ingest")
                {
                    // Get email address of user in updated_by field
                    ApplicationUser u = await _userManager.FindByNameAsync(pendingChange.Updated_By);
                    if(u == null)
                    {
                        u = await _userManager.FindByEmailAsync(pendingChange.Updated_By);
                    }

                    string email = u.Email;

                    if (u != null)
                    {
                        if (IsValidEmail(email))
                        {
                            //Console.WriteLine("EMAIL NOTIFICATION BEING SENT TO " + email);
                            await _emailSender.SendEmailAsync(email, "SkillBridge System Opportunity Change Rejected", "This email is being sent to notify you that your recent SkillBridge Opportunity update for the " + model.Program_Name + " Program has been rejected.<br/><br/>Reason:<br/>" + model.Rejection_Reason + "<br/><br/>Organization: " + org.Name + "<br/>Program: " + prog.Program_Name);
                        }
                    }
                }
            }

            return RedirectToAction("ListPendingOpportunityChanges", "Analyst");
        }

        /* Users */
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            // Create model
            CreateUserModel model = new CreateUserModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
               Console.WriteLine("Model is valid");

                ApplicationUser identityUser = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Notes = model.Notes,
                    MustChangePassword = true   // Users must update passwords by default
                };

                Console.WriteLine("application user set");

                IdentityResult result = await _userManager.CreateAsync(identityUser, model.Password);

                if (result.Succeeded)
                {
                    ViewBag.Message = "You have successfully created this user.";
                    return await EditUser(identityUser.Id);
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id = { id } cannot be found";
                return View("NotFound");
            }

            ViewBag.UserAuthorities = await _db.AspNetUserAuthorities
                .Include(o => o.Organization)
                .Include(o => o.Program)
                .Where(o => o.ApplicationUserId == user.Id)
                .ToListAsync();
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Notes = user.Notes,
                OrganizationId = user.OrganizationId.ToString(),
                ProgramId = user.ProgramId.ToString(),
                Claims = userClaims.Select(c => c.Value).ToList(),
                Roles = userRoles
            };

            return View("~/Views/Analyst/EditUser.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id = { model.Id } cannot be found";
                return View("NotFound");
            }
            else
            {
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Notes = model.Notes;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    ViewBag.Message = "You have successfully updated this record.";
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ImpersonateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id = {id} cannot be found";
                return View("NotFound");
            }

            await _signInManager.SignInAsync(user, true);

            return Redirect("/");
        }

        [HttpGet]
        public async Task<IActionResult> Organizations(string term)
        {
            if (!String.IsNullOrWhiteSpace(term))
            {
                var orgs = await _db.Organizations.Where(o => o.Name.ToLower().Contains(term.ToLower())).OrderBy(o => o.Name).Select(o => new { id = o.Id, text = o.Name }).ToListAsync();
                return new JsonResult(new { results = orgs });
            }
            else
            {
                return new JsonResult(null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Programs(int organizationId, string term)
        {
            if (!String.IsNullOrWhiteSpace(term))
            {
                var progs = await _db.Programs.Where(o => o.Organization_Id == organizationId && o.Program_Name.ToLower().Contains(term.ToLower())).OrderBy(o => o.Program_Name).Select(o => new { id = o.Id, text = o.Program_Name }).ToListAsync();
                return new JsonResult(new { results = progs });
            }
            else
            {
                return new JsonResult(null);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string userId, string role, int? organizationId, int? programId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id = {userId} cannot be found";
                return View("NotFound");
            }
            else
            {
                var succeed = true;
                var roles = await _userManager.GetRolesAsync(user);

                if (!roles.Any(o => o == role))
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
                    if (!result.Succeeded)
                    {
                        succeed = false;
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }

                if (succeed)
                {
                    // Add authority, if appropriate
                    if (organizationId.HasValue)
                    {
                        //TODO: Convert to Mapping
                        var authority = new AspNetUserAuthorityModel { ApplicationUserId = user.Id, OrganizationId = organizationId.Value, ProgramId = programId, CreatedDate = DateTime.Now, CreatedBy = User.Identity.Name };
                        _db.AspNetUserAuthorities.Add(authority);
                        await _db.SaveChangesAsync();
                    }

                    ViewBag.Message = $"You have successfully added the {role} role to this user.";
                }
            }

            return await EditUser(userId);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id = {userId} cannot be found";
                return View("NotFound");
            }
            else
            {
                var result = await _userManager.RemoveFromRoleAsync(user, role);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    // Deal with organization role removal
                    if (role == "Organization")
                    {
                        var authorities = await _db.AspNetUserAuthorities.Where(o => o.ApplicationUserId == user.Id).ToListAsync();
                        _db.AspNetUserAuthorities.RemoveRange(authorities);
                        await _db.SaveChangesAsync();
                    }

                    // Deal with organization role removal
                    if (role == "Program")
                    {
                        var authorities = await _db.AspNetUserAuthorities.Where(o => o.ApplicationUserId == user.Id && o.ProgramId.HasValue).ToListAsync();
                        _db.AspNetUserAuthorities.RemoveRange(authorities);
                        await _db.SaveChangesAsync();
                    }

                    ViewBag.Message = $"You have successfully removed the {role} role from this user.";
                }
            }

            return await EditUser(userId);
        }



        [HttpGet]
        public async Task<IActionResult> RemoveAuthority(int id)
        {
            var authority = await _db.AspNetUserAuthorities.Where(o => o.Id == id).FirstOrDefaultAsync();

            if (authority != null)
            {
                _db.AspNetUserAuthorities.Remove(authority);
                await _db.SaveChangesAsync();

                ViewBag.Message = $"You have successfully removed the {(authority.ProgramId.HasValue ? "program" : "organization")} from this user.";
                return await EditUser(authority.ApplicationUserId);
            }
            else
            {
                ViewBag.ErrorMessage = $"Authority with id = {id} cannot be found";
                return View("NotFound");
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id = { id } cannot be found";
                return View("NotFound");
            }
            else
            {
                string deletedUser = user.UserName;

                var roles = await _userManager.GetRolesAsync(user);
                var result = await _userManager.RemoveFromRolesAsync(user, roles);

                if (result.Succeeded)
                {
                    var authorities = await _db.AspNetUserAuthorities.Where(o => o.ApplicationUserId == user.Id).ToListAsync();
                    if (authorities != null)
                    {
                        _db.AspNetUserAuthorities.RemoveRange(authorities);
                        await _db.SaveChangesAsync();
                    }

                    var result2 = await _userManager.DeleteAsync(user);

                    if (result2.Succeeded)
                    {
                        return RedirectToAction("ListUsers");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View("ListUsers");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListUsers()
        {
            var users = _userManager.Users;

            var usersAndRoles = new List<ListUserModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                string roleList = string.Join(", ", roles);

                ListUserModel model = new ListUserModel
                {
                    User = user,
                    RoleNames = roleList
                };

                usersAndRoles.Add(model);
            }

            return View(usersAndRoles);
        }
    }
}
