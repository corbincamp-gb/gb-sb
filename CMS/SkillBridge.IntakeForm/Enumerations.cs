using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntakeForm.Models
{
    public class Enumerations
    {
        public enum Status
        {
            Scrapped = 0, Good = 1, Draft = 2
        }

        public enum EntryStatus
        {
            Started = 1, Submitted, Incomplete, UnderReview, PendingDetermination, YesIf, Approved, Rejected
        }

        public enum TemplateType
        {
            NotDefined = 0, MainApplication, ProgramForm
        }

        public enum PartType
        {
            NotDefined = 0, MainApplication_Contacts, MainApplication_OrganizationInformation, ProgramForm_Contacts, ProgramForm_ProgramInformation, ProgramForm_JobInformation, ProgramForm_LocationDetails, ProgramForm_Costs, ProgramForm_TrainingPlan
        }

        public enum QuestionType
        {
            Text = 1, Textarea, Select, Multiselect, RadioButtonList, CheckBoxList, CheckBoxList2Columns, FileUpload, MultiFileUpload, Table, PhoneNumber, Email, Number, State
        }

        public enum AnswerType
        {
            Option = 1, Other, None
        }

    }
}
