using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Forms
{
    /// <summary>
    /// The initial entry that is being filled out
    /// </summary>
    [Table("Entries")]
    public class Entry
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Zoho ticket ID
        /// </summary>
        public string ZohoTicketId { get; set; }

        /// <summary>
        /// Entry status, taken from the Enumerations.EntryStatus enumeration
        /// </summary>
        public int EntryStatusID { get; set; }

        /// <summary>
        /// Name of organization
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// Employer identification number (EIN)
        /// </summary>
        public string Ein { get; set; }

        /// <summary>
        /// Address line 1
        /// </summary>
        public string Address1 { get; set; }

        /// <summary>
        /// Address line 2
        /// </summary>
        public string? Address2 { get; set; }

        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// State ID, foreign key to the States table
        /// </summary>
        public int StateId { get; set; }

        public virtual State? State { get; set; }    

        /// <summary>
        /// Zip code
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Point of contact, first name 
        /// </summary>
        public string PocFirstName { get; set; }

        /// <summary>
        /// Point of contact, last name
        /// </summary>
        public string PocLastName { get; set; }

        /// <summary>
        /// Point of contact, title
        /// </summary>
        public string PocTitle { get; set; }

        /// <summary>
        /// Point of contact, phone number
        /// </summary>
        public string PocPhoneNumber { get; set; }

        /// <summary>
        /// Point of contact, email
        /// </summary>
        public string PocEmail { get; set; }

        /// <summary>
        /// The number of programs associated with this entry
        /// </summary>
        public byte NumberOfPrograms { get; set; }

        /// <summary>
        /// The date the application was submitted 
        /// </summary>
        public DateTime? SubmissionDate { get; set; }

        /// <summary>
        /// Internal notes; not visible to the client
        /// </summary>
        public string? InternalNotes { get; set; }

        /// <summary>
        /// Reason(s) an application was rejected
        /// </summary>
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Notes to the client on why a yes, if determination was made
        /// </summary>
        public string? ExternalNotes { get; set; }

        /// <summary>
        /// Has the entry been reviewed by an analyst
        /// </summary>
        public bool ReviewedByAnalyst { get; set; } = false;

        /// <summary>
        /// Has the entry been reviewed by OSD
        /// </summary>
        public bool ReviewedByOsd { get; set; } = false;

        /// <summary>
        /// The date the record was added to the database
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// The date the record was last updated in the database
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// A list of all the forms associated with this entry
        /// </summary>
        public virtual List<Form> Forms { get; set; } = new List<Form>();

        // A history of all the status tracking for this entry
        public virtual List<EntryStatusTracking> EntryStatusTracking { get; set; } = new List<EntryStatusTracking>();

        public bool CanEdit()
        {
            switch (EntryStatusID)
            {
                case (int)Enumerations.EntryStatus.Started:
                case (int)Enumerations.EntryStatus.Incomplete:
                case (int)Enumerations.EntryStatus.YesIf:
                    return true;

                default:
                    return false;
            }
        }

    }
}