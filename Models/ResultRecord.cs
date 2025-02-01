namespace McqTask.Models
{
    public class ResultRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; } // Foreign key to associate the student with a group
        public int Score { get; set; }
        public int ExamId { get; set; } // Foreign key to associate the exam with a group
        public Exam Exam { get; set; } // Navigation property for the related Exam
        public Student Student { get; set; } // Navigation property for the related Student
    }
}
