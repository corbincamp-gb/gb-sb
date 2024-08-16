using IntakeForm.Models.Data.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntakeForm.Models.Data.Templates
{
    /// <summary>
    /// The deserialized version of the form that is stored in the database
    /// </summary>
    public class DeserializedFormTemplate
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Name of template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The version number
        /// </summary>
        public decimal Version { get; set; }

        /// <summary>
        /// The array of parts associated with this form
        /// </summary>
        public List<Part> Parts { get; set; }

        public DeserializedFormTemplate()
        {
            ID = 0;
            Version = 0;
            Parts = new List<Part>();
        }

        public bool IsComplete(List<FormResponse> formResponses)
        {
            return Parts.All(o => o.IsComplete(formResponses));
        }
    }
}
