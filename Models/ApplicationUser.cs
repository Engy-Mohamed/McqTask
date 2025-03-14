
using ServiceStack.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace McqTask.Models
{
    public class ApplicationUser : IdentityUser
         
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [Unique]
        public override string Email { get; set; }

        [Required]
        [Unique]
        public override string PhoneNumber { get; set; }

  

        public int? GroupId { get; set; } // Nullable because Admins don’t have Groups
        public virtual Group Group { get; set; } // Navigation property

        public virtual ICollection<ResultRecord> ResultRecords { get; set; } = new List<ResultRecord>(); // ✅ Initialize collection
    }
}
