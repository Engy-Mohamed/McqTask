namespace McqTask.Models
{
    public class QuestionViewModel
    {
        public int Id { get; set; }

        public int ExamId { get; set; } // Required because every question belongs to an exam

        public string Text { get; set; } // Question Text

        public QuestionType Type { get; set; } // Enum for Question Type

        public List<Option>? Options { get; set; } = new List<Option>(); // List of options for MCQs

        public List<MatchingPair>? MatchingPairs { get; set; } = new List<MatchingPair>(); // Matching pairs
    }

}

