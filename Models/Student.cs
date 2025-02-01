using System.ComponentModel.DataAnnotations.Schema;
using ServiceStack.DataAnnotations;
namespace McqTask.Models
{
    [Table("Students")]
    public class Student
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Unique]
        [Required]
        public string? Email { get; set; }
        [Unique]
        [Required]
        public string? PhoneNumber { get; set; }
        public int Score { get; set; }
        public int GroupId { get; set; } // Foreign key to associate the syudent with a group
        public Group? Group { get; set; } // Navigation property for the related Group

        public ICollection<ResultRecord>? ResultRecords { get; set; }
    }
}
