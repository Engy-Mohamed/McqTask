namespace McqTask.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ICollection<Option> Options { get; set; }
        public ICollection<int> CorrectOptionIds { get; set; }
    }
}
