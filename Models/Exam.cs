﻿using System.ComponentModel.DataAnnotations.Schema;
using ServiceStack.DataAnnotations;

namespace McqTask.Models
{
    [Table("Exams")]
    public class Exam
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; } // Foreign key to associate the exam with a category
        public Category? Category { get; set; } // Navigation property for the related Category
        public int ExamTime { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string ExamCode { get; set; } = Guid.NewGuid().ToString();

        // Many-to-Many Relationship with Groups
        public ICollection<ExamGroup> ExamGroups { get; set; } = new List<ExamGroup>();
        public ICollection<Question>? Questions { get; set; }
        public ICollection<ResultRecord>? ResultRecords { get; set; }
    }
}
