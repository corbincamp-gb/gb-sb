using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.Data;
using SkillBridge_System_Prototype.Models;
using SkillBridge_System_Prototype.Util.Global;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace SkillBridge_System_Prototype.Controllers
{
    [Authorize(Roles = "Organization")]

    public class MyOrganizationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MyOrganizationController> _logger;
        private readonly IEmailSender _emailSender;

        public MyOrganizationController(ILogger<MyOrganizationController> logger, ApplicationDbContext db, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
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

        // Pull the data from the existing Organization record
        public async Task<IActionResult> List()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            bool IsAdmin = currentUser.IsInRole("Admin");
            var userid = _userManager.GetUserId(User); // Get user id:

            var _authenticationRepository = new AuthenticationRepository(_db);
            var organizations = await _authenticationRepository.GetOrganizationsByUser(userid);

            // Take the first organization in the list
            if (organizations != null && organizations.Count > 1)
            {
                return View("List", organizations);
            }
            else
            {
                return RedirectToAction("EditOrganization", new { Id = organizations.FirstOrDefault().Id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditOrganization(int id)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            bool IsAdmin = currentUser.IsInRole("Admin");
            var userid = _userManager.GetUserId(User); // Get user id:

            var _authenticationRepository = new AuthenticationRepository(_db);
            var organizations = await _authenticationRepository.GetOrganizationsByUser(userid);
            
            if (!organizations.Any(o => o.Id == id))
            {
                return RedirectToAction("List");
            }

            // Look for parent organization
            SB_Organization org = organizations.FirstOrDefault(o => o.Parent_Organization_Name == o.Name);
            if (org == null) org = organizations.FirstOrDefault(o => o.Id == id);
            SB_Mou mou = _db.Mous.FirstOrDefault(e => e.Id == org.Mou_Id);

            // Find any pending changes for this organization
            SB_PendingOrganizationChange pendingChange = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Organization_Id == org.Id && e.Pending_Change_Status == 0);

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

            ViewBag.MOU_Organization_Name = mou.Organization_Name;
            ViewBag.MOU_Url = mou.Url;
            ViewBag.MOU_Creation_Date = mou.Creation_Date;
            ViewBag.MOU_Expiration_Date = mou.Expiration_Date;
            ViewBag.MOU_Service = mou.Service;

            var _mouFileRepository = new MouFileRepository(_db);
            ViewBag.MouFile = await _mouFileRepository.GetMouFileByMouId(org.Mou_Id);

            return View(model);
        }

        public async Task<IActionResult> ViewMouFile(int Id)
        {
            var _mouFileRepository = new MouFileRepository(_db);
            var mouFile = await _mouFileRepository.GetMouFile(Id);
            return File(mouFile.FileBlob.Blob, mouFile.ContentType);
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

        // Post the change to the pending organizations change database table, ready for an analyst to review and approve
        [HttpPost]
        public async Task<IActionResult> EditOrganization(EditOrganizationModel model)
        {
            //SB_PendingOrganizationChange org = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Id == int.Parse(model.Id));

            // Find any pending changes for this organization
            SB_PendingOrganizationChange pendingChange = _db.PendingOrganizationChanges.FirstOrDefault(e => e.Organization_Id == int.Parse(model.Id) && e.Pending_Change_Status == 0);

            string userName = HttpContext.User.Identity.Name;

            if (model == null)
            {
                ViewBag.ErrorMessage = $"Organization with id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                // If theres already a unresolved pending change, update it
                if (pendingChange != null && pendingChange.Pending_Change_Status == 0)
                {
                    pendingChange.Is_Active = model.Is_Active;
                    pendingChange.Name = GlobalFunctions.RemoveSpecialCharacters(model.Name);
                    pendingChange.Poc_First_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_First_Name);
                    pendingChange.Poc_Last_Name = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Last_Name);
                    pendingChange.Poc_Email = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Email);
                    pendingChange.Poc_Phone = GlobalFunctions.RemoveSpecialCharacters(model.Poc_Phone);
                    pendingChange.Date_Created = model.Date_Created;
                    pendingChange.Created_By = GlobalFunctions.RemoveSpecialCharacters(model.Created_By);
                    pendingChange.Date_Updated = DateTime.Now;
                    pendingChange.Updated_By = userName;
                    pendingChange.Organization_Url = GlobalFunctions.RemoveSpecialCharacters(model.Organization_Url);
                    pendingChange.Organization_Type = model.Organization_Type;
                    pendingChange.Notes = GlobalFunctions.RemoveSpecialCharacters(model.Notes);
                    pendingChange.Legacy_Provider_Id = model.Legacy_Provider_Id;
                    // Status is still pending, so no need to update
                }
                else  // If not, create a new one
                {
                    // Create the pending change object to push to the database table
                    SB_PendingOrganizationChange org = new SB_PendingOrganizationChange
                    {
                        Is_Active = model.Is_Active,
                        // Id = this is auto-incremented as its added to the pending change table
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
                        Pending_Change_Status = 0   // 0 = Pending
                    };

                    _db.PendingOrganizationChanges.Add(org);
                }

                var result = await _db.SaveChangesAsync();

                if (result >= 1)    // RESULT IS ACTUALLY THE NUMBER OF RECORDS UPDATED -- MAY NEED TO CHANGE THIS EVERYWHERE
                {
                    //Console.WriteLine("SAVE RESULT WAS 1");
                    return RedirectToAction("MyOrganizationUpdateSuccess");
                }
                else
                {
                    //Console.WriteLine("SAVE RESULT WAS 0");
                }

                return RedirectToAction("MyOrganizationUpdateSuccess");
            }
        }

        // Update success/fail views
        public IActionResult MyOrganizationUpdateSuccess()
        {
            return View();
        }
    }
}
