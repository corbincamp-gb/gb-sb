namespace IntakeForm.Models.View.Forms
{
    /// <summary>
    /// Model that holds the information necessary to display the information in the _ViewQuestion partial
    /// </summary>
    public class ViewQuestionModel
    {
        /// <summary>
        /// Question
        /// </summary>
        public IntakeForm.Models.Data.Templates.Question Question { get; set; }

        /// <summary>
        /// Responses
        /// </summary>
        public List<IntakeForm.Models.Data.Forms.FormResponse> Responses { get; set; }
    }
}
