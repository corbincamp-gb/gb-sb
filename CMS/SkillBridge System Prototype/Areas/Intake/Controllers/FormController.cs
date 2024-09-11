using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.Intake.Data;
using System.Threading.Tasks;
using System;
using IntakeForm.Models.Data.Templates;
using System.Collections.Generic;
using IntakeForm.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Skillbridge.Business.Data;
using Skillbridge.Business.Model.Db;

namespace SkillBridge_System_Prototype.Intake.Controllers
{
    [Area("Intake")]
    public class FormController : SkillBridge_System_Prototype.Controllers.CmsController
    {
        private readonly ILogger _logger;
        private readonly ITemplateRepository _templateRepository;
        private readonly IFormRepository _formRepository;

        public FormController(ILogger<FormController> logger, ITemplateRepository templateRepository, IFormRepository formRepository, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db) : base(roleManager, userManager, db)
        {
            _logger = logger;
            _templateRepository = templateRepository;
            _formRepository = formRepository;
        }


        private async Task PopulateSharedVariables(int id, string zohoTicketId)
        {
            var entry = await _formRepository.GetEntryByZohoTicketId(zohoTicketId);

            ViewBag.Entry = entry;

            if (entry != null)
            {
                ViewBag.ProgressBar = await _formRepository.GetEntryResponses(entry.ID);
            }

            ViewBag.FormTemplates = new List<DeserializedFormTemplate>
            {
                await _templateRepository.GetCurrentFormTemplate(Enumerations.TemplateType.MainApplication),
                await _templateRepository.GetCurrentFormTemplate(Enumerations.TemplateType.ProgramForm)
            };

            ViewBag.States = await _formRepository.GetStates();
            ViewBag.Form = await _formRepository.GetForm(id);
            ViewBag.ViewOnly = false;
        }

        private async Task PopulateSharedVariables(int id, string zohoTicketId, int partId)
        {
            await PopulateSharedVariables(id, zohoTicketId);

            var responses = await _formRepository.GetFormResponses(id, partId);

            ViewBag.CurrentPart = partId;
            ViewBag.Responses = responses;

            var form = (IntakeForm.Models.Data.Forms.Form)ViewBag.Form;

            var formTemplates = (List<DeserializedFormTemplate>)ViewBag.FormTemplates;
            var formTemplate = formTemplates.FirstOrDefault(o => o.ID == form.FormTemplateID);

            if (formTemplate != null && formTemplate.Parts != null)
            {
                var part = formTemplate.Parts.Where(o => o.ID == partId).FirstOrDefault();

                if (part != null)
                {
                    ViewBag.Part = part;
                }
            }

        }

        public async Task<IActionResult> Index(int id, string zohoTicketId)
        {
            return await Introduction(id, zohoTicketId);
        }

        public async Task<IActionResult> Introduction(int id, string zohoTicketId)
        {
            await PopulateSharedVariables(id, zohoTicketId);

            if (ViewBag.Entry == null) return RedirectToAction("Index", "IntakeHome", new { ZohoTicketId = zohoTicketId });

            var entry = (IntakeForm.Models.Data.Forms.Entry)ViewBag.Entry;

            // If the entry cannot be edited, send them to the review screen
            if (!entry.CanEdit())
            {
                return await Review(id, zohoTicketId);
            }

            return View("~/Areas/Intake/Views/Form/Introduction.cshtml");
        }


