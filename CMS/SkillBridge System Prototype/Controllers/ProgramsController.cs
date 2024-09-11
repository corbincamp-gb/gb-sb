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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;
using Microsoft.Extensions.Configuration;
using SkillBridge_System_Prototype.ViewModel;
using Skillbridge.Business.Data;
using Skillbridge.Business.Model.Db;
using Skillbridge.Business.Model.Db.TrainingPlans;
using Skillbridge.Business.Query;
using Skillbridge.Business.Repository.Repositories;
using Taku.Core.Global;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize(Roles = "Admin, Analyst, Service")]
    public class ProgramsController : BaseProgramTrainingPlanController
    {
        internal readonly ILogger<ProgramsController> _logger;
        internal static IConfiguration _configuration;
        internal readonly UserManager<ApplicationUser> _userManager;
        internal readonly IEmailSender _emailSender;
        internal readonly IHttpContextAccessor _httpContextAccessor;

        public ProgramsController(ILogger<ProgramsController> logger, ApplicationDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor) : base(db)
        {
            _logger = logger;
            _userManager = userManager;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ListPrograms", "Programs");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /* All Programs */

        [HttpGet]
        public async Task<IActionResult> CreateProgram()
        {
            List<OrganizationModel> orgs = new List<OrganizationModel>();

            // Populate org and program lists
            foreach (OrganizationModel org in _db.Organizations)
            {
                orgs.Add(org);
            };

            // Order lists
            orgs = orgs.OrderBy(o => o.Name).ToList();

            ViewBag.Orgs = orgs;

            // Participation Population Dropdown
            List<ParticipationPopulation> pops = new List<ParticipationPopulation>();

            // Populate org and program lists
            foreach (ParticipationPopulation pop in _db.ParticipationPopulations)
            {
                pops.Add(pop);
            };

            pops = pops.OrderBy(o => o.Name).ToList();

            ViewBag.Participation_Population_List = pops;

            // Job Family dropdown
            List<JobFamily> jfs = new List<JobFamily>();

            foreach (JobFamily jf in _db.JobFamilies)
            {
                jfs.Add(jf);
            };

            jfs = jfs.OrderBy(o => o.Name).ToList();

            ViewBag.Job_Family_List = jfs;

            // Services Supported dropdown
            List<Service> ss = new List<Service>();
            Console.WriteLine("should be looking at services");

            foreach (Service s in _db.Services)
            {
                Console.WriteLine("s: " + s);
                ss.Add(s);
            };

            //ss = ss.OrderBy(o => o.Name).ToList();        // This was screwing up the IDs of the returned values for the services that ended up not matching the DB values

            ViewBag.Services_Supported_List = ss;

            // Delivery Method dropdown
            List<DeliveryMethod> dm = new List<DeliveryMethod>();
            Console.WriteLine("should be looking at delivery method");

            foreach (DeliveryMethod m in _db.DeliveryMethods)
            {
                Console.WriteLine("m: " + m);
                dm.Add(m);
            };

            dm = dm.OrderBy(o => o.Name).ToList();

            ViewBag.Delivery_Method_List = dm;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram(CreateProgramViewModel model)
        {
            string userName = HttpContext.User.Identity.Name;

            var org = _db.Organizations.FirstOrDefault(e => e.Id == model.Organization_Id);
            var mou = _db.Mous.FirstOrDefault(e => e.Id == org.Mou_Id);

            /*List<OrganizationModel> orgs = new List<OrganizationModel>();

            // Populate org and program lists
            foreach (OrganizationModel o in _db.Organizations)
            {
                orgs.Add(o);
            };

            // Order lists
            orgs = orgs.OrderBy(o => o.Name).ToList();

            ViewBag.Orgs = orgs;*/



            /*
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

            */

            /*foreach(int id in model.Services_Supported_List)
            {
                ProgramService ps = new ProgramService
                {
                    Program_Id = tempProg.Id,
                    Service_Id = newService
                };

                _db.ProgramService.Add(ps);
            }*/

            //Console.WriteLine("model.Services_Supported: " + model.Services_Supported);

            if (ModelState.IsValid)
            {
                try
                {
                    PendingProgramAdditionModel prog = new PendingProgramAdditionModel
                    {
                        Program_Name = GlobalFunctions.RemoveSpecialCharacters(model.Program_Name),
                        Program_Status = model.Program_Status,
                        Organization_Id = model.Organization_Id,
                        Organization_Name = org.Name,
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
                        Delivery_Method = GlobalFunctions.RemoveSpecialCharacters(model.Delivery_Method.ToString()),
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
                        For_Spouses = model.For_Spouses
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

                    prog.Requires_OSD_Review = true;

                    _db.PendingProgramAdditions.Add(prog);
                    var result = await _db.SaveChangesAsync();

                    if (result >= 1)
                    {
                        // Participation Populations
                        if (model.Populations_List != null)
                        {
                            Console.WriteLine("Posted program has participation populations of: " + model.Populations_List);
                            Console.WriteLine("Posted program has participation populations count of: " + model.Populations_List.Count);

                            foreach (int p in model.Populations_List)
                            {
                                Console.WriteLine("adding population to program: " + p);

                                PendingProgramAdditionParticipationPopulation pp = new PendingProgramAdditionParticipationPopulation
                                {
                                    Pending_Program_Id = prog.Id,
                                    Participation_Population_Id = p
                                };

                                _db.PendingProgramAdditionsParticipationPopulation.Add(pp);
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

                                PendingProgramAdditionJobFamily jf = new PendingProgramAdditionJobFamily
                                {
                                    Pending_Program_Id = prog.Id,
                                    Job_Family_Id = j
                                };

                                _db.PendingProgramAdditionsJobFamily.Add(jf);
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

                                PendingProgramAdditionService ps = new PendingProgramAdditionService
                                {
                                    Pending_Program_Id = prog.Id,
                                    Service_Id = s
                                };

                                _db.PendingProgramAdditionsService.Add(ps);
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

                                PendingProgramAdditionDeliveryMethod dm = new PendingProgramAdditionDeliveryMethod
                                {
                                    Pending_Program_Id = prog.Id,
                                    Delivery_Method_Id = m
                                };

                                _db.PendingProgramAdditionsDeliveryMethod.Add(dm);
                            }
                        }

                        //Update optimized fields

                        var result1 = await _db.SaveChangesAsync();

                        if (result1 >= 1)
                        {
                            prog.Services_Supported = GetServiceListForAdditionalProg(prog);
                            prog.Job_Family = GetJobFamiliesListForAdditionalProg(prog);
                            prog.Participation_Populations = GetParticipationPopulationStringFromAdditionalProgram(prog);

                            var result2 = await _db.SaveChangesAsync();

                            if (result2 >= 1)
                            {
                                return RedirectToAction("TrainingPlanChanges", new { id = prog.Id, isAddition = true });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " - " + ex.StackTrace);
                }
            }



            return View();
            //return RedirectToAction("ListPrograms", "Programs");
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

        private string GetServiceListForAdditionalProg(PendingProgramAdditionModel prog)
        {
            string services = "";

            var ps = _db.PendingProgramAdditionsService.Where(x => x.Pending_Program_Id == prog.Id).ToList();

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

        private string GetJobFamiliesListForAdditionalProg(PendingProgramAdditionModel prog)
        {
            string jfs = "";

            var pjf = _db.PendingProgramAdditionsJobFamily.Where(x => x.Pending_Program_Id == prog.Id).ToList();

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

        private string GetParticipationPopulationStringFromAdditionalProgram(PendingProgramAdditionModel prog)
        {
            string pps = "";

            var ppps = _db.PendingProgramAdditionsParticipationPopulation.Where(x => x.Pending_Program_Id == prog.Id).ToList();


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

        [HttpGet]
        public IActionResult ListPrograms()
        {
            var progs = _db.Programs;
            return View(progs);
        }

        [HttpGet]
        public IActionResult ListProgramsServerSide()
        {
            List<ListProgramModel> model = new List<ListProgramModel>();
            return View(model);
        }

        // Pull the data from the existing Program record
        [HttpGet]
        public async Task<IActionResult> EditProgram(string id, bool edit)
        {
            // Find any pending changes for this Program
            PendingProgramChangeModel pendingChange = await _db.PendingProgramChanges.FirstOrDefaultAsync(e => e.Program_Id == int.Parse(id) && e.Pending_Change_Status == 0);

            // Check for pending change, if it exists, redirect analyst user to the pending change instead
            if (pendingChange != null)
            {
                //Redirect to the approval page instead so analyst can enter changes and approve same time
                return Redirect(Url.Action("ReviewPendingProgramChange", "Analyst") + "/" + pendingChange.Id + "?progId=" + pendingChange.Program_Id);
            }

            // Populate Dropdown info for Participation Population Dropdown
            List<ParticipationPopulation> pops = new List<ParticipationPopulation>();

            foreach (ParticipationPopulation pop in _db.ParticipationPopulations)
            {
                pops.Add(pop);
            };

            pops = pops.OrderBy(o => o.Name).ToList();

            ViewBag.Participation_Population_List = pops;

            // Populate selected Participation Populations
            List<ProgramParticipationPopulation> popsList = await _db.ProgramParticipationPopulation.Where(e => e.Program_Id == int.Parse(id)).ToListAsync();

            List<int> selectedPops = new List<int>();

            foreach (ProgramParticipationPopulation p in popsList)
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

            List<ProgramJobFamily> jfsList = await _db.ProgramJobFamily.Where(e => e.Program_Id == int.Parse(id)).ToListAsync();

            List<int> selectedJfs = new List<int>();

            foreach (ProgramJobFamily j in jfsList)
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

            List<ProgramService> ssList = await _db.ProgramService.Where(e => e.Program_Id == int.Parse(id)).ToListAsync();

            List<int> selectedSs = new List<int>();

            foreach (ProgramService s in ssList)
            {
                selectedSs.Add(s.Service_Id);
                Console.WriteLine("Adding service to selected ss w id: " + s.Service_Id);
            }

            // Populate Dropdown info for Delivery Method Dropdown
            List<DeliveryMethod> dms = new List<DeliveryMethod>();

            foreach (DeliveryMethod dm in await _db.DeliveryMethods.ToListAsync())
            {
                dms.Add(dm);
            };

            dms = dms.OrderBy(o => o.Name).ToList();

            ViewBag.Delivery_Method_List = dms;

            // Populate selected Delivery Method

            List<ProgramDeliveryMethod> dmsList = await _db.ProgramDeliveryMethod.Where(e => e.Program_Id == int.Parse(id)).ToListAsync();

            List<int> selectedDms = new List<int>();

            foreach (ProgramDeliveryMethod m in dmsList)
            {
                selectedDms.Add(m.Delivery_Method_Id);
                Console.WriteLine("Adding delivery method to selected dms w id: " + m.Delivery_Method_Id);
            }

            //ViewBag.Selected_Participation_Populations = selectedPops;
            //Populations_List = selectedPops;

            // Find the existing Program in the current database
            ProgramModel prog = await _db.Programs.FirstOrDefaultAsync(e => e.Id == int.Parse(id));

            OrganizationModel org = await _db.Organizations.FirstOrDefaultAsync(e => e.Id == prog.Organization_Id);

            if (org.Is_Active == false)
            {
                ViewBag.Should_Disable_Editing = 1;
            }
            else
            {
                ViewBag.Should_Disable_Editing = 0;
            }

            // Find existing opportunities
            ViewBag.Opportunities = await _db.Opportunities.Where(o => o.Program_Id == prog.Id).OrderBy(o => o.State).ThenBy(o => o.City).ToListAsync();

            if (prog == null)
            {
                ViewBag.ErrorMessage = $"Program with id = {id} cannot be found";
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
                Pending_Fields = new List<string>(),
                Populations_List = selectedPops,
                Job_Family_List = selectedJfs,
                Services_Supported_List = selectedSs,
                Delivery_Method_List = selectedDms
            };

            // If we have a pending change we want a way to notify the user
            if (pendingChange != null)
            {
                ViewBag.hasPendingChange = true;

                // Update the model with data from the pending change
                model = UpdateProgModelWithPendingChanges(model, pendingChange);
            }
            else
            {
                ViewBag.hasPendingChange = false;
            }

            // Gather training plan information for display
            var repository = new TrainingPlanRepository(_db);
            var programTrainingPlans = await repository.GetProgramTrainingPlansByProgramIdAsync(prog.Id);

            ViewBag.ProgramTrainingPlans = programTrainingPlans;
            ViewBag.InstructionalMethodsList = await _db.InstructionalMethods.OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.TrainingPlanLengthsList = await _db.TrainingPlanLengths.OrderBy(o => o.SortOrder).ToListAsync();

            return View("~/Views/Programs/EditProgram.cshtml", model);
        }

        public EditProgramModel UpdateProgModelWithPendingChanges(EditProgramModel model, PendingProgramChangeModel pendingChange)
        {
            if (model.Is_Active != pendingChange.Is_Active) { model.Is_Active = pendingChange.Is_Active; model.Pending_Fields.Add("Is_Active"); }

            if (model.Program_Name != pendingChange.Program_Name)
            {
                if ((String.IsNullOrEmpty(model.Program_Name) == true && String.IsNullOrEmpty(pendingChange.Program_Name) == true) == false)
                { model.Program_Name = pendingChange.Program_Name; model.Pending_Fields.Add("Program_Name"); }
            }

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

            // Populate selected Services Selected
            List<PendingProgramService> ssList = _db.PendingProgramService.Where(e => e.Program_Id == int.Parse(model.Id) && e.Pending_Program_Id == pendingChange.Id).ToList();

            List<int> selectedSs = new List<int>();

            foreach (PendingProgramService s in ssList)
            {
                selectedSs.Add(s.Service_Id);
                //Console.WriteLine("Adding pending service to selected s w id: " + s.Service_Id);
            }

            List<ProgramService> oldSsList = _db.ProgramService.Where(e => e.Program_Id == int.Parse(model.Id)).ToList();

            List<int> oldSs = new List<int>();

            foreach (ProgramService p in oldSsList)
            {
                oldSs.Add(p.Service_Id);
                //Console.WriteLine("Adding pending service to selected pops w id: " + p.Service_Id);
            }

            //Console.WriteLine("==============Checking ss count in pending change, its " + selectedSs.Count);

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
            /*if (model.Job_Family != pendingChange.Job_Family)
            {
                if ((String.IsNullOrEmpty(model.Job_Family) == true && String.IsNullOrEmpty(pendingChange.Job_Family) == true) == false)
                { model.Job_Family = pendingChange.Job_Family; model.Pending_Fields.Add("Job_Family"); }
            }*/
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

        [Authorize(Roles = "Admin, Analyst")]
        // Post the change to the pending organizations change database table, ready for an analyst to review and approve
        [HttpPost]
        public async Task<IActionResult> EditProgram(EditProgramModel model)
        {
            //PendingOrganizationModel org = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Id == int.Parse(model.Id));

            //Console.WriteLine("bypassApproval: " + bypassApproval);

            //Console.WriteLine("model.Qp_Intake_Submission_Id: " + model.Qp_Intake_Submission_Id);
            if (CanPostEdit(int.Parse(model.Id)) == false)
            {
                ViewBag.ErrorMessage = $"Program with id = {model.Id} cannot be edited";
                return View("NotFound");
            }

            // Get Original Program
            ProgramModel origProg = _db.Programs.FirstOrDefault(e => e.Id == int.Parse(model.Id));

            // Find any pending changes for this organization
            PendingProgramChangeModel pendingChange = _db.PendingProgramChanges.FirstOrDefault(e => e.Program_Id == int.Parse(model.Id) && e.Pending_Change_Status == 0);

            string userName = HttpContext.User.Identity.Name;

            bool newForSpouses = false;

            //string userName = HttpContext.User.Identity.Name;

            bool updateEnabledFields = false;
            bool updateOptimizationFields = false;
            bool updateOnlineNationwideFields = false;

            bool sendOSDEmailNotification = false;

            if (model == null)
            {
                ViewBag.ErrorMessage = $"Program with id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                int numStatesFound = new NumberOfStatesInProgramQuery().Get(origProg, _db);

                PendingProgramChangeModel prog = new PendingProgramChangeModel { };

                // If theres already a unresolved pending change, update it
                if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                {
                    pendingChange.Is_Active = model.Is_Active;
                    pendingChange.Program_Name = GlobalFunctions.RemoveSpecialCharacters(model.Program_Name);
                    pendingChange.Organization_Name = GlobalFunctions.RemoveSpecialCharacters(model.Organization_Name);
                    pendingChange.Program_Id = int.Parse(model.Id);
                    pendingChange.Organization_Id = model.Organization_Id;
                    pendingChange.Lhn_Intake_Ticket_Id = GlobalFunctions.RemoveSpecialCharacters(model.Lhn_Intake_Ticket_Id);   // LHN Intake Ticket Number
                    pendingChange.Has_Intake = model.Has_Intake;         // Do we have a completed QuestionPro intake form from them
                    pendingChange.Intake_Form_Version = GlobalFunctions.RemoveSpecialCharacters(model.Intake_Form_Version);   // Which version of the QuestionPro intake form did they fill out
                    pendingChange.Qp_Intake_Submission_Id = GlobalFunctions.RemoveSpecialCharacters(model.Qp_Intake_Submission_Id);  // The ID of the QuestionPro intake form submission
                    pendingChange.Location_Details_Available = model.Location_Details_Available;  // From col O of master spreadsheet
                    pendingChange.Has_Consent = model.Has_Consent;
                    pendingChange.Qp_Location_Submission_Id = GlobalFunctions.RemoveSpecialCharacters(model.Qp_Location_Submission_Id);
                    pendingChange.Lhn_Location_Ticket_Id = GlobalFunctions.RemoveSpecialCharacters(model.Lhn_Location_Ticket_Id);
                    pendingChange.Has_Multiple_Locations = numStatesFound > 1 ? true : false;//model.Has_Multiple_Locations;
                    pendingChange.Reporting_Form_2020 = model.Reporting_Form_2020;
                    pendingChange.Date_Authorized = model.Date_Authorized;   // Date the 
                    pendingChange.Mou_Link = GlobalFunctions.RemoveSpecialCharacters(model.Mou_Link);       // URL link to actual MOU packet
                    pendingChange.Mou_Creation_Date = model.Mou_Creation_Date;
                    pendingChange.Mou_Expiration_Date = model.Mou_Expiration_Date;
                    pendingChange.Nationwide = model.Online == true || numStatesFound >= GlobalFunctions.MIN_STATES_FOR_NATIONWIDE ? true : false; // Nationwide should be calculated on if it's an online program or if sum of child opportunities are offered 3 or more collective states
                    pendingChange.Online = model.Online;
                    pendingChange.Participation_Populations = GlobalFunctions.RemoveSpecialCharacters(model.Participation_Populations);   // Might want enum for this
                    //pendingChange.Delivery_Method = model.Delivery_Method; //GlobalFunctions.RemoveSpecialCharacters(model.Delivery_Method);
                    pendingChange.States_Of_Program_Delivery = GlobalFunctions.RemoveSpecialCharacters(model.States_Of_Program_Delivery);
                    pendingChange.Program_Duration = model.Program_Duration;
                    pendingChange.Support_Cohorts = model.Support_Cohorts;
                    pendingChange.Opportunity_Type = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Type);
                    pendingChange.Job_Family = GlobalFunctions.RemoveSpecialCharacters(model.Job_Family);
                    pendingChange.Services_Supported = GlobalFunctions.RemoveSpecialCharacters(model.Services_Supported);
                    pendingChange.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(model.Enrollment_Dates);
                    pendingChange.Date_Created = model.Date_Created;
                    pendingChange.Date_Updated = DateTime.Now;  // Date program was last edited/updated in the system
                    pendingChange.Created_By = GlobalFunctions.RemoveSpecialCharacters(model.Created_By);
                    pendingChange.Updated_By = userName;
                    pendingChange.Program_Url = GlobalFunctions.RemoveSpecialCharacters(model.Program_Url);
                    pendingChange.Program_Status = model.Program_Status;// 0 is disabled, 1 is enabled
                    pendingChange.Admin_Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_First_Name);
                    pendingChange.Admin_Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Last_Name);
                    pendingChange.Admin_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Email);
                    pendingChange.Admin_Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Phone);
                    pendingChange.Public_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(model.Public_Poc_Name);
                    pendingChange.Public_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Public_Poc_Email);
                    pendingChange.Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes);
                    pendingChange.For_Spouses = model.For_Spouses;
                    pendingChange.Legacy_Program_Id = model.Legacy_Program_Id;
                    pendingChange.Legacy_Provider_Id = model.Legacy_Provider_Id;
                    //pendingChange.Pending_Change_Status = 1;
                    //pendingChange.Delivery_Method = GetDeliveryMethodId(model.Delivery_Method);
                    //model.Delivery_Method = pendingChange.Delivery_Method.ToString() == "-1" ? null : pendingChange.Delivery_Method.ToString();
                    pendingChange.Pending_Change_Status = 0;
                    pendingChange.Requires_OSD_Review = CheckForOSDApprovalNecessary(model, origProg);

                    sendOSDEmailNotification = pendingChange.Requires_OSD_Review;

                    // Status is still pending, so no need to update
                    _db.PendingProgramChanges.Update(pendingChange);
                }
                else  // If not, create a new one
                {
                    // Create the pending change object to push to the database table

                    prog.Is_Active = model.Is_Active;
                    prog.Program_Name = GlobalFunctions.RemoveSpecialCharacters(model.Program_Name);
                    prog.Program_Id = int.Parse(model.Id);
                    prog.Organization_Name = GlobalFunctions.RemoveSpecialCharacters(model.Organization_Name);
                    prog.Organization_Id = model.Organization_Id;
                    prog.Lhn_Intake_Ticket_Id = GlobalFunctions.RemoveSpecialCharacters(model.Lhn_Intake_Ticket_Id);   // LHN Intake Ticket Number
                    prog.Has_Intake = model.Has_Intake;         // Do we have a completed QuestionPro intake form from them
                    prog.Intake_Form_Version = GlobalFunctions.RemoveSpecialCharacters(model.Intake_Form_Version);   // Which version of the QuestionPro intake form did they fill out
                    prog.Qp_Intake_Submission_Id = GlobalFunctions.RemoveSpecialCharacters(model.Qp_Intake_Submission_Id);  // The ID of the QuestionPro intake form submission
                    prog.Location_Details_Available = model.Location_Details_Available;  // From col O of master spreadsheet
                    prog.Has_Consent = model.Has_Consent;
                    prog.Qp_Location_Submission_Id = GlobalFunctions.RemoveSpecialCharacters(model.Qp_Location_Submission_Id);
                    prog.Lhn_Location_Ticket_Id = GlobalFunctions.RemoveSpecialCharacters(model.Lhn_Location_Ticket_Id);
                    prog.Has_Multiple_Locations = numStatesFound > 1 ? true : false;
                    prog.Reporting_Form_2020 = model.Reporting_Form_2020;
                    prog.Date_Authorized = model.Date_Authorized;   // Date the 
                    prog.Mou_Link = model.Mou_Link;       // URL link to actual MOU packet
                    prog.Mou_Creation_Date = model.Mou_Creation_Date;
                    prog.Mou_Expiration_Date = model.Mou_Expiration_Date;
                    prog.Nationwide = model.Online == true || numStatesFound >= GlobalFunctions.MIN_STATES_FOR_NATIONWIDE ? true : false; // Nationwide should be calculated on if it's an online program or if sum of child opportunities are offered 3 or more collective states
                    prog.Online = model.Online;
                    prog.Participation_Populations = GlobalFunctions.RemoveSpecialCharacters(model.Participation_Populations);   // Might want enum for this
                    //prog.Delivery_Method = model.Delivery_Method;
                    prog.States_Of_Program_Delivery = GlobalFunctions.RemoveSpecialCharacters(model.States_Of_Program_Delivery);
                    prog.Program_Duration = model.Program_Duration;
                    prog.Support_Cohorts = model.Support_Cohorts;
                    prog.Opportunity_Type = GlobalFunctions.RemoveSpecialCharacters(model.Opportunity_Type);
                    prog.Job_Family = GlobalFunctions.RemoveSpecialCharacters(model.Job_Family);
                    prog.Services_Supported = GlobalFunctions.RemoveSpecialCharacters(model.Services_Supported);
                    prog.Enrollment_Dates = GlobalFunctions.RemoveSpecialCharacters(model.Enrollment_Dates);
                    prog.Date_Created = DateTime.Now;
                    prog.Date_Updated = DateTime.Now;  // Date program was last edited/updated in the system
                    prog.Created_By = model.Created_By;
                    prog.Updated_By = userName;
                    prog.Program_Url = GlobalFunctions.RemoveSpecialCharacters(model.Program_Url);
                    prog.Program_Status = model.Program_Status;// 0 is disabled; 1 is enabled
                    prog.Admin_Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_First_Name);
                    prog.Admin_Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Last_Name);
                    prog.Admin_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Email);
                    prog.Admin_Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(model.Admin_Poc_Phone);
                    prog.Public_Poc_Name = GlobalFunctions.RemoveSpecialCharacters(model.Public_Poc_Name);
                    prog.Public_Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Public_Poc_Email);
                    prog.Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes);
                    prog.For_Spouses = model.For_Spouses;
                    prog.Legacy_Program_Id = model.Legacy_Program_Id;
                    prog.Legacy_Provider_Id = model.Legacy_Provider_Id;
                    //prog.Delivery_Method = GetDeliveryMethodId(model.Delivery_Method);
                    //Console.WriteLine("Delivery Method List: " + model.Delivery_Method_List);
                    //model.Delivery_Method = prog.Delivery_Method.ToString() == "-1" ? null : prog.Delivery_Method.ToString();
                    //prog.Pending_Change_Status = 1;   // 0 = Pending   , 1 = Approved
                    prog.Pending_Change_Status = 0;
                    prog.Requires_OSD_Review = (model != null && origProg != null) ? CheckForOSDApprovalNecessary(model, origProg) : false;

                    sendOSDEmailNotification = prog.Requires_OSD_Review;

                    _db.PendingProgramChanges.Add(prog);
                }

                if (model.Populations_List != null)
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

                // Set optimization fields to update on child program/opportunity records if we have a name change
                if (!String.Equals(origProg.Program_Name, model.Program_Name, StringComparison.Ordinal))
                {
                    updateOptimizationFields = true;
                }

                // If Online or Nationwide has changed, we will need to update opps
                if (origProg.Online != model.Online || origProg.Nationwide != model.Nationwide)
                {
                    updateOnlineNationwideFields = true;
                }

                var result = await _db.SaveChangesAsync();

                //if (result >= 1)    // RESULT IS ACTUALLY THE NUMBER OF RECORDS UPDATED -- MAY NEED TO CHANGE THIS EVERYWHERE
                //{
                // If theres already a unresolved pending change, update it
                if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                {
                    // Remove existing pending changes for this dropdown before adding any
                    List<PendingProgramParticipationPopulation> popsList = _db.PendingProgramParticipationPopulation.Where(e => e.Pending_Program_Id == pendingChange.Id).ToList();

                    // If we have existing population items for this pending change, remove them first
                    if (popsList.Count > 0)
                    {
                        _db.PendingProgramParticipationPopulation.RemoveRange(popsList);

                        var result2 = await _db.SaveChangesAsync();
                        Console.WriteLine("-------------Removing existing pps from pending change");
                    }
                }

                if (model.Populations_List != null)
                {
                    Console.WriteLine("Posted program has participation populations of: " + model.Populations_List);
                    Console.WriteLine("Posted program has participation populations of: " + model.Populations_List.Count);

                    // If theres already a unresolved pending change, update it
                    if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                    {
                        // Check each population type and add if it doesnt exist for this pending change
                        foreach (int p in model.Populations_List)
                        {
                            //PendingProgramParticipationPopulation existingPop = _db.PendingProgramParticipationPopulation.FirstOrDefault(e => e.Program_Id == pendingChange.Id && e.Participation_Population_Id == p);

                            // If this specific record doesnt already exist in pending pops, create it
                            //if(existingPop == null)
                            //{
                            //Console.WriteLine("adding population to pending program change: " + p);

                            PendingProgramParticipationPopulation pp = new PendingProgramParticipationPopulation
                            {
                                Program_Id = pendingChange.Program_Id,
                                Pending_Program_Id = pendingChange.Id,
                                Participation_Population_Id = p
                            };

                            _db.PendingProgramParticipationPopulation.Add(pp);
                            //}
                        }
                    }
                    else
                    {
                        if (model.Populations_List != null)
                        {
                            foreach (int p in model.Populations_List)
                            {
                                //Console.WriteLine("adding population to pending program change: " + p);

                                PendingProgramParticipationPopulation pp = new PendingProgramParticipationPopulation
                                {
                                    Program_Id = prog.Program_Id,
                                    Pending_Program_Id = prog.Id,
                                    Participation_Population_Id = p
                                };

                                _db.PendingProgramParticipationPopulation.Add(pp);
                            }
                        }
                    }
                }

                // If theres already a unresolved pending change, update it
                if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                {
                    // Remove existing pending changes for this dropdown before adding any
                    List<PendingProgramJobFamily> jfsList = _db.PendingProgramJobFamily.Where(e => e.Pending_Program_Id == pendingChange.Id).ToList();

                    // If we have existing population items for this pending change, remove them first
                    if (jfsList.Count > 0)
                    {
                        _db.PendingProgramJobFamily.RemoveRange(jfsList);

                        var result2 = await _db.SaveChangesAsync();
                        Console.WriteLine("-------------Removing existing jfs from pending change");
                    }
                }

                if (model.Job_Family_List != null)
                {
                    Console.WriteLine("Posted program has jf of: " + model.Job_Family_List);
                    Console.WriteLine("Posted program has jf of: " + model.Job_Family_List.Count);

                    // If theres already a unresolved pending change, update it
                    if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                    {
                        // Check each population type and add if it doesnt exist for this pending change
                        foreach (int j in model.Job_Family_List)
                        {
                            //PendingProgramParticipationPopulation existingPop = _db.PendingProgramParticipationPopulation.FirstOrDefault(e => e.Program_Id == pendingChange.Id && e.Participation_Population_Id == p);

                            // If this specific record doesnt already exist in pending pops, create it
                            //if(existingPop == null)
                            //{
                            //Console.WriteLine("adding jf to pending program change: " + j);

                            PendingProgramJobFamily jf = new PendingProgramJobFamily
                            {
                                Program_Id = pendingChange.Program_Id,
                                Pending_Program_Id = pendingChange.Id,
                                Job_Family_Id = j
                            };

                            _db.PendingProgramJobFamily.Add(jf);
                            //}
                        }
                    }
                    else
                    {
                        foreach (int j in model.Job_Family_List)
                        {
                            //Console.WriteLine("adding jf to pending program change: " + j);

                            PendingProgramJobFamily jf = new PendingProgramJobFamily
                            {
                                Program_Id = prog.Program_Id,
                                Pending_Program_Id = prog.Id,
                                Job_Family_Id = j
                            };

                            _db.PendingProgramJobFamily.Add(jf);
                        }
                    }
                }

                // If theres already a unresolved pending change, update it
                if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                {
                    // Remove existing pending changes for this dropdown before adding any
                    List<PendingProgramService> ssList = _db.PendingProgramService.Where(e => e.Pending_Program_Id == pendingChange.Id).ToList();

                    // If we have existing population items for this pending change, remove them first
                    if (ssList.Count > 0)
                    {
                        _db.PendingProgramService.RemoveRange(ssList);

                        var result2 = await _db.SaveChangesAsync();
                        //Console.WriteLine("-------------Removing existing ss from pending change");
                    }
                }

                if (model.Services_Supported_List != null)
                {
                    //Console.WriteLine("Posted program has ss of: " + model.Services_Supported_List);
                    //Console.WriteLine("Posted program has ss of: " + model.Services_Supported_List.Count);

                    // If theres already a unresolved pending change, update it
                    if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                    {
                        // Check each population type and add if it doesnt exist for this pending change
                        foreach (int s in model.Services_Supported_List)
                        {
                            //PendingProgramParticipationPopulation existingPop = _db.PendingProgramParticipationPopulation.FirstOrDefault(e => e.Program_Id == pendingChange.Id && e.Participation_Population_Id == p);

                            // If this specific record doesnt already exist in pending pops, create it
                            //if(existingPop == null)
                            //{
                            Console.WriteLine("adding ss to pending program change: " + s);

                            PendingProgramService ss = new PendingProgramService
                            {
                                Program_Id = pendingChange.Program_Id,
                                Pending_Program_Id = pendingChange.Id,
                                Service_Id = s
                            };

                            _db.PendingProgramService.Add(ss);
                            //}
                        }
                    }
                    else
                    {
                        foreach (int j in model.Services_Supported_List)
                        {
                            Console.WriteLine("adding ss to pending program change: " + j);

                            PendingProgramService s = new PendingProgramService
                            {
                                Program_Id = prog.Program_Id,
                                Pending_Program_Id = prog.Id,
                                Service_Id = j
                            };

                            _db.PendingProgramService.Add(s);
                        }
                    }
                }

                // If theres already a unresolved pending change, update it
                if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                {
                    // Remove existing pending changes for this dropdown before adding any
                    List<PendingProgramDeliveryMethod> dmsList = _db.PendingProgramDeliveryMethod.Where(e => e.Pending_Program_Id == pendingChange.Id).ToList();

                    // If we have existing population items for this pending change, remove them first
                    if (dmsList.Count > 0)
                    {
                        _db.PendingProgramDeliveryMethod.RemoveRange(dmsList);

                        var result2 = await _db.SaveChangesAsync();
                        Console.WriteLine("-------------Removing existing dms from pending change");
                    }
                }

                if (model.Delivery_Method_List != null)
                {
                    Console.WriteLine("Posted program has delivery methods of: " + model.Delivery_Method_List);
                    Console.WriteLine("Posted program has delivery methods of: " + model.Delivery_Method_List.Count);

                    // If theres already a unresolved pending change, update it
                    if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                    {
                        // Check each population type and add if it doesnt exist for this pending change
                        foreach (int m in model.Delivery_Method_List)
                        {
                            //PendingProgramParticipationPopulation existingPop = _db.PendingProgramParticipationPopulation.FirstOrDefault(e => e.Program_Id == pendingChange.Id && e.Participation_Population_Id == p);

                            // If this specific record doesnt already exist in pending pops, create it
                            //if(existingPop == null)
                            //{
                            Console.WriteLine("adding delivery method to pending program change: " + m);

                            PendingProgramDeliveryMethod dm = new PendingProgramDeliveryMethod
                            {
                                Program_Id = pendingChange.Program_Id,
                                Pending_Program_Id = pendingChange.Id,
                                Delivery_Method_Id = m
                            };

                            _db.PendingProgramDeliveryMethod.Add(dm);
                            //}
                        }
                    }
                    else
                    {
                        if (model.Delivery_Method_List != null)
                        {
                            foreach (int m in model.Delivery_Method_List)
                            {
                                Console.WriteLine("adding delivery method to pending program change: " + m);

                                PendingProgramDeliveryMethod dm = new PendingProgramDeliveryMethod
                                {
                                    Program_Id = prog.Program_Id,
                                    Pending_Program_Id = prog.Id,
                                    Delivery_Method_Id = m
                                };

                                _db.PendingProgramDeliveryMethod.Add(dm);
                            }
                        }
                    }
                }

                var result1 = await _db.SaveChangesAsync();

                Console.WriteLine("-------------Should be saving pending dropdown changes, result was " + result);

                if (result1 >= 1)
                {
                    int pendingId = pendingChange != null ? pendingChange.Id : prog.Id;

                    if (sendOSDEmailNotification)
                    {
                        string currentHost = $"{Request.Host}";
                        string notificationEmail = _configuration["OsdNotificationEmail"];
                        string currentURI = $"{Request.Scheme}://{Request.Host}";
                        // Send confirmation Email
                        await _emailSender.SendEmailAsync(notificationEmail, "Program Change Requires OSD Approval", "A recent program change requires OSD approval.<br/><a href='" + currentURI + "/Analyst/ReviewPendingProgramChange/" + pendingId + "?progId=" + origProg.Id + "'>Click here to review it</a>");
                    }

                    return RedirectToAction("TrainingPlanChanges", new { id = pendingId, isAddition = false });
                }

                //return RedirectToAction("ListPrograms", "Programs");

                // Auto approve record instead
                //#region Auto Approve

                /*
                // Update the program with the data from the reviewed change
                if (origProg != null)
                {
                    origProg.Is_Active = model.Is_Active;
                    origProg.Program_Name = PreventNullString(model.Program_Name);
                    origProg.Organization_Name = PreventNullString(model.Organization_Name);
                    origProg.Organization_Id = model.Organization_Id;
                    origProg.Lhn_Intake_Ticket_Id = PreventNullString(model.Lhn_Intake_Ticket_Id);
                    origProg.Has_Intake = model.Has_Intake;
                    origProg.Intake_Form_Version = PreventNullString(model.Intake_Form_Version);
                    origProg.Qp_Intake_Submission_Id = PreventNullString(model.Qp_Intake_Submission_Id);
                    origProg.Location_Details_Available = model.Location_Details_Available;
                    origProg.Has_Consent = model.Has_Consent;
                    origProg.Qp_Location_Submission_Id = PreventNullString(model.Qp_Location_Submission_Id);
                    origProg.Lhn_Location_Ticket_Id = PreventNullString(model.Lhn_Location_Ticket_Id);
                    origProg.Has_Multiple_Locations = numStatesFound > 1 ? true : false;
                    origProg.Reporting_Form_2020 = model.Reporting_Form_2020;
                    origProg.Date_Authorized = model.Date_Authorized;
                    origProg.Mou_Link = PreventNullString(model.Mou_Link);
                    origProg.Mou_Creation_Date = model.Mou_Creation_Date;
                    origProg.Mou_Expiration_Date = model.Mou_Expiration_Date;
                    origProg.Nationwide = model.Online == true || numStatesFound >= GlobalFunctions.MIN_STATES_FOR_NATIONWIDE ? true : false; // Nationwide should be calculated on if it's an online program or if sum of child opportunities are offered 3 or more collective states
                    origProg.Online = model.Online;
                    origProg.Participation_Populations = PreventNullString(model.Participation_Populations);
                    //origProg.Delivery_Method = model.Delivery_Method;//PreventNullString(model.Delivery_Method);
                    origProg.States_Of_Program_Delivery = PreventNullString(model.States_Of_Program_Delivery);
                    origProg.Program_Duration = model.Program_Duration;
                    origProg.Support_Cohorts = model.Support_Cohorts;
                    origProg.Opportunity_Type = PreventNullString(model.Opportunity_Type);
                    origProg.Job_Family = PreventNullString(model.Job_Family);
                    origProg.Services_Supported = PreventNullString(model.Services_Supported);
                    origProg.Enrollment_Dates = PreventNullString(model.Enrollment_Dates);
                    origProg.Date_Created = model.Date_Created;
                    origProg.Date_Updated = DateTime.Now;
                    origProg.Created_By = PreventNullString(model.Created_By);
                    origProg.Updated_By = PreventNullString(model.Updated_By);
                    origProg.Program_Url = PreventNullString(model.Program_Url);
                    origProg.Program_Status = model.Program_Status;
                    origProg.Admin_Poc_First_Name = PreventNullString(model.Admin_Poc_First_Name);
                    origProg.Admin_Poc_Last_Name = PreventNullString(model.Admin_Poc_Last_Name);
                    origProg.Admin_Poc_Email = PreventNullString(model.Admin_Poc_Email);
                    origProg.Admin_Poc_Phone = PreventNullString(model.Admin_Poc_Phone);
                    origProg.Public_Poc_Name = PreventNullString(model.Public_Poc_Name);
                    origProg.Public_Poc_Email = PreventNullString(model.Public_Poc_Email);
                    origProg.Notes = PreventNullString(model.Notes);
                    //prog.For_Spouses = model.For_Spouses;
                    Console.WriteLine("program for spouses being set to: " + newForSpouses);
                    origProg.For_Spouses = newForSpouses;
                    origProg.Legacy_Program_Id = model.Legacy_Program_Id;
                    origProg.Legacy_Provider_Id = model.Legacy_Provider_Id;

                    //prog.Populations_List = selectedPops;
                }

                Console.WriteLine("origProg.Organization_Id: " + origProg.Organization_Id);
                _db.Programs.Update(origProg);

                var resultA = await _db.SaveChangesAsync();
                Console.WriteLine("resultA: " + resultA);
                if (resultA > 0)
                {
                    // Remove existing pending changes for this dropdown before adding any
                    List<ProgramParticipationPopulation> popsList = _db.ProgramParticipationPopulation.Where(e => e.Program_Id == origProg.Id).ToList();

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
                                Program_Id = origProg.Id,
                                Participation_Population_Id = p
                            };

                            _db.ProgramParticipationPopulation.Add(pp);
                        }
                    }

                    // Remove existing pending changes for this dropdown before adding any
                    List<ProgramJobFamily> jfsList = _db.ProgramJobFamily.Where(e => e.Program_Id == origProg.Id).ToList();

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
                                Program_Id = origProg.Id,
                                Job_Family_Id = j
                            };

                            _db.ProgramJobFamily.Add(jf);
                        }
                    }

                    // Remove existing pending changes for this dropdown before adding any
                    List<ProgramService> ssList = _db.ProgramService.Where(e => e.Program_Id == origProg.Id).ToList();

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
                                Program_Id = origProg.Id,
                                Service_Id = s
                            };

                            _db.ProgramService.Add(ss);
                        }
                    }

                    // Remove existing pending changes for this dropdown before adding any
                    List<ProgramDeliveryMethod> dmList = _db.ProgramDeliveryMethod.Where(e => e.Program_Id == origProg.Id).ToList();

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
                                Program_Id = origProg.Id,
                                Delivery_Method_Id = m
                            };

                            _db.ProgramDeliveryMethod.Add(dm);
                        }
                    }

                    // Update pending change and the updated dropdown values
                    //pendingChange.Pending_Change_Status = 1;
                    //_db.PendingProgramChanges.Update(pendingChange);

                    var result6 = await _db.SaveChangesAsync();

                    if (result6 > 0)
                    {
                        if (updateEnabledFields)
                        {
                            var oppsToUpdate = _db.Opportunities.Where(p => p.Program_Id == origProg.Id);

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
                            Console.WriteLine("updateOptimizationFields for program true, should be updating opps program name val");
                            var oppsToUpdate = _db.Opportunities.Where(p => p.Program_Id == origProg.Id);

                            if (oppsToUpdate.ToList<OpportunityModel>().Count > 0)
                            {
                                foreach (OpportunityModel o in oppsToUpdate)
                                {
                                    o.Program_Name = PreventNullString(model.Program_Name);
                                    _db.Opportunities.Update(o);
                                }
                            }
                        }

                        if(updateOnlineNationwideFields)
                        {
                            var oppsToUpdate = _db.Opportunities.Where(p => p.Program_Id == origProg.Id);

                            if (oppsToUpdate.ToList<OpportunityModel>().Count > 0)
                            {
                                foreach (OpportunityModel o in oppsToUpdate)
                                {
                                    o.Nationwide = origProg.Nationwide;
                                    o.Online = origProg.Online;
                                    _db.Opportunities.Update(o);
                                }
                            }
                        }

                        var result7 = await _db.SaveChangesAsync();

                        if (result7 > 0)
                        {
                            List<OpportunityModel> relatedOpps = _db.Opportunities.FromCache().Where(e => e.Program_Id == origProg.Id).ToList();
                            GlobalFunctions.UpdateStatesOfProgramDelivery(origProg, relatedOpps, _db);
                            GlobalFunctions.UpdateJobFamilyListForOpps(origProg, relatedOpps, _db);
                            return RedirectToAction("ListPrograms");
                        }
                    }
                }
                else
                {

                }
                #endregion
                */
            }

            /*foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }*/



            return RedirectToAction("ListPrograms");
            //}
            //else
            //{
            //    return RedirectToAction("ListPrograms");
            //}
        }

        public async Task<IActionResult> TrainingPlanChanges(int id, bool isAddition)
        {
            return await base.BaseTrainingPlanChanges(id, isAddition, "~/views/programs/trainingplanchanges.cshtml");
        }

        public async Task<IActionResult> ViewTrainingPlan(int id)
        {
            return await base.BaseViewTrainingPlan(id, "~/views/programs/viewtrainingplan.cshtml");
        }

        public async Task<IActionResult> TrainingPlanForm(int id, bool isAddition)
        {
            return await base.BaseTrainingPlanForm(id, isAddition, "~/views/programs/trainingplanform.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ModifyTrainingPlan(int pendingId, bool isAddition, int trainingPlanId)
        {
            return await base.BaseModifyTrainingPlan(pendingId, isAddition, trainingPlanId, "~/views/programs/trainingplanform.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> TrainingPlanForm(int pendingId, bool isAddition, TrainingPlan model)
        {
            return await base.BaseTrainingPlanForm(pendingId, isAddition, model, "~/views/programs/trainingplanform.cshtml", "/programs/listprograms");
        }

        public async Task<IActionResult> DeactivateTrainingPlan(int id, int programId)
        {
            return await base.BaseDeactivateTrainingPlan(id, programId, $"/Programs/EditProgram/{programId}?edit=false");
        }

        public async Task<IActionResult> ActivateTrainingPlan(int id, int programId)
        {
            return await base.BaseActivateTrainingPlan(id, programId, $"/Programs/EditProgram/{programId}?edit=false");
        }


        private int GetDeliveryMethodId(string dm)
        {
            int newDeliveryMethod = -1;

            if (dm == "In-person")
            {
                newDeliveryMethod = 0;
            }
            else if (dm == "Online")
            {
                newDeliveryMethod = 1;
            }
            else if (dm == "Hybrid (in-person and online)" || dm == "Hybrid" || dm == "Hybrid (in-person and online) and Online")
            {
                newDeliveryMethod = 2;
            }

            return newDeliveryMethod;
        }

        private List<int> CheckForDeliveryMethodChange(List<int> dmsList, ProgramModel origProg)
        {
            bool changed = false;

            List<ProgramDeliveryMethod> oldDmsList = _db.ProgramDeliveryMethod.Where(e => e.Program_Id == origProg.Id).ToList();

            List<int> selectedDms = new List<int>();

            if (dmsList != null)
            {
                foreach (int p in dmsList)
                {
                    selectedDms.Add(p);
                    //Console.WriteLine("Adding pending participation population to selected pops w id: " + p.Participation_Population_Id);
                }
            }


            List<int> oldDms = new List<int>();

            foreach (ProgramDeliveryMethod m in oldDmsList)
            {
                oldDms.Add(m.Delivery_Method_Id);
                //Console.WriteLine("Adding pending participation population to selected pops w id: " + p.Participation_Population_Id);
            }

            //Console.WriteLine("==============Checking part pop count in pending change, its " + selectedPops.Count);

            selectedDms.Sort();
            oldDms.Sort();

            foreach (int i in selectedDms)
            {
                Console.WriteLine("selectedDms item: " + i);
            }

            foreach (int i in oldDms)
            {
                Console.WriteLine("oldDms item: " + i);
            }

            List<int> dups4 = selectedDms.Intersect(oldDms).ToList();
            List<int> distinct4 = selectedDms.Except(oldDms).ToList();

            //Console.WriteLine("distinct.count in analystController for PP: " + distinct.Count);

            foreach (int i in dups4)
            {
                Console.WriteLine("duplicate item: " + i);
            }

            foreach (int j in distinct4)
            {
                Console.WriteLine("distinct item: " + j);
            }

            if (distinct4.Count > 0 || selectedDms.Count != oldDms.Count) // If there is a difference between the old and new list of ints
            {
                return selectedDms;
            }

            return selectedDms;
        }

        private bool CheckForOSDApprovalNecessary(EditProgramModel model, ProgramModel origProg)
        {
            bool required = false;

            var newDeliveryMethods = CheckForDeliveryMethodChange(model.Delivery_Method_List, origProg);
            bool DeliveryMethodChanged = (model.Delivery_Method_List != null ? String.Join(",", model.Delivery_Method_List.OrderBy(o => o).ToList()) : String.Empty) != String.Join(",", newDeliveryMethods.OrderBy(o => o).ToList());

            model.Delivery_Method_List = newDeliveryMethods;

            if (newDeliveryMethods.Count > 0)
            {
                model.Pending_Fields.Add("Delivery_Method_List");
            }

            // Check if any of the fields that require OSD approval are changed
            if (model.Program_Name != origProg.Program_Name ||
                DeliveryMethodChanged == true ||
               //model.Delivery_Method != origProg.Delivery_Method || // null vs "0"
               model.Program_Duration != origProg.Program_Duration ||
               model.Opportunity_Type != origProg.Opportunity_Type ||
               model.Online != origProg.Online)
            {
                required = true;
            }

            return required;
        }

        private bool CheckForOSDApprovalNecessary(ProgramModel model, ProgramModel origProg)
        {
            bool required = false;

            var oldDeliveryMethods = model.ProgramDeliveryMethods.Select(o => o.Delivery_Method_Id).ToList();
            var newDeliveryMethods = CheckForDeliveryMethodChange(oldDeliveryMethods, origProg);
            bool DeliveryMethodChanged = oldDeliveryMethods != newDeliveryMethods;

            model.ProgramDeliveryMethods = newDeliveryMethods.Select(o => new ProgramDeliveryMethod { Delivery_Method_Id = o }).ToList();

            // Check if any of the fields that require OSD approval are changed
            if (model.Program_Name != origProg.Program_Name ||
                DeliveryMethodChanged == true ||
               //model.Delivery_Method != origProg.Delivery_Method || // null vs "0"
               model.Program_Duration != origProg.Program_Duration ||
               model.Opportunity_Type != origProg.Opportunity_Type ||
               model.Online != origProg.Online)
            {
                required = true;
            }

            return required;
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

        public string PreventNullString(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                s = "";
            }
            return s;
        }

        public bool CanPostEdit(int progId)
        {
            ProgramModel prog = _db.Programs.FirstOrDefault(e => e.Id == progId);

            OrganizationModel org = _db.Organizations.FirstOrDefault(e => e.Id == prog.Organization_Id);

            if (org.Is_Active)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProgram(string id)
        {
            var org = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Id == int.Parse(id));

            if (org == null)
            {
                ViewBag.ErrorMessage = $"Organization with id = {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                _db.Remove(org);
                var result = await _db.SaveChangesAsync();

                if (result == 1)
                {
                    return RedirectToAction("ListOrganizations");
                }
                else
                {

                }

                /*foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }*/

                return View("ListOrganizations");
            }
        }

        [HttpGet]
        public IActionResult DownloadCSV()
        {
            var progs = _db.Programs;

            try
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("Id,Program_Name,Organization_Id,Lhn_Intake_Ticket_Id,Has_Intake,Intake_Form_Version,Qp_Intake_Submission_Id,Location_Details_Available,Has_Consent,Qp_Location_Submission_Id,Lhn_Location_Ticket_Id,Has_Multiple_Locations,Reporting_Form_2020,Date_Authorized,Mou_Link,Mou_Creation_Date,Mou_Expiration_Date,Nationwide,Online,Participation_Populations,Delivery_Method,States_Of_Program_Delivery,Program_Duration,Support_Cohorts,Opportunity_Type,Job_Family,Services_Supported,Enrollment_Dates,Date_Created,Date_Updated,Created_By,Updated_By,Program_Url,Program_Status,Admin_Poc_First_Name,Admin_Poc_Last_Name,Admin_Poc_Email,Admin_Poc_Phone,Public_Poc_Name,Public_Poc_Email,Notes,For_Spouses,Legacy_Program_Id,Legacy_Provider_Id");

                foreach (ProgramModel prog in progs)
                {
                    string newProgramName = EscCommas(prog.Program_Name.Replace(System.Environment.NewLine, ""));
                    string Lhn_Intake_Ticket_Id = EscCommas(prog.Lhn_Intake_Ticket_Id.Replace(System.Environment.NewLine, ""));
                    string Intake_Form_Version = EscCommas(prog.Intake_Form_Version.Replace(System.Environment.NewLine, ""));
                    string Qp_Intake_Submission_Id = EscCommas(prog.Qp_Intake_Submission_Id.Replace(System.Environment.NewLine, ""));
                    string Qp_Location_Submission_Id = EscCommas(prog.Qp_Location_Submission_Id.Replace(System.Environment.NewLine, ""));
                    string Lhn_Location_Ticket_Id = EscCommas(prog.Lhn_Location_Ticket_Id.Replace(System.Environment.NewLine, ""));
                    string Participation_Populations = EscCommas(prog.Participation_Populations.Replace(System.Environment.NewLine, ""));
                    //string Delivery_Method = EscCommas(prog.Delivery_Method.Replace(System.Environment.NewLine, ""));

                    string States_Of_Program_Delivery = EscCommas(prog.States_Of_Program_Delivery.Replace(System.Environment.NewLine, ""));

                    string Program_Duration = EscCommas(prog.Program_Duration.ToString().Replace(System.Environment.NewLine, ""));

                    string Opportunity_Type = EscCommas(prog.Opportunity_Type.Replace(System.Environment.NewLine, ""));

                    string Job_Family = EscCommas(prog.Job_Family.Replace(System.Environment.NewLine, ""));

                    string Services_Supported = EscCommas(prog.Services_Supported.Replace(System.Environment.NewLine, ""));

                    string Enrollment_Dates = EscCommas(prog.Enrollment_Dates.Replace(System.Environment.NewLine, ""));

                    string Program_Url = EscCommas(prog.Program_Url.Replace(System.Environment.NewLine, ""));

                    string Public_Poc_Name = EscCommas(prog.Public_Poc_Name.Replace(System.Environment.NewLine, ""));

                    string Public_Poc_Email = EscCommas(prog.Public_Poc_Email.Replace(System.Environment.NewLine, ""));

                    string Notes = prog.Notes == null ? "" : EscCommas(prog.Notes.Replace(System.Environment.NewLine, ""));

                    string Mou_Link = EscCommas(prog.Mou_Link.Replace(System.Environment.NewLine, ""));
                    //string Employer_Poc_Name = org.Employer_Poc_Name.Replace(System.Environment.NewLine, ""); //add a line terminating ;
                    //string summaryDescription = org.Summary_Description.Replace(System.Environment.NewLine, ""); //add a line /terminating ;
                    //string jobDescription = org.Jobs_Description.Replace(System.Environment.NewLine, ""); //add a line terminating ;

                    stringBuilder.AppendLine($"{prog.Id},{newProgramName},{prog.Organization_Id},{Lhn_Intake_Ticket_Id},{prog.Has_Intake},{Intake_Form_Version},{Qp_Intake_Submission_Id},{prog.Location_Details_Available},{prog.Has_Consent},{Qp_Location_Submission_Id},{Lhn_Location_Ticket_Id},{prog.Has_Multiple_Locations},{prog.Reporting_Form_2020},{prog.Date_Authorized},{Mou_Link},{prog.Mou_Creation_Date},{prog.Mou_Expiration_Date},{prog.Nationwide},{prog.Online},{Participation_Populations},{prog.Delivery_Method},{States_Of_Program_Delivery},{Program_Duration},{prog.Support_Cohorts},{Opportunity_Type},{Job_Family},{Services_Supported},{Enrollment_Dates},{prog.Date_Created},{prog.Date_Updated},{prog.Created_By},{prog.Updated_By},{Program_Url},{prog.Program_Status},{prog.Admin_Poc_First_Name},{prog.Admin_Poc_Last_Name},{prog.Admin_Poc_Email},{prog.Admin_Poc_Phone},{Public_Poc_Name},{Public_Poc_Email},{Notes},{prog.For_Spouses},{prog.Legacy_Program_Id},{prog.Legacy_Provider_Id}");
                }

                //return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", "Programs-FULL-" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
                return ExportPrograms();
            }
            catch
            {
                return Error();
            }
        }

        public FileStreamResult ExportPrograms()
        {
            var result = WriteCsvToMemory(_db.Programs);
            var memoryStream = new MemoryStream(result);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddSeconds(2);
            options.Path = "/Programs";
            _httpContextAccessor.HttpContext.Response.Cookies.Append("programsDownloadStarted", "1", options);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = "Programs-FULL-" + DateTime.Today.ToString("MM-dd-yy") + ".csv" };
        }


        public byte[] WriteCsvToMemory(IEnumerable<ProgramModel> records)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.Context.RegisterClassMap<ProgramMap>();
                //csvWriter.WriteRecords(records);
                csvWriter.WriteHeader<ProgramModel>();
                csvWriter.WriteField("Program_Duration");
                csvWriter.WriteField("Participation_Population");
                csvWriter.WriteField("Job_Family");
                csvWriter.WriteField("Supported_Service");
                csvWriter.NextRecord();
                foreach (ProgramModel p in records)
                {
                    csvWriter.WriteRecord(p);
                    csvWriter.WriteField($"{GetProgDuration(p.Program_Duration)}");
                    csvWriter.WriteField($"{GetPartPop(p.Id)}");
                    csvWriter.WriteField($"{GetJobFam(p.Id)}");
                    csvWriter.WriteField($"{GetService(p.Id)}");
                    csvWriter.NextRecord();
                }

                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        public string GetProgDuration(int i)
        {
            string newVal = "";
            if (i == 0)
            {
                newVal = "1 - 30 days";
            }
            else if (i == 1)
            {
                newVal = "31 - 60 days";
            }
            else if (i == 2)
            {
                newVal = "61 - 90 days";
            }
            else if (i == 3)
            {
                newVal = "91 - 120 days";
            }
            else if (i == 4)
            {
                newVal = "121 - 150 days";
            }
            else if (i == 5)
            {
                newVal = "151 - 180 days";
            }
            else if (i == 6)
            {
                newVal = "Individually Developed – not to exceed 40 hours";
            }
            else if (i == 7)
            {
                newVal = "Self - paced";
            }
            return newVal;
        }

        public string GetPartPop(int id)
        {
            string newVal = "";
            int i = 0;

            List<ProgramParticipationPopulation> popsList = _db.ProgramParticipationPopulation.Where(e => e.Program_Id == id).ToList();

            if (popsList != null)
            {
                if (popsList.Count > 0)
                {
                    foreach (ProgramParticipationPopulation p in popsList)
                    {
                        newVal += ConvertPartPopIdToName(p.Participation_Population_Id);

                        if (i < popsList.Count - 1)
                        {
                            newVal += ", ";
                        }
                        i++;
                    }
                }
            }

            return newVal;
        }

        public string GetJobFam(int id)
        {
            string newVal = "";
            int i = 0;

            List<ProgramJobFamily> jfList = _db.ProgramJobFamily.Where(e => e.Program_Id == id).ToList();

            if (jfList != null)
            {
                if (jfList.Count > 0)
                {
                    foreach (ProgramJobFamily jf in jfList)
                    {
                        newVal += ConvertJobFamIdToName(jf.Job_Family_Id);

                        if (i < jfList.Count - 1)
                        {
                            newVal += ", ";
                        }
                        i++;
                    }
                }
            }

            return newVal;
        }

        public string GetService(int id)
        {
            string newVal = "";
            int i = 0;

            List<ProgramService> ssList = _db.ProgramService.Where(e => e.Program_Id == id).ToList();

            if (ssList != null)
            {
                if (ssList.Count > 0)
                {
                    foreach (ProgramService ss in ssList)
                    {
                        newVal += ConvertServiceIdToName(ss.Service_Id);

                        if (i < ssList.Count - 1)
                        {
                            newVal += ", ";
                        }
                        i++;
                    }
                }
            }

            return newVal;
        }

        public string ConvertPartPopIdToName(int id)
        {
            string newName = "";
            if (id == 1)
            {
                newName = "Service Members";
            }
            else if (id == 2)
            {
                newName = "Veterans";
            }
            else if (id == 3)
            {
                newName = "Military Spouses";
            }
            return newName;
        }

        public string ConvertJobFamIdToName(int id)
        {
            string newName = "";
            if (id == 1)
            {
                newName = "Architecture and Engineering";
            }
            else if (id == 2)
            {
                newName = "Arts, Design, Entertainment, Sports, and Media";
            }
            else if (id == 3)
            {
                newName = "Building and Grounds Cleaning and Maintenance";
            }
            else if (id == 4)
            {
                newName = "Business and Financial Operations";
            }
            else if (id == 5)
            {
                newName = "Community and Social Service";
            }
            else if (id == 6)
            {
                newName = "Computer and Mathematical";
            }
            else if (id == 7)
            {
                newName = "Construction and Extraction";
            }
            else if (id == 8)
            {
                newName = "Education, Training, and Library";
            }
            else if (id == 9)
            {
                newName = "Farming, Fishing, and Forestry";
            }
            else if (id == 10)
            {
                newName = "Food Preparation and Serving Related";
            }
            else if (id == 11)
            {
                newName = "Healthcare Practitioners and Technical";
            }
            else if (id == 12)
            {
                newName = "Healthcare Support";
            }
            else if (id == 13)
            {
                newName = "Installation, Maintenance, and Repair";
            }
            else if (id == 14)
            {
                newName = "Legal";
            }
            else if (id == 15)
            {
                newName = "Life, Physical, and Social Science";
            }
            else if (id == 16)
            {
                newName = "Management";
            }
            else if (id == 17)
            {
                newName = "Military Specific";
            }
            else if (id == 18)
            {
                newName = "Protective Service";
            }
            else if (id == 19)
            {
                newName = "Sales and Related";
            }
            else if (id == 20)
            {
                newName = "Transportation and Material Moving";
            }
            else if (id == 21)
            {
                newName = "Other";
            }
            else if (id == 22)
            {
                newName = "Office and Administrative Support";
            }
            else if (id == 23)
            {
                newName = "Personal Care and Service";
            }
            else if (id == 24)
            {
                newName = "Production";
            }
            return newName;
        }

        public string ConvertServiceIdToName(int id)
        {
            string newName = "";
            if (id == 1)
            {
                newName = "Air Force";
            }
            else if (id == 2)
            {
                newName = "Army";
            }
            else if (id == 3)
            {
                newName = "Coast Guard";
            }
            else if (id == 4)
            {
                newName = "Marine Corps";
            }
            else if (id == 5)
            {
                newName = "Navy";
            }
            else if (id == 6)
            {
                newName = "Space Force";
            }
            return newName;
        }

        public string EscCommas(string data)
        {
            //string QUOTE = "\"";
            //string ESCAPED_QUOTE = "\"\"";
            char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };

            if (data != null)
            {
                if (data.Contains(","))
                {
                    data = String.Format("\"{0}\"", data);
                }

                /*if (data.Contains(QUOTE))
                    data = data.Replace(QUOTE, ESCAPED_QUOTE);

                if (data.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                    data = QUOTE + data + QUOTE;*/
            }

            return data;
        }
    }

    public sealed class ProgramMap : ClassMap<ProgramModel>
    {
        public ProgramMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.Legacy_Program_Id).Ignore();
            Map(m => m.Legacy_Provider_Id).Ignore();
            Map(m => m.Program_Duration).Ignore();
            Map(m => m.Participation_Populations).Ignore();
            Map(m => m.Job_Family).Ignore();
            //Map(m => m.Program_Duration).Convert(row => GetProgDuration(row.Row.GetField("Program_Duration")));
        }
    }
}

