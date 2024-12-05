namespace McqTask.Models
{
    public class MatchingPair
    {
        public int Id { get; set; }
        // Foreign key
        public int QuestionId { get; set; }

        // Navigation property for the related Question
        public Question Question { get; set; }
        public string LeftSideText { get; set; }
        public string RightSideText { get; set; }
    }
}
