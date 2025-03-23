using McqTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using McqTask.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceStack;

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
                    
                    //Get correct file extension
                    string extension = Path.GetExtension(model.UploadedFile.FileName).ToLower();
                    string tempDirectory = Path.GetTempPath(); // Get system temp directory

                    // Ensure file is saved with the correct extension
                    string filePath = Path.Combine(tempDirectory, Guid.NewGuid().ToString() + extension);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.UploadedFile.CopyToAsync(stream);
                    }

                    List<Question> questions = new List<Question>();
                    List<int> unparsedQuestionNumbers = new List<int>();

                    if (model.FileType == "PDF")
                    {
                        (questions, unparsedQuestionNumbers) = ExtractQuestions.ExtractQuestionsFromPdf(filePath, model.IsAnswerWithDot);
                    }
                    else if (model.FileType == "Excel")
                    {
                        (questions, unparsedQuestionNumbers) = ExtractQuestions.ExtractQuestionsFromExcel(filePath);
                    }
                    else
                    {
                        ViewBag.Message = "Unsupported file type.";
                        return RedirectToAction("Uploadfile");
                    }
                    foreach (var question in questions)
                    {
                        question.ExamId = model.ExamId; // Assign ExamId from the model
                    }

                    // Save to database
                    
                    _context.Questions.AddRange(questions);

                    await _context.SaveChangesAsync();
                    var fileupload = new FileUpload();
                    fileupload.UnparsedQuestionNumbers = unparsedQuestionNumbers;

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
