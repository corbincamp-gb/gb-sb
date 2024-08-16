using IntakeForm.Models.Data.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Forms
{
    /// <summary>
    /// The form that an organization is filling out (or has already filled out)
    /// </summary>
    [Table("Forms")]
    public class Form
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Foreign key to the Entries table, which indicates which entry this form lives under
        /// </summary>
        public int EntryID { get; set; }

        /// <summary>
        /// Entry object
        /// </summary>
        public virtual Entry Entry { get; set; }

        /// <summary>
        /// Foreign key to the FormTemplates table, which indicates which template of the form contains the questions being asked
        /// </summary>
        public int FormTemplateID { get; set; }

        /// <summary>
        /// Form template associated with the form in question
        /// </summary>
        public FormTemplate? FormTemplate { get; set; }

        /// <summary>
        /// The order the form should display
        /// </summary>
        public int FormOrder { get; set; }

        /// <summary>
        /// The date the record was added to the database
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// The date the record was last updated in the database
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// A list of all the responses associated with this form
        /// </summary>
        public virtual List<FormResponse> FormResponses { get; set; } = new List<FormResponse>();

    }
}
