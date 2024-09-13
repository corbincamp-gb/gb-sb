using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Forms
{
    /// <summary>
    /// The selected choice (or choices) by the organizations from a list of choices that are available to a particular question within the form template
    /// </summary>
    [Table("FormResponseChoices")]
    public class FormResponseChoice
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Foreign key to the FormResponses table, indicates which response the answer choice belongs to
        /// </summary>
        public int FormResponseID { get; set; }

        /// <summary>
        /// Unenforced foreign key; indicates which answer choice from the form template was selected
        /// </summary>
        public int AnswerChoiceID { get; set; }

        /// <summary>
        /// The date the record was added to the database
        /// </summary>
        public DateTime AddedDate { get; set; }

    }
}
