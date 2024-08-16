using IntakeForm.Models.Data.Forms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.View.Forms
{
    /// <summary>
    /// Model that holds the information necessary to display the part and section portion of the progress bar
    /// </summary>
    public class ProgressBarState
    {
        /// <summary>
        /// Primary key for the FormResponses table
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Foreign key to the Form table
        /// </summary>
        public int FormID { get; set; }

        /// <summary>
        /// Unenforced foreign key; indicates which part in the form template the question lives in
        /// </summary>
        public int PartID { get; set; }

        /// <summary>
        /// Unenforced foreign key; indicates which section in the form template the question lives in
        /// </summary>
        public int SectionID { get; set; }

        /// <summary>
        /// Unenforced foreign key; indicates which question in the form template the answer belongs to
        /// </summary>
        public int QuestionID { get; set; }

        /// <summary>
        /// Indicates if a response is required for this question
        /// </summary>
        public bool IsResponseRequired { get; set; }

        /// <summary>
        /// Indicates if a response has been provided
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// List of selected answer choice IDs, if any
        /// </summary>
        [NotMapped]
        public List<FormResponseChoice> FormResponseChoices { get; set; }  = new List<FormResponseChoice>();
    }

}
