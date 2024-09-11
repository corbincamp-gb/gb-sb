using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Forms
{
    /// <summary>
    /// The supporting documentation for a particular question
    /// </summary>
    [Table("FormResponseFiles")]
    public class FormResponseFile
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Foreign key to the FormResponses table, indicates which question the selection belongs to
        /// </summary>
        public int FormResponseID { get; set; }

        /// <summary>
        /// Foreign key to the Files table specifying the relevant file from the 
        /// </summary>
        public int FileID { get; set; }

        /// <summary>
        /// The date the record was added to the database
        /// </summary>
        public DateTime AddedDate { get; set; }

    }
}
