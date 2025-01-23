namespace McqTask.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; } // Foreign key to associate the exam with a category
        public Category Category { get; set; } // Navigation property for the related Category
       
        public ICollection<Question> Questions { get; set; }
        public ICollection<ResultRecord> ResultRecords { get; set; }
    }
}
