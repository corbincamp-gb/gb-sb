using static IntakeForm.Models.Enumerations;

namespace IntakeForm.Models.Data.Templates
{
    public class AnswerColumn
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Question type, possible values are in Enumerations.QuestionType
        /// </summary>
        public Enumerations.QuestionType QuestionType { get; set; }

        /// <summary>
        /// Label of the column
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Is answer required
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// The order this column should display in
        /// </summary>
        public int Order { get; set; }
    }
}
