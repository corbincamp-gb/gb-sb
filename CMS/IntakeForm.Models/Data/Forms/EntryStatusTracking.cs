using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Forms
{
    /// <summary>
    /// The tracking record for a change to the entry status of a particular submission
    /// </summary>
    [Table("EntryStatusTracking")]
    public class EntryStatusTracking
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Entry ID
        /// </summary>
        public int EntryID { get; set; }

        /// <summary>
        /// The entry object associated with this tracking record
        /// </summary>
        public virtual Entry Entry { get; set; }

        /// <summary>
        /// The role of the person who changed the status
        /// </summary>
        public string Role{ get; set; }

        /// <summary>
        /// The old entry status ID
        /// </summary>
        public int OldEntryStatusID { get; set; }

        /// <summary>
        /// The new entry status ID
        /// </summary>
        public int NewEntryStatusID { get; set; }

        /// <summary>
        /// Notes about the entry status change
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// The date the record was added to the database
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// The name of the person who added the record
        /// </summary>
        public string AddedBy { get; set; }
    }
}