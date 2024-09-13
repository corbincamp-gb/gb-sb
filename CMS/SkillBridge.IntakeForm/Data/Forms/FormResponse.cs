using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Forms
{
    /// <summary>
    /// The answer by an organization to a particular question within the form template
    /// </summary>
    [Table("FormResponses")]
    public class FormResponse
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Foreign key to the Forms table, indicates which form the answer belongs to
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
        /// Nullable, this is the textual answer provided by the credentialing body when a question within the form template allows for one, i.e., in the case of a textarea, text input field, or WYSIWYG editor
        /// </summary>
        public string? Answer { get; set; }

        /// <summary>
        /// Is a response required to this question based on the form template in question
        /// </summary>
        public bool IsResponseRequired { get; set; }

        /// <summary>
        /// The date the record was added to the database
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// The date the record was last updated in the database
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// The list of selected answer choices by the credentialing body if the question allows for a selection from a list of answers; this list will be null and/or empty if the question does not contain a list of answers.
        /// </summary>
        public virtual List<FormResponseChoice> FormResponseChoices { get; set; } = new List<FormResponseChoice>();

        /// <summary>
        /// The list of rows of answers for questions that are tables; this list will be null and/or empty if the question is not a table
        /// </summary>
        public virtual List<FormResponseRow> FormResponseRows { get; set; } = new List<FormResponseRow>();

        /// <summary>
        /// The list of supporting documentation for this question, if the question allows for supporting documentation; this list will be null and/or empty if the question does not allow for supporting documentation.
        /// </summary>
        public virtual List<FormResponseFile> FormResponseFiles { get; set; } = new List<FormResponseFile>();

        /// <summary>
        /// Function that determines if the response is complete for a particular credential
        /// </summary>
        /// <returns></returns>
        public bool IsComplete()
        {
            // A section is "complete" if there is an answer, either in the text field or in the list of selected choices or in the list of entered rows
            return !string.IsNullOrWhiteSpace(Answer) || FormResponseChoices.Any() || FormResponseRows.Any();
        }
    }
}
