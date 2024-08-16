using IntakeForm.Models.Data.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntakeForm.Models.Data.Templates
{
    /// <summary>
    /// A grouping of sections, kind of like the main heading of a page
    /// </summary>
    public class Part
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Label for the part, often a number or letter
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The full text heading of the part
        /// </summary>
        public string PartText { get; set; } = string.Empty;

        /// <summary>
        /// Part type, possible values are in Enumerations.PartType
        /// </summary>
        public Enumerations.PartType PartType { get; set; }

        /// <summary>
        /// The order the part should appear when displaying the list of parts
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// A description of the part
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// A list of sections associated with the part
        /// </summary>
        public List<Section> Sections { get; set; } = new List<Section>();

        public bool IsComplete(List<FormResponse> formResponses)
        {
            return Sections.All(o => o.IsComplete(formResponses.Where(r => r.PartID == ID).ToList()));
        }
    }
}