        public async Task<IActionResult> Part(int id, string zohoTicketId, int partId = 1)
        {
            await PopulateSharedVariables(id, zohoTicketId, partId);

            if (ViewBag.Entry == null) return RedirectToAction("Index", "IntakeHome", new { ZohoTicketId = zohoTicketId });

            var entry = (IntakeForm.Models.Data.Forms.Entry)ViewBag.Entry;

            // If the entry cannot be edited, send them to the review screen
            if (!entry.CanEdit())
            {
                return await Review(id, zohoTicketId);
            }

            return View("~/Areas/Intake/Views/Form/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> SavePart(int id, string zohoTicketId, int partId)
        {
            await PopulateSharedVariables(id, zohoTicketId, partId);

            if (ViewBag.Entry == null) return RedirectToAction("Index", "IntakeHome", new { ZohoTicketId = zohoTicketId });

            var entry = (IntakeForm.Models.Data.Forms.Entry)ViewBag.Entry;

            // If the entry cannot be edited, send them to the review screen
            if (!entry.CanEdit())
            {
                return await Review(id, zohoTicketId);
            }

            // Determine if the form is view-only, which means you can only save the file uploads but no other parts of the form
            var form = (IntakeForm.Models.Data.Forms.Form)ViewBag.Form;

            var partObj = (IntakeForm.Models.Data.Templates.Part)ViewBag.Part;

            // Loop through each question and make sure each required field is filled in

            foreach (var sectionObj in partObj.Sections)
            {
                foreach (var question in sectionObj.Questions)
                {
                    if (question.Required)
                    {
                        var answers = Request.Form["question_" + question.ID];

                        switch (question.QuestionType)
                        {
                            case Enumerations.QuestionType.RadioButtonList:
                            case Enumerations.QuestionType.CheckBoxList:
                            case Enumerations.QuestionType.CheckBoxList2Columns:
                            case Enumerations.QuestionType.Select:
                            case Enumerations.QuestionType.Multiselect:
                            case Enumerations.QuestionType.State:

                                if (!answers.Any())
                                {
                                    if (question.AnswerChoices.Count > 1)
                                    {
                                        ModelState.AddModelError("response_" + question.ID, "You must select at least one choice as a response to the following: " + question.QuestionText);
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("response_" + question.ID, "You must check the box associated with this statement: " + question.AnswerChoices[0].AnswerText);
                                    }
                                }
                                else
                                {
                                    if (question.MaxResponse > 0)
                                    {
                                        if (answers.Count() > question.MaxResponse)
                                        {
                                            ModelState.AddModelError("response_" + question.ID, "You may only select a maximum of " + question.MaxResponse + " choice(s) as a response to the following: " + question.QuestionText);
                                        }
                                    }

                                    foreach (var answer in answers)
                                    {
                                        foreach (var answerChoice in question.AnswerChoices)
                                        {
                                            if (answerChoice.ID.ToString() == answer && answerChoice.AnswerType == Enumerations.AnswerType.Other && String.IsNullOrWhiteSpace(Request.Form["response_" + question.ID]))
                                            {
                                                ModelState.AddModelError("response_" + question.ID, "You must specify your response to the following: " + question.QuestionText);
                                            }
                                        }
                                    }
                                }
                                break;

                            case Enumerations.QuestionType.PhoneNumber:
                                var phoneRegex = new Regex(@"\(\d\d\d\) \d\d\d\-\d\d\d\d(.*)");
                                foreach (var answer in answers)
                                {
                                    if (!phoneRegex.IsMatch(answer))
                                    {
                                        ModelState.AddModelError("question_" + question.ID, $"Your response to {question.QuestionText} must be in the format (###) ###-####. You entered {answer}.");
                                    }
                                }
                                break;

                            case Enumerations.QuestionType.Email:
                                foreach (var answer in answers)
                                {
                                    if (answer.IndexOf("@") < 1)
                                    {
                                        ModelState.AddModelError("question_" + question.ID, $"Your response to {question.QuestionText} is not a valid email address. You entered {answer}.");
                                    }
                                }
                                break;

                            case Enumerations.QuestionType.Number:
                                decimal d;
                                foreach (var answer in answers)
                                {
                                    if (!decimal.TryParse(answer, out d))
                                    {
                                        ModelState.AddModelError("question_" + question.ID, $"Your response to {question.QuestionText} must be a valid number. You entered {answer}.");
                                    }
                                }
                                break;

                            case Enumerations.QuestionType.Table:

                                foreach (var col in question.AnswerColumns)
                                {
                                    if (col.Required)
                                    {
                                        if (!Request.Form.Any(o => o.Key.StartsWith($"question_{question.ID}_{col.ID}")) || Request.Form.Any(o => o.Key.StartsWith($"question_{question.ID}_{col.ID}") && String.IsNullOrWhiteSpace(o.Value)))
                                        {
                                            ModelState.AddModelError("question_" + question.ID + "_" + col.ID, "You must supply a " + col.Label + " for all entries.");
                                        }
                                    }
                                }

                                break;

                            case Enumerations.QuestionType.FileUpload:
                            case Enumerations.QuestionType.MultiFileUpload:

                                break;

                            default:

                                if (String.IsNullOrWhiteSpace(Request.Form["question_" + question.ID]))
                                {
                                    ModelState.AddModelError("question_" + question.ID, "Your changes were not saved. You must provide a response for the following: " + question.QuestionText);
                                }
                                break;
                        }
                    }

                    // Regardless of whether the question is required, check is any of the answer choices are "none" and make sure no other answer choices are selected
                    if ((question.QuestionType == Enumerations.QuestionType.CheckBoxList || question.QuestionType == Enumerations.QuestionType.CheckBoxList2Columns) && question.AnswerChoices.Any(o => o.AnswerType == Enumerations.AnswerType.None))
                    {
                        var answers = Request.Form["question_" + question.ID];
                        if (answers.Count > 1 && answers.Contains(question.AnswerChoices.First(c => c.AnswerType == Enumerations.AnswerType.None).ID.ToString()))
                        {
                            ModelState.AddModelError("question_" + question.ID, "You may only choose one option if you select " + question.AnswerChoices.First(c => c.AnswerType == Enumerations.AnswerType.None).AnswerText + " for " + question.QuestionText);
                        }
                    }

                    // If there are any child questions and the response to the parent question matches the answer choice required to display the child questions, validate required fields
                    if (question.ChildQuestions != null && question.ChildQuestions.Any() && Request.Form["question_" + question.ID].Any(o => question.ChildQuestionsAppearUsingAnswerChoiceIDs.Contains(int.Parse(o))))
                    {
                        // Pull child questions and grandchild questions into an array to loop through
                        var childQuestions = new List<Question>();
                        childQuestions.AddRange(question.ChildQuestions);
                        foreach (var childQ in question.ChildQuestions)
                        {
                            foreach (var childResponse in Request.Form["question_" + childQ.ID])
                            {
                                if (childQ.ChildQuestions != null && childQ.ChildQuestions.Count > 0 && (childQ.ChildQuestionsAppearUsingAnswerChoiceIDs.Contains(int.Parse(childResponse))))
                                {
                                    childQuestions.AddRange(childQ.ChildQuestions);
                                }
                            }
                        }

                        foreach (var subq in childQuestions)
                        {
                            var answers = Request.Form["question_" + question.ID];

                            // Child questions are required if it's not specific to a particular answer choice OR when it is specific to a particular answer choice and that answer choice was selected
                            if (!subq.ChildQuestionOfAnswerChoiceIDs.Any() || subq.ChildQuestionOfAnswerChoiceIDs.Any(o => answers.Contains(o.ToString())))
                            {
                                switch (subq.QuestionType)
                                {
                                    case Enumerations.QuestionType.Text:
                                    case Enumerations.QuestionType.Textarea:
                                    case Enumerations.QuestionType.PhoneNumber:
                                    case Enumerations.QuestionType.Email:
                                    case Enumerations.QuestionType.Number:

                                        if (String.IsNullOrWhiteSpace(Request.Form["question_" + subq.ID]))
                                        {
                                            ModelState.AddModelError("question_" + subq.ID, "You must provide a response for the following: " + subq.QuestionText);
                                        }
                                        break;

                                    case Enumerations.QuestionType.RadioButtonList:
                                    case Enumerations.QuestionType.CheckBoxList:
                                    case Enumerations.QuestionType.CheckBoxList2Columns:
                                    case Enumerations.QuestionType.Select:
                                    case Enumerations.QuestionType.Multiselect:
                                    case Enumerations.QuestionType.State:

                                        var subAnswers = Request.Form["question_" + subq.ID];
                                        if (!subAnswers.Any())
                                        {
                                            if (question.AnswerChoices.Count > 1)
                                            {
                                                ModelState.AddModelError("response_" + subq.ID, "You must select at least one choice as a response to the following: " + subq.QuestionText);
                                            }
                                            else
                                            {
                                                ModelState.AddModelError("response_" + subq.ID, "You must check the box associated with this statement: " + subq.AnswerChoices[0].AnswerText);
                                            }
                                        }
                                        else
                                        {
                                            if (subq.MaxResponse > 0)
                                            {
                                                if (subAnswers.Count() > subq.MaxResponse)
                                                {
                                                    ModelState.AddModelError("response_" + subq.ID, "You may only select a maximum of " + subq.MaxResponse + " choice(s) as a response to the following: " + subq.QuestionText);
                                                }
                                            }

                                            foreach (var subAnswer in subAnswers)
                                            { 
                                                foreach (var answerChoice in subq.AnswerChoices)
                                                {
                                                    if (answerChoice.ID.ToString() == subAnswer && answerChoice.AnswerType == Enumerations.AnswerType.Other && String.IsNullOrWhiteSpace(Request.Form["response_" + subq.ID]))
                                                    {
                                                        ModelState.AddModelError("response_" + subq.ID, "You must specify your response to the following: " + subq.QuestionText);
                                                    }
                                                }
                                            }
                                        }
                                        break;

                                    case Enumerations.QuestionType.Table:

                                        foreach (var col in subq.AnswerColumns)
                                        {
                                            if (col.Required)
                                            {
                                                if (!Request.Form.Any(o => o.Key.StartsWith($"question_{subq.ID}_{col.ID}")) || Request.Form.Any(o => o.Key.StartsWith($"question_{subq.ID}_{col.ID}") && String.IsNullOrWhiteSpace(o.Value)))
                                                {
                                                    ModelState.AddModelError("question_" + subq.ID + "_" + col.ID, "You must supply a " + col.Label + " for all entries.");
                                                }
                                            }
                                        }

                                        break;

                                }
                            }
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var msgs = new List<string>();
                var errors = new List<string>();

                var formTemplates = (List<IntakeForm.Models.Data.Templates.DeserializedFormTemplate>)ViewBag.FormTemplates;
                var formTemplate = formTemplates.FirstOrDefault(o => o.ID == form.FormTemplateID);
                var formTemplatePart = formTemplate.Parts.FirstOrDefault(o => o.ID == partId);

                var models = new List<IntakeForm.Models.Data.Forms.FormResponse>();

                // Loop through each question
                foreach (var question in Request.Form["QuestionID"])
                {
                    var questionID = 0;

                    if (int.TryParse(question, out questionID))
                    {
                        // Because every section is displayed on the part page, we have to loop through the sections within the part to find if the question belongs to that section
                        foreach (var formTemplateSection in formTemplatePart.Sections)
                        {
                            // NOTE: this is hard coded to search only 3-levels deep; as of the initial release of this form, we do not have questions that nest deeper than that. 
                            // If that ever happens, this function will need to be rewritten as a recursive one to find the question as deeply as it may live.
                            var formTemplateQuestion = formTemplateSection.Questions.FirstOrDefault(o => o.ID == questionID || o.ChildQuestions.Any(c => c.ID == questionID || c.ChildQuestions.Any(cq => cq.ID == questionID)));
                            var isChildQuestion = false;

                            // Loop through "recursively" to find the child question in the form
                            while (formTemplateQuestion != null && formTemplateQuestion.ID != questionID)
                            {
                                formTemplateQuestion = formTemplateQuestion.ChildQuestions.FirstOrDefault(o => o.ID == questionID || o.ChildQuestions.Any(c => c.ID == questionID));
                                if (formTemplateQuestion != null) isChildQuestion = true;
                            }

                            if (formTemplateQuestion != null)
                            {
                                var model = models.FirstOrDefault(o => o.FormID == id && o.PartID == formTemplatePart.ID && o.SectionID == formTemplateSection.ID && o.QuestionID == questionID);

                                if (model == null)
                                {
                                    model = new IntakeForm.Models.Data.Forms.FormResponse { FormID = id, PartID = formTemplatePart.ID, SectionID = formTemplateSection.ID, QuestionID = questionID };
                                }

                                // Identify the question type and save things accordingly
                                switch (formTemplateQuestion.QuestionType)
                                {
                                    case Enumerations.QuestionType.Text:
                                    case Enumerations.QuestionType.Textarea:
                                    case Enumerations.QuestionType.Email:
                                    case Enumerations.QuestionType.PhoneNumber:
                                    case Enumerations.QuestionType.Number:
                                    case Enumerations.QuestionType.State:

                                        model.IsResponseRequired = formTemplateQuestion.Required;
                                        model.Answer = Request.Form["question_" + question];
                                        if (models.IndexOf(model) == -1 && model.IsResponseRequired || !String.IsNullOrWhiteSpace(model.Answer))
                                        {
                                            models.Add(model);
                                        }

                                        break;

                                    case Enumerations.QuestionType.FileUpload:
                                    case Enumerations.QuestionType.MultiFileUpload:

                                        // Assign all files that were selected
                                        if (Request.Form["files_" + question].Any())
                                        {
                                            model.FormResponseFiles.AddRange(Request.Form["files_" + question].Select(o => new IntakeForm.Models.Data.Forms.FormResponseFile { FileID = int.Parse(o) }).ToList());
                                        }

                                        if (models.IndexOf(model) == -1 && model.FormResponseFiles != null)
                                        {
                                            model.IsResponseRequired = formTemplateQuestion.Required;
                                            models.Add(model);
                                        }

                                        break;

                                    case Enumerations.QuestionType.Table:

                                        foreach (var col in formTemplateQuestion.AnswerColumns)
                                        {
                                            foreach (var field in Request.Form.Where(o => o.Key.StartsWith($"question_{question}_{col.ID}")).ToList())
                                            {
                                                model.FormResponseRows.Add(new IntakeForm.Models.Data.Forms.FormResponseRow { RowID = int.Parse(field.Key.Substring(field.Key.LastIndexOf("_") + 1)), ColumnID = col.ID, Answer = field.Value });
                                            }
                                        }

                                        if (models.IndexOf(model) == -1 && model.FormResponseRows != null)
                                        {
                                            model.IsResponseRequired = formTemplateQuestion.Required;
                                            models.Add(model);
                                        }

                                        break;

                                    default:

                                        model.IsResponseRequired = formTemplateQuestion.Required;
                                        model.FormResponseChoices = Request.Form["question_" + question].Select(o => new IntakeForm.Models.Data.Forms.FormResponseChoice { AnswerChoiceID = int.Parse(o) }).ToList();
                                        model.Answer = Request.Form["response_" + question];

                                        if (models.IndexOf(model) == -1 && model.IsResponseRequired || !String.IsNullOrWhiteSpace(model.Answer) || model.FormResponseChoices != null)
                                        {
                                            model.IsResponseRequired = formTemplateQuestion.Required;
                                            models.Add(model);
                                        }

                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        errors.Add($"Could not convert question {question} to an integer.");
                    }
                }

                try
                {
                    await _formRepository.SaveFormResponses(id, partId, models);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }

                if (errors.Count == 0)
                {
                    msgs.Add("You have successfully saved your response(s).");
                }

                ViewBag.Messages = msgs;
                ViewBag.Errors = errors;
            }

            return await Part(id, zohoTicketId, partId);
        }

        public async Task<IActionResult> Submit(int id, string zohoTicketId)
        {
            await PopulateSharedVariables(id, zohoTicketId);

            if (ViewBag.Entry == null) return RedirectToAction("Index", "IntakeHome", new { ZohoTicketId = zohoTicketId });

            var entry = (IntakeForm.Models.Data.Forms.Entry)ViewBag.Entry;

            // If the entry cannot be edited, send them to the review screen
            if (!entry.CanEdit())
            {
                return await Review(id, zohoTicketId);
            }

            return View("~/Areas/Intake/Views/Form/Submit.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int id)
        {
            var msgs = new List<string>();
            var errors = new List<string>();

            try
            {
                var entry = await _formRepository.SubmitEntry(id);
                msgs.Add("You have successfully submitted your application.");

                ViewBag.Messages = msgs;
                return await Review(entry.ID, entry.ZohoTicketId);
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }


            return View("~/Areas/Intake/Views/Form/Submit.cshtml");
        }

        public async Task<IActionResult> Review(int id, string zohoTicketId)
        {
            await PopulateSharedVariables(id, zohoTicketId);

            if (ViewBag.Entry == null) return RedirectToAction("Index", "IntakeHome", new { ZohoTicketId = zohoTicketId });

            var entry = (IntakeForm.Models.Data.Forms.Entry)ViewBag.Entry;

            // If the status of the entry is less than "submitted", send them to the introduction screen
            if (entry.EntryStatusID < (int)Enumerations.EntryStatus.Submitted)
            {
                return await Introduction(id, zohoTicketId);
            }

            return View("~/Areas/Intake/Views/Form/Review.cshtml");
        }

    }
}