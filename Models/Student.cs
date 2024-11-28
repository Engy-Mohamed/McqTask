using ServiceStack.DataAnnotations;
namespace McqTask.Models
{
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
    }
}
