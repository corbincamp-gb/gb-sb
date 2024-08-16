using IntakeForm.Models.Data.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace IntakeForm.Models.Data.Templates
{
    /// <summary>
    /// A landing page for the form; a group of questions live under a section, each of which appear on the page
    /// </summary>
    public class Section
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Label for the section, often a number or letter
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The full text heading of the section
        /// </summary>
        public string SectionText { get; set; } = string.Empty;

        /// <summary>
        /// The order the section should appear when listed among all the sections within the part
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// A description of the section
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// A list of questions associated with the section
        /// </summary>
        public List<Question> Questions { get; set; } = new List<Question>();


        public bool IsComplete(List<FormResponse> formResponses)
        {
            return Questions.All(o => o.IsComplete(formResponses.Where(r => r.SectionID == ID).ToList()));
        }
    }
}
