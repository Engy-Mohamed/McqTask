using System.ComponentModel.DataAnnotations.Schema;

namespace McqTask.Models
{
    [Table("categories")]
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Exam>? Exams { get; set; }
    }
}
