namespace McqTask.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public ICollection<Student> Students { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
