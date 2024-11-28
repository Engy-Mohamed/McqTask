using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace McqTask.Models
{
    public class ExamContext : DbContext
    {
        public ExamContext(DbContextOptions<ExamContext> options) : base(options)
        {
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Exam> Exams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-6IJG1AU\\SQLEXPRESS;Initial Catalog=ExamDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
