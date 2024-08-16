using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Forms
{
    /// <summary>
    /// States
    /// </summary>
    [Table("States")]
    public partial class State
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 2 digit state code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Full name of the state
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The code plus the name of the state
        /// </summary>
        public string Label { get; set; } = string.Empty;

    }
}

