using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeForm.Models.Data.Templates
{
    /// <summary>
    /// This object represents a serialized version of the form template that was used by the Credentialing Body to submit their credentials. In the initial development, we are not creating a means to allow the form template to be modified; however, we expect that to be an iteration down the road and this format is in preparation of that iteration.
    /// </summary>
    [Table("FormTemplates")]
    public class FormTemplate
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The template type, possible values appear in Enumerations.TemplateType
        /// </summary>
        public byte TemplateTypeID { get; set; }

        /// <summary>
        /// The date the record was added to the database
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// The person who added the record to the database
        /// </summary>
        public string AddedBy { get; set; } = string.Empty;

        /// <summary>
        /// The date the record was last updated in the database
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// The person who last updated the record in the database
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// The serialized version of the form. This may be deserialized into the DSA.Models.Templates.DeserializedFormTemplate format using JsonConvert.DeserializeObject()
        /// </summary>
        public string SerializedFormTemplate { get; set; } = string.Empty;

        /// <summary>
        /// The date when this form template was retired
        /// </summary>
        public DateTime RetiredDate { get; set; }

    }
}
