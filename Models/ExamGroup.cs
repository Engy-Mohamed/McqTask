namespace McqTask.Models
{
    public class ExamGroup
    {
        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }
    }

}
