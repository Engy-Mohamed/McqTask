using McqTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace McqTask.Controllers
{
    public class AdminController : Controller
    {
        private readonly ExamContext _context;

        public AdminController(ExamContext context)
        {
            _context = context;
        }

        public IActionResult AddQuestion()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddQuestion(Question question, List<string> options, int correctOptionIndex)
        {
            question.Options = options.Select(o => new Option { Text = o }).ToList();
            question.CorrectOptionId = correctOptionIndex;
            //question.CorrectOptionId = question.Options.ToList()[correctOptionIndex].Id;

            _context.Questions.Add(question);
            _context.SaveChanges();

            return RedirectToAction("AddQuestion");
        }

        public IActionResult ViewResults()
        {
            var students = _context.Students.ToList();
            return View(students);
        }

        public async Task<IActionResult> StudentResults()
        {
            // Retrieve all students and their scores
            var students = await _context.Students
                .ToListAsync();

            return View(students);
        }

        //todo: show all questions with their answers and the correct answer.
        public async Task<IActionResult> Questions()
        {
            // Retrieve all Questions
            var questions = await _context.Questions
                .ToListAsync();

            return View(questions);
        }
    }

}
