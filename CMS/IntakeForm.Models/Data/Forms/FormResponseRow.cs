using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Forms
{
    /// <summary>
    /// The selected choice (or choices) by the organizations from a list of choices that are available to a particular question within the form template
    /// </summary>
    [Table("FormResponseRows")]
    public class FormResponseRow
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Foreign key to the FormResponses table, indicates which response the row belongs to
        /// </summary>
        public int FormResponseID { get; set; }

        /// <summary>
        /// Indicates which row the response is for using a 1-based index
        /// </summary>
        public int RowID { get; set; }

        /// <summary>
        /// Unenforced foreign key; indicates which answer column the response is for
        /// </summary>
        public int ColumnID { get; set; }

        /// <summary>
        /// Answer for this row and column
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// The date the record was added to the database
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// The date the record was last modified
        /// </summary>
        public DateTime UpdatedDate { get; set; }
    }
}
