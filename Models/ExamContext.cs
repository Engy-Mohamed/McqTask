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
        public DbSet<MatchingPair> MatchingPairs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ResultRecord> ResultRecords { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            // we don't need them the connection string is in appsettings.json and loaded in program.cs
            //  optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            //base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationship between Group and Students
            modelBuilder.Entity<ResultRecord>()
                .HasOne(q => q.Student)
                .WithMany(e => e.ResultRecords)
                .HasForeignKey(q => q.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ResultRecord>()
            .HasOne(q => q.Exam)
            .WithMany(e => e.ResultRecords)
            .HasForeignKey(q => q.ExamId)
            .OnDelete(DeleteBehavior.Cascade);


            // Configure relationship between category and Exams
            modelBuilder.Entity<Exam>()
                .HasOne(q => q.Category)
                .WithMany(e => e.Exams)
                .HasForeignKey(q => q.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship between Group and Students
            modelBuilder.Entity<Student>()
                .HasOne(q => q.Group)
                .WithMany(e => e.Students)
                .HasForeignKey(q => q.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship between Question and Options
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade); ;


            // Configure relationship between Question and MatchingPairs
            modelBuilder.Entity<Question>()
                .HasMany(q => q.MatchingPairs)
                .WithOne(mp => mp.Question)
                .HasForeignKey(mp => mp.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship between Exam and Questions
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
