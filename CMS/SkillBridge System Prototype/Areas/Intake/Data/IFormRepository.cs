using IntakeForm.Models;
using IntakeForm.Models.Data.Forms;
using IntakeForm.Models.Data.Templates;
using IntakeForm.Models.View.Admin;
using IntakeForm.Models.View.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Intake.Data
{
    public interface IFormRepository
    {
        /* Entries */
        Task<List<Entry>> GetEntries(AdminSearchModel searchModel);
        Task<Entry?> GetEntry(int id);
        Task<Entry?> GetEntryByZohoTicketId(string zohoTicketId);
        Task<Entry> SaveEntry(Entry model);
        Task<Entry> SubmitEntry(int id);
        Task<Entry> MarkAsReviewedByAnalyst(int id, int entryStatusId, string notes, string addedBy);
        Task<Entry> MarkAsReviewedByOsd(int id, string notes, string addedBy);
        Task<Entry> MakeDetermination(int id, int entryStatusId, string notes, string rejectionReason, string addedBy);


        /* Forms */
        Task<List<ProgressBarState>> GetEntryProgress(int entryID);
        Task<Form?> GetForm(int formID);
        Task<List<FormResponse>> GetEntryResponses(int entryID);
        Task<List<FormResponse>> GetFormResponses(int formID, int partID);
        Task<bool> SaveFormResponses(int formID, int partID, List<FormResponse> response);
        Task<bool> RemoveFormResponses(int formID, List<FormResponse> responses);

        /* Lookups */
        Task<List<State>> GetStates();
    }
}
