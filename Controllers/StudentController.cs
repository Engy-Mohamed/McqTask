using McqTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
            var allQuestions = _context.Questions.Include(q => q.Options).ToList();
            // Get 10 random questions
            var randomQuestions = allQuestions.OrderBy(q => Guid.NewGuid()).Take(10).ToList();

            // Store questions in session
            HttpContext.Session.SetString("ExamQuestions", JsonSerializer.Serialize(randomQuestions.Select(q => q.Id)));
            HttpContext.Session.SetInt32("CurrentQuestion", 0);

            ViewBag.StudentId = studentId;
            ViewBag.TotalQuestions = 10;
            ViewBag.CurrentQuestion = 1;

            return View(randomQuestions.First());
        }
        [HttpPost]
        public IActionResult NavigateQuestion(int studentId, int direction, Dictionary<int, int> answers)
        {
            var questionIds = JsonSerializer.Deserialize<List<int>>(HttpContext.Session.GetString("ExamQuestions"));
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestion").Value;

            // Save current answer
            if (answers != null && answers.Any())
            {
                var answersDict = HttpContext.Session.GetString("ExamAnswers") != null
                    ? JsonSerializer.Deserialize<Dictionary<int, int>>(HttpContext.Session.GetString("ExamAnswers"))
                    : new Dictionary<int, int>();

                foreach (var answer in answers)
                {
                    answersDict[answer.Key] = answer.Value;
                }
                HttpContext.Session.SetString("ExamAnswers", JsonSerializer.Serialize(answersDict));
            }

            // Update current question index
            currentIndex += direction;
            HttpContext.Session.SetInt32("CurrentQuestion", currentIndex);

            if (currentIndex >= questionIds.Count)
            {
                return RedirectToAction("SubmitExam", new { studentId = studentId });
            }

            var question = _context.Questions
                .Include(q => q.Options)
                .FirstOrDefault(q => q.Id == questionIds[currentIndex]);

            ViewBag.StudentId = studentId;
            ViewBag.TotalQuestions = questionIds.Count;
            ViewBag.CurrentQuestion = currentIndex + 1;

            return View("TakeExam", question);
        }

        [HttpPost]
        public IActionResult SubmitExam(int studentId)
        {
            var answers = JsonSerializer.Deserialize<Dictionary<int, int>>(
                HttpContext.Session.GetString("ExamAnswers") ?? "{}"
            );

            int score = 0;
            foreach (var answer in answers)
            {
                var question = _context.Questions.Find(answer.Key);
                if (question != null && question.CorrectOptionId == answer.Value)
                    score++;
            }

            var student = _context.Students.Find(studentId);
            if (student != null)
            {
                student.Score = score;
                _context.SaveChanges();
            }

            // Clear session
            HttpContext.Session.Clear();

            return RedirectToAction("Result", new { studentId = studentId });
        }

        public IActionResult Result(int studentId)
        {
            var student = _context.Students.Find(studentId);
            return View(student);
        }
    }

}
