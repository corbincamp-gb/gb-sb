using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skillbridge.Business.Data;
using Skillbridge.Business.Model.Db;
using Skillbridge.Business.Model.Db.TrainingPlans;
using Skillbridge.Business.Repository.Repositories;

namespace SkillBridge_System_Prototype.Controllers
{
    public class BaseProgramTrainingPlanController : Controller
    {
        internal readonly ApplicationDbContext _db;

        public BaseProgramTrainingPlanController(ApplicationDbContext db)
        {
            _db = db;
        }

        protected async Task<IActionResult> BaseTrainingPlanChanges(int id, bool isAddition, string view)
        {
            PendingProgramModel pending = null;

            if (isAddition)
            {
                var pendingAddition = _db.PendingProgramAdditions.FirstOrDefault(e => e.Id == id && e.Pending_Change_Status == 0);

                if (pendingAddition != null)
                {
                    pending = new PendingProgramModel(pendingAddition);
                }
            }
            else
            {
                var pendingChange = _db.PendingProgramChanges.FirstOrDefault(e => e.Id == id && e.Pending_Change_Status == 0);

                if (pendingChange != null)
                {
                    pending = new PendingProgramModel(pendingChange);
                }
            }

            if (pending == null)
            {
                ViewBag.ErrorMessage = $"Program with id = {id} cannot be found";
                return View("NotFound");
            }

            var repository = new TrainingPlanRepository(_db);
            var trainingPlans = await repository.GetTrainingPlansByOrgIdAsync(pending.Organization_Id);

            ViewBag.TrainingPlans = trainingPlans;

            return View(view, pending);
        }

        protected async Task<IActionResult> BaseViewTrainingPlan(int id, string view)
        {
            var repository = new TrainingPlanRepository(_db);
            var trainingPlan = await repository.GetTrainingPlanAsync(id);

            if (trainingPlan == null)
            {
                ViewBag.ErrorMessage = $"Training plan with id = {id} cannot be found";
                return View("NotFound");
            }

            ViewBag.InstructionalMethodsList = await _db.InstructionalMethods.OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.TrainingPlanLengthsList = await _db.TrainingPlanLengths.OrderBy(o => o.SortOrder).ToListAsync();

            return View(view, trainingPlan);
        }

        protected async Task<IActionResult> BaseTrainingPlanForm(int id, bool isAddition, string view)
        {
            PendingProgramModel pending = null;

            if (isAddition)
            {
                var pendingAddition = _db.PendingProgramAdditions.FirstOrDefault(e => e.Id == id && e.Pending_Change_Status == 0);

                if (pendingAddition != null)
                {
                    pending = new PendingProgramModel(pendingAddition);
                }
            }
            else
            {
                var pendingChange = _db.PendingProgramChanges.FirstOrDefault(e => e.Id == id && e.Pending_Change_Status == 0);

                if (pendingChange != null)
                {
                    pending = new PendingProgramModel(pendingChange);
                }
            }

            if (pending == null)
            {
                ViewBag.ErrorMessage = $"Program with id = {id} cannot be found";
                return View("NotFound");
            }

            ViewBag.Pending = pending;
            ViewBag.InstructionalMethodsList = await _db.InstructionalMethods.OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.TrainingPlanLengthsList = await _db.TrainingPlanLengths.OrderBy(o => o.SortOrder).ToListAsync();

            return View(view, new TrainingPlan());
        }

        [HttpPost]
        protected async Task<IActionResult> BaseModifyTrainingPlan(int pendingId, bool isAddition, int trainingPlanId, string view)
        {
            if (trainingPlanId > 0 && pendingId > 0)
            {
                PendingProgramModel pending = null;

                if (isAddition)
                {
                    var pendingAddition = _db.PendingProgramAdditions.FirstOrDefault(e => e.Id == pendingId && e.Pending_Change_Status == 0);

                    if (pendingAddition != null)
                    {
                        pending = new PendingProgramModel(pendingAddition);
                    }
                }
                else
                {
                    var pendingChange = _db.PendingProgramChanges.FirstOrDefault(e => e.Id == pendingId && e.Pending_Change_Status == 0);

                    if (pendingChange != null)
                    {
                        pending = new PendingProgramModel(pendingChange);
                    }
                }

                if (pending == null)
                {
                    ViewBag.ErrorMessage = $"Program with id = {pendingId} cannot be found";
                    return View("NotFound");
                }

                var repository = new TrainingPlanRepository(_db);
                var trainingPlan = await repository.GetTrainingPlanAsync(trainingPlanId);

                ViewBag.Pending = pending;
                ViewBag.InstructionalMethodsList = await _db.InstructionalMethods.OrderBy(o => o.SortOrder).ToListAsync();
                ViewBag.TrainingPlanLengthsList = await _db.TrainingPlanLengths.OrderBy(o => o.SortOrder).ToListAsync();

                return View(view, trainingPlan);
            }

            ViewBag.ErrorMessage = $"An error occurred assigning pending change {pendingId} to training plan {trainingPlanId}";
            return View("NotFound");
        }



