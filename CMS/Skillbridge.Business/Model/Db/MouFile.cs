using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge.Business.Model.Db
{
    [Table("MouFiles")]
    public partial class MouFile
    {
        private static string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };

        [Key]
        public int Id { get; set; }

        [ForeignKey("Mou")]
        public int MouId { get; set; }

        public MouModel Mou { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public string CreateBy { get; set; }

        public MouFileBlob FileBlob { get; set; }

        public bool IsActive { get; set; }

        public string GetContentLengthForDisplay()
        {
            int counter = 0;
            decimal number = ContentLength;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }
    }


    /// <summary>
    /// The actual array of bytes that is the file
    /// </summary>
    [Table("MouFiles")]
    public partial class MouFileBlob
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

