namespace McqTask.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Type { get; set; } // e.g., MultipleChoice, MultipleResponse, Matching
        public List<Option> Options { get; set; }
        // Navigation property for matching questions
        public List<MatchingPair> MatchingPairs { get; set; }
    }
}
