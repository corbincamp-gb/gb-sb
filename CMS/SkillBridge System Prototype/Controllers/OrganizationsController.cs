using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.Data;
using SkillBridge_System_Prototype.Models;
using SkillBridge_System_Prototype.Util.Global;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize(Roles = "Admin, Analyst, Service")]
    public class OrganizationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrganizationsController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrganizationsController(ILogger<OrganizationsController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return RedirectToAction("ListOrganizations", "Organizations");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /* All Organizations */

        [HttpGet]
        public IActionResult CreateOrganization()
        {
            List<SB_Mou> mouList = _db.Mous.ToList();

            CreateOrganizationModelView model = new CreateOrganizationModelView
            {
                Mous = mouList
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganizationAsync(CreateOrganizationModelView model, IFormFile mouFile)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            bool IsAdmin = currentUser.IsInRole("Admin");
            var userid = _userManager.GetUserId(User); // Get user id:
            var user = await _userManager.FindByIdAsync(userid);

            string id = user.ProgramId.ToString();

            if (ModelState.IsValid)
            {
                try
                {
                    // If existing parent organization exists and this is tied under it, this should reference the existing MOU instead of creating a new one

                    if(model.Mou_Id > 0)
                    {
                        //SB_Mou mou = _db.Mous.FirstOrDefault(m => m.Id == model.Mou_Id);

                        SB_Organization org = new SB_Organization
                        {
                            Is_Active = model.Is_Active,
                            Mou_Id = model.Mou_Id,
                            Is_MOU_Parent = model.Is_MOU_Parent,
                            Name = GlobalFunctions.RemoveSpecialCharacters(model.Name),
                            Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_First_Name),
                            Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Last_Name),
                            Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Email),
                            Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Phone),
                            Date_Created = DateTime.Now, // Date org was created in system
                            Date_Updated = DateTime.Now, // Date org was last edited/updated in the system.
                            Created_By = _userManager.GetUserName(currentUser),
                            Updated_By = _userManager.GetUserName(currentUser),
                            Organization_Url = GlobalFunctions.RemoveSpecialCharacters(model.Organization_Url),
                            Organization_Type = model.Organization_Type,
                            Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes),
                            Legacy_Provider_Id = -1,
                            Legacy_MOU_Id = -1
                        };

                        _db.Organizations.Add(org);
                        var result2 = await _db.SaveChangesAsync();

                        if (result2 > 0)
                        {
                            return RedirectToAction("ListOrganizations", "Organizations");
                        }
                    }
                    else
                    {
                        SB_Mou mou = new SB_Mou
                        {
                            Creation_Date = model.Creation_Date,
                            Expiration_Date = model.Expiration_Date,
                            Url = GlobalFunctions.RemoveSpecialCharacters(model.Url),
                            Service = GlobalFunctions.RemoveSpecialCharacters(model.Service),
                            Is_OSD = model.Is_OSD,
                            Organization_Name = GlobalFunctions.RemoveSpecialCharacters(model.Name),
                            Legacy_Provider_Id = -1
                        };

                        // Save to DB
                        _db.Mous.Add(mou);

                        var result = await _db.SaveChangesAsync();

                        if (result > 0)
                        {
                            SB_Organization org = new SB_Organization
                            {
                                Is_Active = model.Is_Active,
                                Mou_Id = mou.Id,
                                Is_MOU_Parent = model.Is_MOU_Parent,
                                Name = GlobalFunctions.RemoveSpecialCharacters(model.Name),
                                Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_First_Name),
                                Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Last_Name),
                                Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Email),
                                Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Phone),
                                Date_Created = DateTime.Now, // Date org was created in system
                                Date_Updated = DateTime.Now, // Date org was last edited/updated in the system.
                                Created_By = _userManager.GetUserName(currentUser),
                                Updated_By = _userManager.GetUserName(currentUser),
                                Organization_Url = GlobalFunctions.RemoveSpecialCharacters(model.Organization_Url),
                                Organization_Type = model.Organization_Type,
                                Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes),
                                Legacy_Provider_Id = -1,
                                Legacy_MOU_Id = -1
                            };

                            _db.Organizations.Add(org);
                            var result2 = await _db.SaveChangesAsync();

                            if (result2 > 0)
                            {
                                return await UploadMou(mou.Id, org.Id.ToString(), mouFile);
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
        }

        [HttpGet]
        public IActionResult ListOrganizations()
        {
            var orgs = _db.Organizations;
            return View(orgs);
        }

        [HttpGet]
        public IActionResult ListOrganizationsServerSide()
        {
            List<ListOrganizationModel> model = new List<ListOrganizationModel>();
            return View(model);
        }

        // Pull the data from the existing Organization record
        [HttpGet]
        public async Task<IActionResult> EditOrganization(string id, bool edit)
        {
            // Find any pending changes for this opportunity
            SB_PendingOrganizationChange pendingChange = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Organization_Id == int.Parse(id) && e.Pending_Change_Status == 0);

            // Check for pending change, if it exists, redirect analyst user to the pending change instead
            if (pendingChange != null)
            {
                //Redirect to the approval page instead so analyst can enter changes and approve same time
                return Redirect(Url.Action("ReviewPendingOrganizationChange", "Analyst") + "/" + pendingChange.Id + "?orgId=" + pendingChange.Organization_Id);
            }

            // Find the existing Organization in the current database
            SB_Organization org = _db.Organizations.FirstOrDefault(e => e.Id == int.Parse(id));

            // Find any pending changes for this organization
            //SB_PendingOrganizationChange pendingChange = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Organization_Id == int.Parse(id) && e.Pending_Change_Status == 0);

            if (org == null)
            {
                ViewBag.ErrorMessage = $"Organization with id = {id} cannot be found";
                return View("NotFound");
            }

            SB_Mou mou = _db.Mous.FirstOrDefault(e => e.Id == org.Mou_Id);

            var model = new EditOrganizationModel
            {
                Id = org.Id.ToString(),
                Name = org.Name,
                Is_Active = org.Is_Active,
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
                States_Of_Program_Delivery = org.States_Of_Program_Delivery,
                Pending_Fields = new List<string>()
            };

            // If we have a pending change we want a way to notify the user
            if (pendingChange != null)
            {
                ViewBag.hasPendingChange = true;
                
                // Update the model with data from the pending change
                model = UpdateOrgModelWithPendingChanges(model, pendingChange);
            }
            else
            {
                ViewBag.hasPendingChange = false;
            }

            ViewBag.MOU_Id = mou.Id;
            ViewBag.MOU_Organization_Name = mou.Organization_Name;
            ViewBag.MOU_Url = mou.Url;
            ViewBag.MOU_Creation_Date = mou.Creation_Date;
            ViewBag.MOU_Expiration_Date = mou.Expiration_Date;
            ViewBag.MOU_Service = mou.Service;

            var _mouFileRepository = new MouFileRepository(_db);
            ViewBag.MouFile = await _mouFileRepository.GetMouFileByMouId(org.Mou_Id);

            return View("~/Views/Organizations/EditOrganization.cshtml", model);
        }

        public EditOrganizationModel UpdateOrgModelWithPendingChanges(EditOrganizationModel model, SB_PendingOrganizationChange pendingChange)
        {
            if (model.Is_Active != pendingChange.Is_Active) { model.Is_Active = pendingChange.Is_Active; model.Pending_Fields.Add("Is_Active"); }

            if (model.Name != pendingChange.Name)
            {
                if ((String.IsNullOrEmpty(model.Name) == true && String.IsNullOrEmpty(pendingChange.Name) == true) == false)
                { model.Name = pendingChange.Name; model.Pending_Fields.Add("Name"); }
            }
            if (model.Poc_First_Name != pendingChange.Poc_First_Name)
            {
                if ((String.IsNullOrEmpty(model.Poc_First_Name) == true && String.IsNullOrEmpty(pendingChange.Poc_First_Name) == true) == false)
                { model.Poc_First_Name = pendingChange.Poc_First_Name; model.Pending_Fields.Add("Poc_First_Name"); }
            }
            if (model.Poc_Last_Name != pendingChange.Poc_Last_Name)
            {
                if ((String.IsNullOrEmpty(model.Poc_Last_Name) == true && String.IsNullOrEmpty(pendingChange.Poc_Last_Name) == true) == false)
                { model.Poc_Last_Name = pendingChange.Poc_Last_Name; model.Pending_Fields.Add("Poc_Last_Name"); }
            }
            if (model.Poc_Email != pendingChange.Poc_Email)
            {
                if ((String.IsNullOrEmpty(model.Poc_Email) == true && String.IsNullOrEmpty(pendingChange.Poc_Email) == true) == false)
                { model.Poc_Email = pendingChange.Poc_Email; model.Pending_Fields.Add("Poc_Email"); }
            }
            if (model.Poc_Phone != pendingChange.Poc_Phone)
            {
                if ((String.IsNullOrEmpty(model.Poc_Phone) == true && String.IsNullOrEmpty(pendingChange.Poc_Phone) == true) == false)
                { model.Poc_Phone = pendingChange.Poc_Phone; model.Pending_Fields.Add("Poc_Phone"); }
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
            if (model.Organization_Url != pendingChange.Organization_Url) { model.Organization_Url = pendingChange.Organization_Url; model.Pending_Fields.Add("Organization_Url"); }
            if (model.Organization_Type != pendingChange.Organization_Type)
            {
                model.Organization_Type = pendingChange.Organization_Type; model.Pending_Fields.Add("Organization_Type");
            }
            if (model.Notes != pendingChange.Notes)
            {
                if ((String.IsNullOrEmpty(model.Notes) == true && String.IsNullOrEmpty(pendingChange.Notes) == true) == false)
                { model.Notes = pendingChange.Notes; model.Pending_Fields.Add("Notes"); }
            }
            if (model.Legacy_Provider_Id != pendingChange.Legacy_Provider_Id) { model.Legacy_Provider_Id = pendingChange.Legacy_Provider_Id; model.Pending_Fields.Add("Legacy_Provider_Id"); }

            return model;
        }

        [Authorize(Roles = "Admin, Analyst")]
        // Post the change to the pending organizations change database table, ready for an analyst to review and approve
        [HttpPost]
        public async Task<IActionResult> EditOrganization(EditOrganizationModel model)
        {
            //SB_PendingOrganizationChange org = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Id == int.Parse(model.Id));

            // Find any pending changes for this organization
            SB_PendingOrganizationChange pendingChange = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Organization_Id == int.Parse(model.Id) && e.Pending_Change_Status == 0);

            SB_Organization origOrg = _db.Organizations.FirstOrDefault(e => e.Id == int.Parse(model.Id));

            string userName = HttpContext.User.Identity.Name;

            if (model == null)
            {
                ViewBag.ErrorMessage = $"Organization with id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                // If theres already a unresolved pending change, update it
                if(pendingChange != null && pendingChange.Pending_Change_Status == 0)
                {
                    pendingChange.Is_Active = model.Is_Active;
                    pendingChange.Name = GlobalFunctions.RemoveSpecialCharacters(model.Name);
                    pendingChange.Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_First_Name);
                    pendingChange.Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Last_Name);
                    pendingChange.Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Email);
                    pendingChange.Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Phone);
                    pendingChange.Date_Created = model.Date_Created;
                    pendingChange.Date_Updated = model.Date_Updated;
                    pendingChange.Created_By = GlobalFunctions.RemoveSpecialCharacters(model.Created_By);
                    pendingChange.Updated_By = userName;
                    pendingChange.Organization_Url = GlobalFunctions.RemoveSpecialCharacters(model.Organization_Url);
                    pendingChange.Organization_Type = model.Organization_Type;
                    pendingChange.Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes);
                    pendingChange.Legacy_Provider_Id = model.Legacy_Provider_Id;
                    pendingChange.Pending_Change_Status = 1;
                    // Status is still pending, so no need to update
                }
                else  // If not, create a new one
                {
                    // Create the pending change object to push to the database tabler
                    SB_PendingOrganizationChange org = new SB_PendingOrganizationChange
                    {
                        // Id = this is auto-incremented as its added to the pending change table
                        Is_Active = model.Is_Active,
                        Organization_Id = int.Parse(model.Id), // Set original organization Id from the id of the original organization model
                        Name = GlobalFunctions.RemoveSpecialCharacters(model.Name),
                        Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_First_Name),
                        Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Last_Name),
                        Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Email),
                        Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Phone),
                        Date_Created = model.Date_Created,
                        Date_Updated = DateTime.Now,
                        Created_By = model.Created_By,
                        Updated_By = userName,
                        Organization_Url = GlobalFunctions.RemoveSpecialCharacters(model.Organization_Url),
                        Organization_Type = model.Organization_Type,
                        Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes),
                        Legacy_Provider_Id = model.Legacy_Provider_Id,
                        Pending_Change_Status = 1   // 0 = Pending
                    };

                    _db.PendingOrganizationChanges.Add(org);
                }
                
                var result = await _db.SaveChangesAsync();

                if (result >= 1)    // RESULT IS ACTUALLY THE NUMBER OF RECORDS UPDATED -- MAY NEED TO CHANGE THIS EVERYWHERE
                {
                    // Find this specific pending change
                    //SB_PendingOrganizationChange pendingChange = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Organization_Id == int.Parse(orgId) && e.Pending_Change_Status == 0);

                    //string userName = HttpContext.User.Identity.Name;

                    bool updateEnabledFields = false;
                    bool updateOptimizationFields = false;

                    // Set is_active fields to update on child records if we have is_active set to false
                    if (model.Is_Active == false)
                    {
                        //Console.WriteLine("The posted model was set to disabled");
                        updateEnabledFields = true;
                    }

                    // Set optimization fields to update on child program/oppotunity records if we have a name change
                    if (!String.Equals(origOrg.Name, model.Name, StringComparison.Ordinal))
                    {
                        updateOptimizationFields = true;
                    }

                    // Update the organization with the data from the reviewed change
                    if (origOrg != null)
                    {
                        origOrg.Is_Active = model.Is_Active;
                        origOrg.Name = PreventNullString(model.Name);
                        origOrg.Poc_First_Name = PreventNullString(model.Poc_First_Name);
                        origOrg.Poc_Last_Name = PreventNullString(model.Poc_Last_Name);
                        origOrg.Poc_Email = PreventNullString(model.Poc_Email);
                        origOrg.Poc_Phone = PreventNullString(model.Poc_Phone);
                        origOrg.Date_Updated = DateTime.Now;
                        origOrg.Updated_By = PreventNullString(userName);
                        origOrg.Organization_Url = PreventNullString(model.Organization_Url);
                        origOrg.Organization_Type = model.Organization_Type;
                        origOrg.Notes = PreventNullString(model.Notes);
                    }

                    _db.Organizations.Update(origOrg);

                    var result1 = await _db.SaveChangesAsync();

                    if (result1 > 0)
                    {
                        // Update pending change
                        //pendingChange.Pending_Change_Status = 1;
                        //_db.PendingOrganizationChanges.Update(pendingChange);
                        //_db.PendingOrganizationChanges.Remove(pendingChange);

                        //var result2 = await _db.SaveChangesAsync();

                        //if (result2 > 0)
                        //{
                            if (updateEnabledFields)
                            {
                                var progsToUpdate = _db.Programs.Where(p => p.Organization_Id == origOrg.Id);
                                var oppsToUpdate = _db.Opportunities.Where(p => p.Organization_Id == origOrg.Id);

                                if (progsToUpdate.ToList<SB_Program>().Count > 0)
                                {
                                    Console.WriteLine("There are programs to update on disable");
                                    foreach (SB_Program p in progsToUpdate)
                                    {
                                        p.Is_Active = false;
                                        p.Date_Deactivated = DateTime.Now;
                                        _db.Programs.Update(p);
                                    }
                                }

                                if (oppsToUpdate.ToList<SB_Opportunity>().Count > 0)
                                {
                                    Console.WriteLine("There are opportunities to update on disable");
                                    foreach (SB_Opportunity o in oppsToUpdate)
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
                                var progsToUpdate = _db.Programs.Where(p => p.Organization_Id == origOrg.Id);
                                var oppsToUpdate = _db.Opportunities.Where(p => p.Organization_Id == origOrg.Id);

                                if (progsToUpdate.ToList<SB_Program>().Count > 0 || oppsToUpdate.ToList<SB_Opportunity>().Count > 0)
                                {
                                    foreach (SB_Program p in progsToUpdate)
                                    {
                                        p.Organization_Name = origOrg.Name;
                                        _db.Programs.Update(p);
                                    }

                                    foreach (SB_Opportunity o in oppsToUpdate)
                                    {
                                        o.Organization_Name = origOrg.Name;
                                        _db.Opportunities.Update(o);
                                    }
                                }
                            }

                            var result3 = await _db.SaveChangesAsync();

                            if (result3 > 0)
                            {
                                return RedirectToAction("ListOrganizations");
                            }
                            else
                            {
                                return RedirectToAction("ListOrganizations");
                            }
                        //}
                    }
                    else
                    {

                    }

                    return RedirectToAction("ListOrganizations");
                }
                else
                {

                }

                /*foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }*/

                return RedirectToAction("ListOrganizations");
            }
        }

        public async Task<IActionResult> ViewMouFile(int Id)
        {
            var _mouFileRepository = new MouFileRepository(_db);
            var mouFile = await _mouFileRepository.GetMouFile(Id);
            return File(mouFile.FileBlob.Blob, mouFile.ContentType);
        }

        [HttpPost]
        public async Task<IActionResult> UploadMou(int MouId, string OrgId, IFormFile mouFile)
        {
            if (MouId == 0) return RedirectToAction("ListOrganizations");

            if (mouFile != null && mouFile.Length > 0)
            {
                if (mouFile.ContentType == "application/pdf")
                {
                    var _mouFileRepository = new MouFileRepository(_db);

                    using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
                    {
                        mouFile.CopyTo(mStream);

                        try
                        {
                            var record = await _mouFileRepository.SaveMouFile(new MouFile
                            {
                                MouId = MouId,
                                FileName = mouFile.FileName,
                                ContentLength = mouFile.Length,
                                ContentType = mouFile.ContentType,
                                FileBlob = new MouFileBlob { Blob = mStream.ToArray() },
                                IsActive = true,
                                CreateBy = HttpContext.User.Identity.Name,
                            });
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", $"Error trying to save file: {ex.Message}");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", $"The MOU must be a PDF of type 'application/pdf'. The file you uploaded is {mouFile.ContentType}");
                }
            }
            else
            {
                ModelState.AddModelError("", "MOU file is required");
            }

            return await EditOrganization(OrgId, true);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMou(int MouId, string OrgId, DateTime? Expiration_Date)
        {
            if (MouId == 0) return RedirectToAction("ListOrganizations");

            if (Expiration_Date.HasValue)
            {
                var mou = await _db.Mous.FirstOrDefaultAsync(o => o.Id == MouId);

                if (mou != null)
                {
                    mou.Expiration_Date = Expiration_Date.Value;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError("MouId", $"MOU {MouId} could not be found");
                }
            }
            else
            {
                ModelState.AddModelError("Expiration_Date", "MOU expiration date is required");
            }

            return await EditOrganization(OrgId, true);
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
        public async Task<IActionResult> DeleteOrganization(string id)
        {
            SB_PendingOrganizationChange org = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Id == int.Parse(id));

            if (org == null)
            {
                ViewBag.ErrorMessage = $"Organization with id = { id } cannot be found";
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
            var orgs = _db.Organizations;

            try
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("Id,Mou_Id,Is_MOU_Parent,Name,Poc_First_Name,Poc_Last_Name,Poc_Email,Poc_Phone,Date_Created,Date_Updated,Created_By,Updated_By,Organization_Url,Organization_Type, Notes,Legacy_Provider_Id,Legacy_MOU_Id");

                foreach (SB_Organization org in orgs)
                {
                    string newOrgName = EscCommas(org.Name.Replace(System.Environment.NewLine, ""));
                    string Poc_First_Name = EscCommas(org.Poc_First_Name.Replace(System.Environment.NewLine, ""));
                    string Poc_Last_Name = EscCommas(org.Poc_Last_Name.Replace(System.Environment.NewLine, ""));
                    string Poc_Email = EscCommas(org.Poc_Email.Replace(System.Environment.NewLine, ""));
                    string Poc_Phone = EscCommas(org.Poc_Phone.Replace(System.Environment.NewLine, ""));
                    string url = EscCommas(org.Poc_Phone.Replace(System.Environment.NewLine, ""));
                    string notes = org.Notes == null ? "" : EscCommas(org.Notes.Replace(System.Environment.NewLine, ""));
                    //string Employer_Poc_Name = org.Employer_Poc_Name.Replace(System.Environment.NewLine, ""); //add a line terminating;
                    //string summaryDescription = org.Summary_Description.Replace(System.Environment.NewLine, ""); //add a line /terminating;
                    //string jobDescription = org.Jobs_Description.Replace(System.Environment.NewLine, ""); //add a line terminating;

                    stringBuilder.AppendLine($"{org.Id},{org.Mou_Id},{org.Is_MOU_Parent},{newOrgName},{Poc_First_Name},{Poc_Last_Name},{Poc_Email},{Poc_Phone},{org.Date_Created},{org.Date_Updated},{org.Created_By},{org.Updated_By},{url},{org.Organization_Type},{notes},{org.Legacy_Provider_Id},{org.Legacy_MOU_Id}");
                }

                //return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", "Organizations-FULL-" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
                return ExportOrganizations();
            }
            catch
            {
                return Error();
            }
        }

        public FileStreamResult ExportOrganizations()
        {
            var result = WriteCsvToMemory(_db.Organizations);
            var memoryStream = new MemoryStream(result);
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddSeconds(2);
            options.Path = "/Organizations";
            _httpContextAccessor.HttpContext.Response.Cookies.Append("organizationsDownloadStarted", "1", options);
            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = "Organizations-FULL-" + DateTime.Today.ToString("MM-dd-yy") + ".csv" };
        }


        public byte[] WriteCsvToMemory(IEnumerable<SB_Organization> records)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.Context.RegisterClassMap<OrganizationMap>();
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

            if (data != null)
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

        /*
         [HttpGet]
        public IActionResult EditOrganization(string id)
        {
            SB_Organization org = _db.Organizations.FirstOrDefault(e => e.Id == int.Parse(id));

            if (org == null)
            {
                ViewBag.ErrorMessage = $"Organization with id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditOrganizationModel
            {
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
                Legacy_Provider_Id = org.Legacy_Provider_Id
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrganization(EditOrganizationModel model)
        {
            SB_Organization org = _db.Organizations.FirstOrDefault(e => e.Id == int.Parse(model.Id));

            string userName = HttpContext.User.Identity.Name;

            if (org == null)
            {
                ViewBag.ErrorMessage = $"Organization with id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                org.Name = model.Name;
                org.Poc_First_Name = model.Poc_First_Name;
                org.Poc_Last_Name = model.Poc_Last_Name;
                org.Poc_Email = model.Poc_Email;
                org.Poc_Phone = model.Poc_Phone;
                org.Date_Created = model.Date_Created;
                org.Date_Updated = DateTime.Now;
                org.Created_By = model.Created_By;
                org.Updated_By = userName;
                org.Organization_Url = model.Organization_Url;
                org.Organization_Type = model.Organization_Type;
                org.Notes = model.Notes;
                org.Legacy_Provider_Id = model.Legacy_Provider_Id;

                var result = await _db.SaveChangesAsync();

                if (result == 1)
                {
                    return RedirectToAction("ListOrganizations");
                }
                else
                {

                }

                return View(model);
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteOrganization(string id)
        {
            SB_Organization org = _db.Organizations.FirstOrDefault(e => e.Id == int.Parse(id));

            if (org == null)
            {
                ViewBag.ErrorMessage = $"Organization with id = { id } cannot be found";
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

                return View("ListOrganizations");
            }
        }
         */
    }
}

public sealed class OrganizationMap : ClassMap<SB_Organization>
{
    public OrganizationMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Legacy_Provider_Id).Ignore();
        Map(m => m.Legacy_MOU_Id).Ignore();
    }
}
