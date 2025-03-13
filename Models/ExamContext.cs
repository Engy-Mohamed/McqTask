using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.AspNetCore.Identity;


using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace McqTask.Models
{

    public class ExamContext : IdentityDbContext<ApplicationUser>
    {
        public ExamContext(DbContextOptions<ExamContext> options) : base(options) { }
        

        public DbSet<Category> Category { get; set; } = default!;
        public DbSet<Student> Students { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<MatchingPair> MatchingPairs { get; set; }
        public DbSet<ResultRecord> ResultRecords { get; set; }
        public DbSet<Group> Groups { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Group>().HasData(new Group { Id = 2, Name = "Default" });
            // Seed Admin Role
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "ExamTaker", NormalizedName = "EXAMTAKER" }
            );

            modelBuilder.Entity<ExamGroup>()
        .HasKey(eg => new { eg.ExamId, eg.GroupId });

            modelBuilder.Entity<ExamGroup>()
                .HasOne(eg => eg.Exam)
                .WithMany(e => e.ExamGroups)
                .HasForeignKey(eg => eg.ExamId);

            modelBuilder.Entity<ExamGroup>()
                .HasOne(eg => eg.Group)
                .WithMany(g => g.ExamGroups)
                .HasForeignKey(eg => eg.GroupId);
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

            modelBuilder.Entity<MatchingPair>()
               .HasOne(q => q.Question)
               .WithMany(e => e.MatchingPairs)
               .HasForeignKey(q => q.QuestionId)
               .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<Option>()
               .HasOne(q => q.Question)
               .WithMany(e => e.Options)
               .HasForeignKey(q => q.QuestionId)
               .OnDelete(DeleteBehavior.Cascade);



            // Configure relationship between Exam and Questions
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Groups
            modelBuilder.Entity<Group>().HasData(
                new Group { Id = 1, Name = "Default" }
           
            );

     

  
        }
    }
}
