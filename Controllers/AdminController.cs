using McqTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using McqTask.Helpers;

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
        public IActionResult AddQuestion(Question question, string questionType, List<string> options, List<int> correctOptionIndices)
        {
            // Ensure the question type is stored
            question.Type = questionType;

            // Map options to the question
            question.Options = options.Select((o, index) => new Option
            {
                Text = o,
                IsCorrect = questionType == "Multiple"
                    ? correctOptionIndices.Contains(index)
                    : correctOptionIndices.Contains(index) && correctOptionIndices.Count == 1
            }).ToList();

            // Save the question to the database
            _context.Questions.Add(question);
            _context.SaveChanges();

            return RedirectToAction("AddQuestion");
        }


        [HttpGet]
        public IActionResult Uploadfile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(FileUpload model)
        {
            if (model.UploadedFile != null && model.UploadedFile.Length > 0)
            {
                // Save the file temporarily (Optional)
                string filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.UploadedFile.CopyToAsync(stream);
                }

                List<Question> questions = ExtractQuestions.ExtractQuestionsFromPdf(filePath);

                // Save to database
                _context.Questions.AddRange(questions);
                await _context.SaveChangesAsync();

                ViewBag.Message = "File uploaded and questions saved successfully!";
            }
            else
            {
                ViewBag.Message = "Please upload a valid file.";
            }

            return RedirectToAction("Uploadfile");
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
