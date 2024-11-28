using McqTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace McqTask.Controllers
{
    public class StudentController : Controller
    {
        private readonly ExamContext _context;

        public StudentController(ExamContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();

            return RedirectToAction("TakeExam", new { studentId = student.Id });
        }

        public IActionResult TakeExam(int studentId)
        {
            var questions = _context.Questions.Include(q => q.Options).ToList();
            ViewBag.StudentId = studentId;
            return View(questions);
        }

        [HttpPost]
        public IActionResult SubmitExam(int studentId, Dictionary<int, int> answers)
        {
            int score = 0;

            foreach (var answer in answers)
            {
                var question = _context.Questions.Find(answer.Key);
                if (question.CorrectOptionId == answer.Value)
                    score++;
            }

            var student = _context.Students.Find(studentId);
            student.Score = score;

            _context.SaveChanges();

            return RedirectToAction("Result", new { studentId = studentId });
        }

        public IActionResult Result(int studentId)
        {
            var student = _context.Students.Find(studentId);
            return View(student);
        }
    }

}
