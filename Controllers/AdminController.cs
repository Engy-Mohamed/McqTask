using McqTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using McqTask.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public IActionResult AddQuestion(Question question, int examId, QuestionType questionType, List<string> options, List<int> correctOptionIndices)
        {
            // Find the exam by ID
            var exam = _context.Exams.Include(e => e.Questions).FirstOrDefault(e => e.Id == examId);
            if (exam == null)
            {
                return NotFound("Exam not found.");
            }

            // Ensure the question type is stored
            question.Type = questionType;

            // Map options to the question
            question.Options = options.Select((o, index) => new Option
            {
                Text = o,
                IsCorrect = questionType == QuestionType.MultipleResponse
                    ? correctOptionIndices.Contains(index)
                    : correctOptionIndices.Contains(index) && correctOptionIndices.Count == 1
            }).ToList();

            // Add question to the exam
            exam.Questions.Add(question);
            _context.SaveChanges();

            return RedirectToAction("ManageExam", new { id = examId });
        }

  




 
        
   

        [HttpGet]
        public IActionResult Uploadfile()
        {
            ViewData["ExamData"] = new SelectList(_context.Exams, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(FileUpload model)
        {
            if (ModelState.IsValid)
            {
                if (model.UploadedFile != null && model.UploadedFile.Length > 0)
                {
                    ViewBag.ExamData = new SelectList(await _context.Exams.ToListAsync(), "Id", "Name");
                    string filePath = Path.GetTempFileName();
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.UploadedFile.CopyToAsync(stream);
                    }

                    (List<Question> questions, List<int> UnparsedQuestionNumbers) = ExtractQuestions.ExtractQuestionsFromPdf(filePath, model.IsAnswerWithDot);
                    foreach (var question in questions)
                    {
                        question.ExamId = model.ExamId; // Assign ExamId from the model
                    }

                    // Save to database
                    
                    _context.Questions.AddRange(questions);

                    await _context.SaveChangesAsync();
                    var fileupload = new FileUpload();
                    fileupload.UnparsedQuestionNumbers = UnparsedQuestionNumbers;

                    return View("UploadFile", fileupload);
                    // 
                    //iewBag.Message = "File uploaded and questions saved successfully!";
                }
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
