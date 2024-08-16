using static IntakeForm.Models.Enumerations;

namespace IntakeForm.Models.Data.Templates
{
    public class AnswerChoice
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Label of the answer
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Answer type, possible values are in Enumerations.AnswerType
        /// </summary>
        public AnswerType AnswerType { get; set; }

        /// <summary>
        /// The order this answer choice should display in
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The text of the answer
        /// </summary>
        public string AnswerText { get; set; } = string.Empty;

        /// <summary>
        /// Any additional descriptive information
        /// </summary>
        public string Description { get; set; } = string.Empty;

    }
}