        [HttpPost]
        protected async Task<IActionResult> BaseTrainingPlanForm(int pendingId, bool isAddition, TrainingPlan model, string view, string redirectUrl)
        {
            PendingProgramModel pending = null;

            foreach (var instructionalMethodId in Request.Form["TrainingPlanInstructionalMethods[].InstructionalMethodId"])
            {
                model.TrainingPlanInstructionalMethods.Add(new TrainingPlanInstructionalMethod
                {
                    InstructionalMethodId = int.Parse(instructionalMethodId),
                    OtherText = (instructionalMethodId == "5" ? Request.Form["TrainingPlanInstructionalMethods[].OtherText"] : String.Empty)
                });
            }

            for (var i = 0; i < model.BreakdownCount; i++)
            {
                model.TrainingPlanBreakdowns.Add(new TrainingPlanBreakdown
                {
                    RowId = int.Parse(Request.Form[$"TrainingPlanBreakdowns[{i + 1}].RowId"]),
                    TrainingModuleTitle = Request.Form[$"TrainingPlanBreakdowns[{i + 1}].TrainingModuleTitle"],
                    LearningObjective = Request.Form[$"TrainingPlanBreakdowns[{i + 1}].LearningObjective"],
                    TotalHours = decimal.Parse(Request.Form[$"TrainingPlanBreakdowns[{i + 1}].TotalHours"]),
                });
            }

            if (model.TrainingPlanBreakdowns.Any(o => o.TotalHours <= 0))
            {
                ModelState.AddModelError("TrainingPlanBreakdowns", "At least one training plan row has total hours of less than zero. Please fix this error.");
            }

            if (isAddition)
            {
                var pendingAddition = _db.PendingProgramAdditions.FirstOrDefault(e => e.Id == pendingId && e.Pending_Change_Status == 0);

                if (pendingAddition != null)
                {
                    if (ModelState.IsValid)
                    {
                        pendingAddition.Requires_OSD_Review = true;
                        pendingAddition.SerializedTrainingPlan = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                        pendingAddition.Date_Updated = DateTime.Now;
                        pendingAddition.Updated_By = HttpContext.User.Identity.Name;
                    }
                    pending = new PendingProgramModel(pendingAddition);
                }
            }
            else
            {
                var pendingChange = _db.PendingProgramChanges.FirstOrDefault(e => e.Id == pendingId && e.Pending_Change_Status == 0);

                if (pendingChange != null)
                {
                    if (ModelState.IsValid)
                    {
                        pendingChange.Requires_OSD_Review = true;
                        pendingChange.SerializedTrainingPlan = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                        pendingChange.Date_Updated = DateTime.Now;
                        pendingChange.Updated_By = HttpContext.User.Identity.Name;
                    }
                    pending = new PendingProgramModel(pendingChange);
                }
            }

            if (pending == null)
            {
                ViewBag.ErrorMessage = $"Program with id = {pendingId} cannot be found";
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                await _db.SaveChangesAsync();
                return Redirect(redirectUrl);
            }

            ViewBag.Pending = pending;
            ViewBag.InstructionalMethodsList = await _db.InstructionalMethods.OrderBy(o => o.SortOrder).ToListAsync();
            ViewBag.TrainingPlanLengthsList = await _db.TrainingPlanLengths.OrderBy(o => o.SortOrder).ToListAsync();

            return View(view, model);
        }

        protected async Task<IActionResult> BaseDeactivateTrainingPlan(int id, int programId, string redirectUrl)
        {
            var repository = new TrainingPlanRepository(_db);
            await repository.DeactivateProgramTrainingPlanAsync(id, programId);
            return Redirect(redirectUrl);
        }

        protected async Task<IActionResult> BaseActivateTrainingPlan(int id, int programId, string redirectUrl)
        {
            var repository = new TrainingPlanRepository(_db);
            await repository.ActivateProgramTrainingPlanAsync(id, programId);
            return Redirect(redirectUrl);
        }


    }
}