namespace McqTask.Models
{
    public class Option
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string ArabicText { get; set; }
        public int QuestionId { get; set; }
        // Navigation property for the related Question
        public Question? Question { get; set; }

        public bool IsCorrect { get; set; }
    }
}
