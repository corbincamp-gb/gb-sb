using IntakeForm.Models;
using IntakeForm.Models.Data.Forms;
using IntakeForm.Models.View.Admin;
using IntakeForm.Models.View.Forms;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Intake.Data
{
    public class FormRepository : IFormRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IntakeFormContext _db;
        private readonly ILogger<FormRepository> _logger;
        private readonly IEmailSender _emailSender;

        public FormRepository(IConfiguration configuration, ILogger<FormRepository> logger, IntakeFormContext db, IEmailSender emailSender)
        {
            _configuration = configuration;
            _logger = logger;
            _db = db;
            _emailSender = emailSender;
        }

        #region Lookup DAL

        public async Task<List<State>> GetStates()
        {
            return await _db.States.OrderBy(o => o.Code).ToListAsync();
        }

        #endregion

        #region Entry DAL

        public async Task<List<Entry>> GetEntries(AdminSearchModel searchModel)
        {
            var sql = _db.Entries.AsQueryable();

            if (searchModel.EntryStatusId.HasValue)
            {
                sql = sql.Where(o => o.EntryStatusID == searchModel.EntryStatusId.Value);
            }

            if (!String.IsNullOrWhiteSpace(searchModel.ZohoTicketId))
            {
                sql = sql.Where(o => o.ZohoTicketId.Contains(searchModel.ZohoTicketId));
            }

            if (!String.IsNullOrWhiteSpace(searchModel.Organization))
            {
                sql = sql.Where(o => o.OrganizationName.Contains(searchModel.Organization));
            }

            if (searchModel.SubmittedStartingOn.HasValue)
            {
                sql = sql.Where(o => o.SubmissionDate.HasValue && o.SubmissionDate.Value >= searchModel.SubmittedStartingOn.Value);
            }

            if (searchModel.SubmittedEndingOn.HasValue)
            {
                sql = sql.Where(o => o.SubmissionDate.HasValue && o.SubmissionDate.Value <= searchModel.SubmittedEndingOn.Value.AddDays(1).AddSeconds(-1));
            }

            if (searchModel.LastUpdatedStartingOn.HasValue)
            {
                sql = sql.Where(o => o.UpdatedDate >= searchModel.LastUpdatedStartingOn.Value);
            }

            if (searchModel.LastUpdatedEndingOn.HasValue)
            {
                sql = sql.Where(o => o.UpdatedDate <= searchModel.LastUpdatedEndingOn.Value.AddDays(1).AddSeconds(-1));
            }

            switch (searchModel.SortBy.ToLower())
            {
                case "zohoticketid":
                    sql = (searchModel.SortOrder.ToLower() == "asc" ? sql.OrderBy(o => o.ZohoTicketId) : sql.OrderByDescending(o => o.ZohoTicketId));
                    break;
                case "organizationname":
                    sql = (searchModel.SortOrder.ToLower() == "asc" ? sql.OrderBy(o => o.OrganizationName) : sql.OrderByDescending(o => o.OrganizationName));
                    break;
                case "entrystatusid":
                    sql = (searchModel.SortOrder.ToLower() == "asc" ? sql.OrderBy(o => o.EntryStatusID) : sql.OrderByDescending(o => o.EntryStatusID));
                    break;
                case "numberofprograms":
                    sql = (searchModel.SortOrder.ToLower() == "asc" ? sql.OrderBy(o => o.NumberOfPrograms) : sql.OrderByDescending(o => o.NumberOfPrograms));
                    break;
                case "submissiondate":
                    sql = (searchModel.SortOrder.ToLower() == "asc" ? sql.OrderBy(o => o.SubmissionDate) : sql.OrderByDescending(o => o.SubmissionDate));
                    break;
                default:
                    sql = (searchModel.SortOrder.ToLower() == "asc" ? sql.OrderBy(o => o.UpdatedDate) : sql.OrderByDescending(o => o.UpdatedDate)); 
                    break;
            }

            return await sql
                .Include(o => o.State)
                .ToListAsync();
        }

        public async Task<Entry?> GetEntry(int id)
        {
            return await _db.Entries
                .Include(o => o.Forms)
                .Include("Forms.FormTemplate")
                .Include(o => o.State)
                .Include(o => o.EntryStatusTracking)
                .FirstOrDefaultAsync(o => o.ID == id);
        }

        public async Task<Entry?> GetEntryByZohoTicketId(string zohoTicketId)
        {
            var entryId = await _db.Entries.Where(o => o.ZohoTicketId == zohoTicketId).Select(o => o.ID).FirstOrDefaultAsync();
            return await GetEntry(entryId);
        }

        public async Task<Entry> SaveEntry(Entry model)
        {
            var now = DateTime.Now;

            var entry = await _db.Entries.Include(o => o.Forms).FirstOrDefaultAsync(o => o.ZohoTicketId == model.ZohoTicketId);

            if (entry == null)
            {
                // Create entry
                entry = new Entry { 
                    ZohoTicketId = model.ZohoTicketId, 
                    EntryStatusID = (int)Enumerations.EntryStatus.Started, 
                    AddedDate = now,
                    EntryStatusTracking = new List<EntryStatusTracking> { new EntryStatusTracking { OldEntryStatusID = 0, NewEntryStatusID = (int)Enumerations.EntryStatus.Started, Notes = String.Empty, Role = "Applicant", AddedDate = now, AddedBy = "Applicant" } },
                    Forms = new List<Form>()  
                };

                _db.Entries.Add(entry);
            }

            entry.OrganizationName = model.OrganizationName;
            entry.Ein = model.Ein;
            entry.Address1 = model.Address1;
            entry.Address2 = model.Address2;
            entry.City = model.City;
            entry.StateId = model.StateId;
            entry.ZipCode = model.ZipCode;
            entry.PhoneNumber = model.PhoneNumber;
            entry.Url = model.Url;
            entry.PocFirstName = model.PocFirstName;
            entry.PocLastName = model.PocLastName;
            entry.PocTitle= model.PocTitle;
            entry.PocPhoneNumber = model.PocPhoneNumber;
            entry.PocEmail = model.PocEmail;
            entry.NumberOfPrograms = model.NumberOfPrograms;
            entry.InternalNotes = model.InternalNotes;
            entry.RejectionReason = model.RejectionReason;
            entry.ExternalNotes = model.ExternalNotes;
            entry.UpdatedDate = now;

            // Determine if we need to create the main application
            if (!entry.Forms.Any(o => o.FormTemplateID == (int)Enumerations.TemplateType.MainApplication))
            {
                entry.Forms.Add(new Form
                {
                    FormTemplateID = (int)Enumerations.TemplateType.MainApplication,
                    FormOrder = 1,
                    AddedDate = now,
                    UpdatedDate = now
                });
            }

            // Determine how many program forms we need to create
            var numberOfProgramForms = model.NumberOfPrograms - entry.Forms.Count(o => o.FormTemplateID == (int)Enumerations.TemplateType.ProgramForm);

            // If it's positive, add program forms
            if (numberOfProgramForms > 0)
            {
                for (var i = 0; i < numberOfProgramForms; i++)
                {
                    entry.Forms.Add(new Form
                    {                         
                        FormTemplateID = (int)Enumerations.TemplateType.ProgramForm,
                        FormOrder = i + 2,
                        AddedDate = now,
                        UpdatedDate = now
                    });
                }
            }

            // If it's negative, delete program forms
            if (numberOfProgramForms < 0)
            {
                for (var i = entry.Forms.Count() - 1; i <= -numberOfProgramForms; i--)
                {
                    entry.Forms.RemoveAt(i);
                }
            }

            await _db.SaveChangesAsync();

            return entry;
        }

        public async Task<Entry> SubmitEntry(int id)
        {
            var now = DateTime.Now;

            var entry = await _db.Entries.Include(o => o.State).Include(o => o.EntryStatusTracking).FirstOrDefaultAsync(o => o.ID == id);

            if (entry != null)
            {
                var newStatusId = (int)Enumerations.EntryStatus.Submitted;

                // When entry is in "yes, if" status and gets submitted, it bypasses the analyst and moves to the OSD Reviewer
                if (entry.EntryStatusID == (int)Enumerations.EntryStatus.YesIf)
                {
                    newStatusId = (int)Enumerations.EntryStatus.PendingDetermination;
                }

                // Add entry status tracking
                if (entry.EntryStatusTracking == null) entry.EntryStatusTracking = new List<EntryStatusTracking>();
                entry.EntryStatusTracking.Add(new EntryStatusTracking { OldEntryStatusID = entry.EntryStatusID, NewEntryStatusID = newStatusId, Role = "Applicant", Notes = "", AddedDate = now, AddedBy = "Applicant" });

                // Update form status to indicate the form has been completed and submitted
                entry.EntryStatusID = newStatusId;
                entry.SubmissionDate = now;
                entry.UpdatedDate = now;

                await _db.SaveChangesAsync();

                // Send email confirmation 
                try
                {
                    var body = new StringBuilder();
                    if (newStatusId == (int)Enumerations.EntryStatus.UnderReview)
                    {
                        body.AppendLine($"<p>Thank you for updating your application to join the SkillBridge program! Your application is now pending review and you will hear back from us when a determination has been made.</p>");
                        body.AppendLine($"<div><strong>Track your application at:</strong> <a href=\"{_configuration["SiteUrl"]}/intake/form/introduction/{entry.ZohoTicketId}\">{_configuration["SiteUrl"]}/intake/form/introduction/{entry.ZohoTicketId}</a></div>");
                    }
                    else
                    {
                        body.AppendLine($"<p>Thank you for submitting your application to join the SkillBridge program! We have received the following information from you:</p>");
                        body.AppendLine($"<div><strong>Organization:</strong> {entry.OrganizationName}</div>");
                        body.AppendLine($"<div><strong>Address:</strong> {entry.Address1} {entry.Address2} {entry.City}, {entry.State.Code} {entry.ZipCode}</div>");
                        body.AppendLine($"<div><strong>POC:</strong> {entry.PocFirstName} {entry.PocLastName}</div>");
                        body.AppendLine($"<div><strong>Ticket ID:</strong> {entry.ZohoTicketId}</div>");
                        body.AppendLine($"<div><strong>Link to application:</strong> <a href=\"{_configuration["SiteUrl"]}/intake/form/introduction/{entry.ZohoTicketId}\">{_configuration["SiteUrl"]}/intake/form/introduction/{entry.ZohoTicketId}</a></div>");
                    }

                    await _emailSender.SendEmailAsync(entry.PocEmail, "SkillBridge Application Confirmation", body.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error submitting entry {id} to email {entry.PocEmail} for organization {entry.OrganizationName}");
                }

                return entry;
            }

            return null;
        }

        public async Task<Entry> MarkAsReviewedByAnalyst(int id, int entryStatusId, string notes, string addedBy)
        {
            var now = DateTime.Now;

            var entry = await _db.Entries.FirstOrDefaultAsync(o => o.ID == id);

            if (entry != null)
            {
                // Add entry status tracking
                if (entry.EntryStatusTracking == null) entry.EntryStatusTracking = new List<EntryStatusTracking>();
                entry.EntryStatusTracking.Add(new EntryStatusTracking { OldEntryStatusID = entry.EntryStatusID, NewEntryStatusID = entryStatusId, Role = "Analyst", Notes = notes, AddedDate = now, AddedBy = addedBy });

                // Update status 
                entry.EntryStatusID = entryStatusId;
                entry.ReviewedByAnalyst = true;
                entry.UpdatedDate = now;

                await _db.SaveChangesAsync();

                // Send email notification, if necessary
                if (entryStatusId == (int)Enumerations.EntryStatus.Incomplete)
                {
                    try
                    {
                        var body = new StringBuilder();
                        body.AppendLine($"<p>We have reviewed your SkillBridge application and have marked it as <strong>incomplete</strong> for the following reason(s):</p>");
                        body.AppendLine($"<div><pre>{notes}</pre></div>");
                        body.AppendLine($"<p><strong>Please return to your application by clicking the following link and updating it:</strong> <a href=\"{_configuration["SiteUrl"]}/intake/form/introduction/{entry.ZohoTicketId}\">{_configuration["SiteUrl"]}/intake/form/introduction/{entry.ZohoTicketId}</a></p>");

                        await _emailSender.SendEmailAsync(entry.PocEmail, "SkillBridge Application Requires Modifications", body.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error marking as reviewed by analyst entry {id} to email {entry.PocEmail} for organization {entry.OrganizationName}");
                    }
                }

                return entry;
            }

            return null;
        }

        public async Task<Entry> MarkAsReviewedByOsd(int id, string notes, string addedBy)
        {
            var now = DateTime.Now;

            var entry = await _db.Entries.FirstOrDefaultAsync(o => o.ID == id);

            if (entry != null)
            {
                // Add entry status tracking
                if (entry.EntryStatusTracking == null) entry.EntryStatusTracking = new List<EntryStatusTracking>();
                entry.EntryStatusTracking.Add(new EntryStatusTracking { OldEntryStatusID = entry.EntryStatusID, NewEntryStatusID = (int)Enumerations.EntryStatus.PendingDetermination, Role = "OSD Reviewer", Notes = notes, AddedDate = now, AddedBy = addedBy });

                // Update status 
                entry.EntryStatusID = (int)Enumerations.EntryStatus.PendingDetermination;
                entry.ReviewedByOsd = true;
                entry.UpdatedDate = now;

                await _db.SaveChangesAsync();

                return entry;
            }

            return null;
        }

        public async Task<Entry> MakeDetermination(int id, int entryStatusId, string notes, string rejectionReason, string addedBy)
        {
            var now = DateTime.Now;

            var entry = await _db.Entries.FirstOrDefaultAsync(o => o.ID == id);

            if (entry != null)
            {
                // Add entry status tracking
                if (entry.EntryStatusTracking == null) entry.EntryStatusTracking = new List<EntryStatusTracking>();
                entry.EntryStatusTracking.Add(new EntryStatusTracking { OldEntryStatusID = entry.EntryStatusID, NewEntryStatusID = entryStatusId, Role = "OSD Signatory", Notes = (entryStatusId == (int)Enumerations.EntryStatus.Rejected ? rejectionReason : notes), AddedDate = now, AddedBy = addedBy });

                // If the status is Rejected, add rejection reason
                entry.RejectionReason = (entryStatusId == (int)Enumerations.EntryStatus.Rejected ? rejectionReason : String.Empty);

                // Update status 
                entry.EntryStatusID = entryStatusId;
                entry.UpdatedDate = now;

                await _db.SaveChangesAsync();

                // Send email notification, if necessary
                switch (entryStatusId)
                {
                    case (int)Enumerations.EntryStatus.YesIf:
                        try
                        {
                            var body = new StringBuilder();
                            body.AppendLine($"<p>We have reviewed your SkillBridge application and have marked it as requiring modifications before approval for the following reason(s):</p>");
                            body.AppendLine($"<div><pre>{notes}</pre></div>");
                            body.AppendLine($"<p><strong>Please return to your application by clicking the following link and updating it:</strong> <a href=\"{_configuration["SiteUrl"]}/intake/form/introduction/{entry.ZohoTicketId}\">{_configuration["SiteUrl"]}/intake/form/introduction/{entry.ZohoTicketId}</a></p>");

                            await _emailSender.SendEmailAsync(entry.PocEmail, "SkillBridge Application Requires Modifications", body.ToString());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error sending email to {entry.PocEmail} for organization {entry.OrganizationName} while making yes if determination (entry {id})");
                        }
                        break;

                    case (int)Enumerations.EntryStatus.Approved:
                        try
                        {
                            var body = new StringBuilder();
                            body.AppendLine($"<p>We are pleased to share with you that your SkillBridge application has been approved...</p>");

                            await _emailSender.SendEmailAsync(entry.PocEmail, "SkillBridge Application Approved", body.ToString());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error sending email to {entry.PocEmail} for organization {entry.OrganizationName} while making approval determination (entry {id})");
                        }
                        break;

                    case (int)Enumerations.EntryStatus.Rejected:
                        try
                        {
                            var body = new StringBuilder();
                            body.AppendLine($"<p>Your SkillBridge application has been closed out...</p>");

                            await _emailSender.SendEmailAsync(entry.PocEmail, "SkillBridge Application Cloused Out", body.ToString());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error sending email to {entry.PocEmail} for organization {entry.OrganizationName} while making close-out determination (entry {id})");
                        }
                        break;

                }

                return entry;
            }

            return null;
        }

        #endregion

        #region Form DAL

        public async Task<List<ProgressBarState>> GetEntryProgress(int entryID)
        {
            var progress = await _db.ProgressBarStates.FromSqlRaw(@"select fr.ID, fr.FormID, fr.PartID, fr.SectionID, fr.QuestionID, fr.IsResponseRequired, cast(case when Answer <> '' or frc.FormResponseChoiceID is not null or frr.FormResponseRowID is not null or frf.FormResponseFileID is not null then 1 else 0 end as bit) IsComplete
            from FormResponses fr
            join Forms f on fr.FormID = f.ID
            left outer join(select FormResponseID, max(ID) as FormResponseChoiceID from FormResponseChoices group by FormResponseID) frc on fr.ID = frc.FormResponseID
            left outer join(select FormResponseID, max(ID) as FormResponseRowID from FormResponseRows group by FormResponseID) frr on fr.ID = frr.FormResponseID
            left outer join(select FormResponseID, max(ID) as FormResponseFileID from FormResponseFiles group by FormResponseID) frf on fr.ID = frf.FormResponseID
            where f.EntryID = @EntryID"
            , new SqlParameter("EntryID", entryID)).ToListAsync();

            var choices = await _db.FormResponseChoices.FromSqlRaw(@"select frc.* 
            from Forms f
            join FormResponses fr on f.ID = fr.FormID
            join FormResponseChoices frc on fr.ID = frc.FormResponseID
            where f.EntryID = @EntryID"
            , new SqlParameter("EntryID", entryID)).ToListAsync();

            progress.ForEach(o => o.FormResponseChoices = choices.Where(c => c.FormResponseID == o.ID).ToList());

            return progress;
        }

        public async Task<Form?> GetForm(int formID)
        {
            return await _db.Forms
                .Include(o => o.FormTemplate)
                .Where(o => o.ID == formID)
                .FirstOrDefaultAsync();
        }

        public async Task<List<FormResponse>> GetEntryResponses(int entryID)
        {
            return await _db.Forms
                .Where(o => o.EntryID == entryID)
                .SelectMany(o => o.FormResponses)
                .Include(o => o.FormResponseChoices)
                .Include(o => o.FormResponseRows)
                .Include(o => o.FormResponseFiles)
                .ToListAsync();
        }

        public async Task<List<FormResponse>> GetFormResponses(int formID, int partID)
        {
            return await _db.FormResponses
                .Include(o => o.FormResponseChoices)
                .Include(o => o.FormResponseRows)
                .Include(o => o.FormResponseFiles)
                .Where(o => o.FormID == formID && o.PartID == partID)
                .ToListAsync();
        }

        public async Task<bool> SaveFormResponses(int formID, int partID, List<FormResponse> responses)
        {
            var now = DateTime.Now;

            var form = await _db.Forms.Include(o => o.FormResponses).FirstOrDefaultAsync(o => o.ID == formID);

            if (form == null) {
                return false;
            }

            // Remove any responses that no longer pertain to the part
            var removedResponses = form.FormResponses.Where(o => o.PartID == partID && !responses.Select(r => r.QuestionID).Contains(o.QuestionID)).ToList();
            foreach (var removedResponse in removedResponses)
            {
                form.FormResponses.Remove(removedResponse);
            }

            foreach (var response in responses)
            {
                // Find the the form response associated with this particular form, credential, and question
                var formResponse = await _db.FormResponses
                    .Include(o => o.FormResponseChoices)
                    .Include(o => o.FormResponseRows)
                    .Include(o => o.FormResponseFiles)
                    .Where(o => o.FormID == formID && o.QuestionID == response.QuestionID)
                    .FirstOrDefaultAsync();

                // If response doesn't exist, add it
                if (formResponse == null)
                {
                    formResponse = new FormResponse { FormID = formID, PartID = response.PartID, SectionID = response.SectionID, QuestionID = response.QuestionID, AddedDate = now };
                    _db.FormResponses.Add(formResponse);
                }

                formResponse.Answer = response.Answer;
                formResponse.IsResponseRequired = response.IsResponseRequired;
                formResponse.UpdatedDate = now;

                // Remove any choices that aren't in the response
                var removedChoices = formResponse.FormResponseChoices.Where(o => !response.FormResponseChoices.Select(c => c.AnswerChoiceID).ToList().Contains(o.AnswerChoiceID)).ToList();
                foreach (var removedChoice in removedChoices)
                {
                    formResponse.FormResponseChoices.Remove(removedChoice);
                }

                // Add any choices that are in the response but not yet in the form response
                var addedChoices = response.FormResponseChoices.Where(o => !formResponse.FormResponseChoices.Select(c => c.AnswerChoiceID).ToList().Contains(o.AnswerChoiceID)).ToList();
                foreach (var addedChoice in addedChoices)
                {
                    formResponse.FormResponseChoices.Add(new FormResponseChoice { AnswerChoiceID = addedChoice.AnswerChoiceID, AddedDate = now });
                }

                // Remove any rows that aren't in the response
                var removedRows = formResponse.FormResponseRows.Where(o => !response.FormResponseRows.Select(c => c.RowID).ToList().Contains(o.RowID)).ToList();
                foreach (var removedRow in removedRows)
                {
                    formResponse.FormResponseRows.Remove(removedRow);
                }

                // Add/update any rows that are in the response
                foreach (var responseRow in response.FormResponseRows)
                {
                    var formResponseRow = formResponse.FormResponseRows.FirstOrDefault(o => o.RowID == responseRow.RowID && o.ColumnID == responseRow.ColumnID);
                    if (formResponseRow == null) 
                    {
                        formResponseRow = new FormResponseRow { RowID = responseRow.RowID, ColumnID = responseRow.ColumnID, AddedDate = now };
                        formResponse.FormResponseRows.Add(formResponseRow);
                    }

                    formResponseRow.Answer = responseRow.Answer;
                    formResponseRow.UpdatedDate = now;
                }

                // Remove any files that aren't in the response
                var removedFiles = formResponse.FormResponseFiles.Where(o => !response.FormResponseFiles.Select(c => c.FileID).ToList().Contains(o.FileID)).ToList();
                foreach (var removedFile in removedFiles)
                {
                    formResponse.FormResponseFiles.Remove(removedFile);
                }

                // Add any files that are in the response but not yet in the form response
                var addedFiles = response.FormResponseFiles.Where(o => !formResponse.FormResponseFiles.Select(c => c.FileID).ToList().Contains(o.FileID)).ToList();
                foreach (var addedFile in addedFiles)
                {
                    formResponse.FormResponseFiles.Add(new FormResponseFile { FileID = addedFile.FileID, AddedDate = now });
                }
            }

            // Save to database
            form.UpdatedDate = now;
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFormResponses(int formID, List<FormResponse> responses)
        {
            var now = DateTime.Now;

            foreach (var response in responses)
            {
                // Get the removed response to this question
                var removedResponses = await _db.FormResponses
                        .Include(o => o.FormResponseChoices)
                        .Include(o => o.FormResponseFiles)
                        .Where(o => o.FormID == formID && o.QuestionID == response.QuestionID)
                        .ToListAsync();

                _db.FormResponses.RemoveRange(removedResponses);
            }

            // Save to database
            await _db.SaveChangesAsync();

            return true;
        }

        #endregion
    }
}
