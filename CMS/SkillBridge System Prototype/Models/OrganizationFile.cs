using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge_System_Prototype.Models
{
    [Table("OrganizationFiles")]
    public partial class OrganizationFile
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Organization")]
        public int OrganizationId { get; set; }

        public SB_Organization Organization { get; set; }

        public string FileType { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public string CreateBy { get; set; }

        public bool IsActive { get; set; } = true;
        public FileBlob FileBlob { get; set; }

        public decimal GetContentLengthInKb()
        {
            return (decimal)ContentLength / 1024;
        }
    }


    /// <summary>
    /// Supporting documentation from the  that accompanies forms; this represents the actual array of bytes that is the file
    /// </summary>
    [Table("OrganizationFiles")]
    public partial class FileBlob
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// An array of bytes that represents the binary of the file that gets uploaded
        /// </summary>
        public byte[] Blob { get; set; } = new byte[0];
    }
}

