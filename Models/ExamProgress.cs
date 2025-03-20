using System.ComponentModel.DataAnnotations;

namespace McqTask.Models
{
    public class ExamProgress
    {
        [Key]
        public int Id { get; set; }

        public string StudentId { get; set; } // Foreign key to Student
        public int ExamId { get; set; }    // Foreign key to Exam
        public int CurrentQuestionIndex { get; set; }

        public string AnswersJson { get; set; } // Store answers in JSON format
        public string MatchingAnswersJson { get; set; }
        public string FlaggedQuestionsJson { get; set; }

        public DateTime ExamStartTime { get; set; } // Store start time
        public int TimeLeftInSeconds { get; set; } // Remaining time in seconds

        public string ExamQuestionsJson { get; set; } = "[]";

        public bool IsCompleted { get; set; } = false; // To mark if the exam is finished
    }

}
