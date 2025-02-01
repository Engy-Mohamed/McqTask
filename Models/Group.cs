using System.ComponentModel.DataAnnotations.Schema;

namespace McqTask.Models
{
    [Table("Groups")]
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Student>? Students { get; set; }
    }
}
