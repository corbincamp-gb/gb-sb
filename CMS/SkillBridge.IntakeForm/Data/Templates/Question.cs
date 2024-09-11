using IntakeForm.Models.Data.Forms;
using static IntakeForm.Models.Enumerations;

namespace IntakeForm.Models.Data.Templates
{
    /// <summary>
    /// A question being asked of the organization
    /// </summary>
    public class Question
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Question type, possible values are in Enumerations.QuestionType
        /// </summary>
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// The order this question should appear when listed among all the questions in the section
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Question label, often a number indicating which question this is
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The full text of the question
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// A description of the question, often additional information provided to help the question be answered appropriately
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// An indicator of whether or not an answer is required to this question
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Indicates the max number of responses; used only when multiselect/checkboxlist/checkboxlist2columns
        /// </summary>
        public int MaxResponse { get; set; }

        /// <summary>
        /// A list of child questions that need to be answered 
        /// </summary>
        public List<Question> ChildQuestions { get; set; } = new List<Question>();

        /// <summary>
        /// An unenforced FK to the list of answer choice IDs that prompt this question to display
        /// </summary>
        public List<int> ChildQuestionsAppearUsingAnswerChoiceIDs { get; set; } = new List<int>();

        /// <summary>
        /// An unenforced FK that indicates which answer choice ID(s) the child question belongs to, if any
        /// </summary>
        public List<int> ChildQuestionOfAnswerChoiceIDs { get; set; } = new List<int>();

        /// <summary>
        /// A list of answer choices that appear when the question allows for the display of options (used for select, multiselect, radio button, checkbox)
        /// </summary>
        public List<AnswerChoice> AnswerChoices { get; set; } = new List<AnswerChoice>();

        public List<AnswerColumn> AnswerColumns { get; set; }


        public bool IsComplete(List<FormResponse> formResponses)
        {
            // If the question isn't required, it's complete regardless of anything else
            if (!Required) return true;

            // Get responses for this question
            var responses = formResponses.Where(o => o.QuestionID == ID).ToList();

            // If the question is required, and there aren't any responses, it's not complete
            if (!responses.Any()) return false;

            switch (QuestionType) {

                case QuestionType.RadioButtonList:
                case QuestionType.CheckBoxList:
                case QuestionType.CheckBoxList2Columns:
                case QuestionType.Select:
                case QuestionType.Multiselect:

                    // If this question requires choices and there aren't any, this isn't complete
                    if (!responses.SelectMany(o => o.FormResponseChoices).Any()) return false;
                    break;

                case QuestionType.Table:

                    // If this question requires table answers and there aren't any, this isn't complete
                    if (!responses.SelectMany(o => o.FormResponseRows).Any()) return false;
                    break;



                case QuestionType.FileUpload:
                case QuestionType.MultiFileUpload:

                    // If this question requires files and there aren't any, this isn't complete
                    if (!responses.SelectMany(o => o.FormResponseFiles).Any()) return false;
                    break;

                default:

                    // If this question doesn't have an answer provided, this isn't complete
                    if (responses.Select(o => o.Answer).Any(o => String.IsNullOrWhiteSpace(o))) return false;
                    break;
            }

            // Loop through any child questions
            if (ChildQuestions.Any(o => o.Required) && ChildQuestionsAppearUsingAnswerChoiceIDs.Any())
            {
                // If the child questions appear because of a choice ID that has been selected, loop through the appropriate child questions
                if (responses.SelectMany(o => o.FormResponseChoices).Any(o => ChildQuestionsAppearUsingAnswerChoiceIDs.Contains(o.AnswerChoiceID)))
                {
                    // Loop through each required child question
                    foreach (var child in ChildQuestions.Where(o => o.Required).ToList())
                    {
                        // If that child question requires specific parents and the child question is appropriate because the response warrants a particular set of questions, check if those children are complete
                        if (child.ChildQuestionsAppearUsingAnswerChoiceIDs.Any() && child.ChildQuestionsAppearUsingAnswerChoiceIDs.Any(o => formResponses.SelectMany(r => r.FormResponseChoices).Select(r => r.AnswerChoiceID).Contains(o)))
                        {
                            return child.IsComplete(formResponses);
                        }

                        // If the child questions aren't answer specific, check for completeness of the children
                        if (!child.ChildQuestionOfAnswerChoiceIDs.Any())
                        {
                            return child.IsComplete(formResponses);
                        }
                    }
                }

                // If the response does not have the choice ID that prompts the child questions, it's valid
                return true;
            }

            return true;
        }
    }
}
