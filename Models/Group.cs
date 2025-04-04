﻿using System.ComponentModel.DataAnnotations.Schema;

namespace McqTask.Models
{
    [Table("Groups")]
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ApplicationUser>? Students { get; set; }

        // Many-to-Many Relationship with Exams
        public ICollection<ExamGroup> ExamGroups { get; set; } = new List<ExamGroup>();
    }
}
