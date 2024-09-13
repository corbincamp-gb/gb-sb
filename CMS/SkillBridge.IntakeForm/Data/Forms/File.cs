using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Forms
{
    /// <summary>
    /// Supporting documentation that is loaded to accompany a form
    /// </summary>
    [Table("Files")]
    public partial class File
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Agency ID from webcoolbox
        /// </summary>
        public int EntryID { get; set; }

        /// <summary>
        /// The mime type of the file, pulled from the HttpPostedFile.ContentType of the file that gets uploaded
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// The name of the file, pulled from the HttpPostedFile.FileName of the file that gets uploaded
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// The content length of the file, pulled from the HttpPostedFile.ContentLength of the file that gets uploaded
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// The date the record was added to the database
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// The date the record was last updated in the database
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        public FileBlob FileBlob { get; set; }
    }


    /// <summary>
    /// Supporting documentation from the  that accompanies forms; this represents the actual array of bytes that is the file
    /// </summary>
    [Table("Files")]
    public partial class FileBlob
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// An array of bytes that represents the binary of the file that gets uploaded
        /// </summary>
        public byte[] Blob { get; set; } = new byte[0];
    }
}

