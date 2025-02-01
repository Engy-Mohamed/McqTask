namespace McqTask.Models
{
    // Enum for Question Types
    public enum QuestionType
    {
        MultipleChoice,
        MultipleResponse,
        Matching
    }

    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; } 
        public int ExamId { get; set; } // Foreign key to associate the question with an exam
        public Exam? Exam { get; set; } // Navigation property for the related Exam
        public List<Option>? Options { get; set; } = new List<Option>();
        public List<MatchingPair>? MatchingPairs { get; set; } = new List<MatchingPair>();
        public Question(int examId, string text, QuestionType type)
        {
            ExamId = examId;
            Text = text;
            Type = type;
       
        }
        public Question()
        {
         

        }
    }
}
