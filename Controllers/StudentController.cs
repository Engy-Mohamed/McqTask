using McqTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace McqTask.Controllers
{
    public class StudentController : Controller
    {
        //private readonly ExamContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDbContextFactory<ExamContext> _dbContextFactory;

        public StudentController( UserManager<ApplicationUser> userManager, IDbContextFactory<ExamContext> dbContextFactory)
        {
          //  _context = context;
            _userManager = userManager;
            _dbContextFactory = dbContextFactory;
        }

        [HttpPost]
        public IActionResult SaveProgress()
        {
            using (var _context = _dbContextFactory.CreateDbContext())
            {
                var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var examProgress = _context.ExamProgress.FirstOrDefault(p => p.StudentId == studentId && !p.IsCompleted);

                if (examProgress == null) return BadRequest("No active exam found.");

                // Retrieve current session data
                var answersJson = HttpContext.Session.GetString("ExamAnswers");
                var matchingAnswersJson = HttpContext.Session.GetString("ExamMatchingAnswers");
                var flaggedQuestionsJson = HttpContext.Session.GetString("FlaggedQuestions");

                int currentQuestionIndex = HttpContext.Session.GetInt32("CurrentQuestion") ?? 0;
                int timeLeft = (int)(examProgress.ExamStartTime.AddMinutes(examProgress.TimeLeftInSeconds / 60) - DateTime.UtcNow).TotalSeconds;

                // Save to database only every 5 minutes
                examProgress.CurrentQuestionIndex = currentQuestionIndex;
                examProgress.AnswersJson = answersJson;
                examProgress.MatchingAnswersJson = matchingAnswersJson;
                examProgress.FlaggedQuestionsJson = flaggedQuestionsJson;
                examProgress.TimeLeftInSeconds = timeLeft;

                _context.SaveChanges();
            }
            return Ok();
        }


        [Authorize(Roles = "ExamTaker")]
        public IActionResult TakeExam([FromQuery] Guid code)
        {
            using (var _context = _dbContextFactory.CreateDbContext())
            {

                var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (studentId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var exam = _context.Exams
                    .Include(e => e.Questions)
                    .ThenInclude(q => q.Options)
                    .Include(e => e.Questions)
                    .ThenInclude(q => q.MatchingPairs)
                    .FirstOrDefault(e => e.ExamCode == code.ToString());


                if (exam == null || !exam.IsActive)
                {
                    return NotFound("Exam not found or not active.");
                }


                var progress = _context.ExamProgress.FirstOrDefault(p => p.StudentId == studentId && p.ExamId == exam.Id);
                int numberOfQuestions = Math.Min(180, exam.Questions.Count);
                if (progress != null && !progress.IsCompleted)
                {
                    

                    HttpContext.Session.SetString("ExamQuestions", progress.ExamQuestionsJson);
                    HttpContext.Session.SetInt32("CurrentQuestion", progress.CurrentQuestionIndex);
                    HttpContext.Session.SetString("ExamAnswers", progress.AnswersJson);
                    HttpContext.Session.SetString("ExamStartTime", progress.ExamStartTime.ToString("o"));
                    HttpContext.Session.SetString("ExamMatchingAnswers", progress.MatchingAnswersJson);
                    HttpContext.Session.SetString("FlaggedQuestions", progress.FlaggedQuestionsJson);
                    HttpContext.Session.SetInt32("ExamDuration", progress.TimeLeftInSeconds / 60);

                    // Save exam code and exam name in session
                    HttpContext.Session.SetString("ExamCode", exam.ExamCode);
                    HttpContext.Session.SetString("ExamName", exam.Name);
                    HttpContext.Session.SetInt32("ExamId", exam.Id);
                    // ✅ Restore progress from the database
                    if (progress.TimeLeftInSeconds <= 0)
                        return SubmitExam(studentId).GetAwaiter().GetResult();

                    ViewBag.ExamEndTime = progress.ExamStartTime.AddSeconds(progress.TimeLeftInSeconds).ToString("o");
                    ViewBag.CurrentQuestion = progress.CurrentQuestionIndex + 1;
                    ViewBag.FlaggedQuestions = JsonSerializer.Deserialize<HashSet<int>>(progress.FlaggedQuestionsJson);
                    ViewBag.SelectedAnswers = JsonSerializer.Deserialize<Dictionary<int, List<int>>>(progress.AnswersJson);
                    ViewBag.SelectedMatchingAnswers = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, int>>>(progress.MatchingAnswersJson);


                    ViewBag.StudentId = studentId;
                    ViewBag.TotalQuestions = numberOfQuestions;
                    ViewBag.CurrentQuestion = 1;
                    ViewBag.ExamName = exam.Name;

                    var questionIds = JsonSerializer.Deserialize<List<int>>(progress.ExamQuestionsJson);
                    var currentQuestion = _context.Questions
                        .Include(q => q.Options)
                        .Include(q => q.MatchingPairs)
                        .FirstOrDefault(q => q.Id == questionIds[progress.CurrentQuestionIndex]);

                    return View(currentQuestion);
                }
                var randomQuestions = exam.Questions.OrderBy(q => Guid.NewGuid()).Take(150).ToList();
                var questionIdsList = randomQuestions.Select(q => q.Id).ToList();
                var examQuestionsJson = JsonSerializer.Serialize(questionIdsList);

                var newProgress = new ExamProgress
                {
                    StudentId = studentId,
                    ExamId = exam.Id,
                    CurrentQuestionIndex = 0,
                    ExamStartTime = DateTime.UtcNow,
                    TimeLeftInSeconds = exam.ExamTime * 60,
                    AnswersJson = JsonSerializer.Serialize(new Dictionary<int, List<int>>()),
                    MatchingAnswersJson = JsonSerializer.Serialize(new Dictionary<int, Dictionary<int, int>>()),
                    FlaggedQuestionsJson = JsonSerializer.Serialize(new HashSet<int>()),
                    ExamQuestionsJson = examQuestionsJson // 🟢 Fix: Store question IDs
                };

                _context.ExamProgress.Add(newProgress);
                _context.SaveChanges();


                // ✅ Store in session as well for better performance
                HttpContext.Session.SetString("ExamQuestions", examQuestionsJson);
                HttpContext.Session.SetInt32("CurrentQuestion", 0);
                HttpContext.Session.SetString("ExamStartTime", DateTime.UtcNow.ToString("o"));
                HttpContext.Session.SetInt32("ExamDuration", exam.ExamTime);
                HttpContext.Session.SetString("FlaggedQuestions", JsonSerializer.Serialize(new HashSet<int>()));
                HttpContext.Session.SetString("ExamAnswers", JsonSerializer.Serialize(new Dictionary<int, List<int>>()));
                HttpContext.Session.SetString("ExamMatchingAnswers", JsonSerializer.Serialize(new Dictionary<int, Dictionary<int, int>>()));


                // Save exam code and exam name in session
                HttpContext.Session.SetString("ExamCode", exam.ExamCode);
                HttpContext.Session.SetString("ExamName", exam.Name);
                HttpContext.Session.SetInt32("ExamId", exam.Id);

                DateTime parsedStartTime = DateTime.Parse(HttpContext.Session.GetString("ExamStartTime")).ToUniversalTime();
                int examDuration = HttpContext.Session.GetInt32("ExamDuration").Value;
                DateTime examEndTime = parsedStartTime.AddMinutes(examDuration);

                HttpContext.Session.SetString("ExamEndTime", examEndTime.ToUniversalTime().ToString("o"));

                ViewBag.ExamEndTime = examEndTime.ToString("o");
                ViewBag.StudentId = studentId;
                ViewBag.TotalQuestions = numberOfQuestions;
                ViewBag.CurrentQuestion = 1;
                ViewBag.ExamName = exam.Name;

                return View(randomQuestions.First());
            }

        }

        [HttpPost]
        //public IActionResult NavigateQuestion(int? studentId, int direction, Dictionary<int, List<int>>? answers, Dictionary<int, Dictionary<int, int>>? matchingAnswers)
        [HttpPost]
        public IActionResult NavigateQuestion(int studentId, int direction, int question_no, [FromForm] IFormCollection form)
        {
            using (var _context = _dbContextFactory.CreateDbContext())
            {
                string studentIdString = Request.Form["studentId"];
                // Check if the exam is being submitted due to time running out
                if (Request.Form["timeUp"] == "true")
                {
                    return SubmitExam(studentIdString).GetAwaiter().GetResult(); // Force execution synchronously
                }
                Dictionary<int, Dictionary<int, int>> matchingAnswers = new Dictionary<int, Dictionary<int, int>>();

                // Retrieve and parse matching answers
                foreach (var key in form.Keys.Where(k => k.StartsWith("matchingAnswers[")))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(key, @"matchingAnswers\[(\d+)\]\[(\d+)\]");
                    if (match.Success)
                    {
                        int outerKey = int.Parse(match.Groups[1].Value);
                        int innerKey = int.Parse(match.Groups[2].Value);
                        int value = int.Parse(form[key]);

                        if (!matchingAnswers.ContainsKey(outerKey))
                        {
                            matchingAnswers[outerKey] = new Dictionary<int, int>();
                        }
                        matchingAnswers[outerKey][innerKey] = value;
                    }
                }

                // Retrieve and parse single-choice/multiple-choice answers
                Dictionary<int, List<int>> answers = new Dictionary<int, List<int>>();
                foreach (var key in form.Keys.Where(k => k.StartsWith("answers[")))
                {
                    var match = Regex.Match(key, @"answers\[(\d+)\]");
                    if (match.Success)
                    {
                        int outerKey = int.Parse(match.Groups[1].Value);
                        int value = int.Parse(form[key]);
                        if (!answers.ContainsKey(outerKey))
                        {
                            answers[outerKey] = new List<int>();
                        }
                        answers[outerKey].Add(value);
                    }
                }

                if (string.IsNullOrWhiteSpace(studentIdString))
                {
                    return BadRequest("Invalid Student ID format.");
                }

                // Retrieve question IDs and current question index from the session
                var questionIds = JsonSerializer.Deserialize<List<int>>(HttpContext.Session.GetString("ExamQuestions"));
                var currentIndex = HttpContext.Session.GetInt32("CurrentQuestion") ?? 0;

                // Load existing answers from the session
                var answersDict = HttpContext.Session.GetString("ExamAnswers") != null
                    ? JsonSerializer.Deserialize<Dictionary<int, List<int>>>(HttpContext.Session.GetString("ExamAnswers"))
                    : new Dictionary<int, List<int>>();

                var MatchingAnswersDict = HttpContext.Session.GetString("ExamMatchingAnswers") != null
                    ? JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, int>>>(HttpContext.Session.GetString("ExamMatchingAnswers"))
                    : new Dictionary<int, Dictionary<int, int>>();

                // Retrieve and update flagged questions
                var flaggedQuestions = HttpContext.Session.GetString("FlaggedQuestions") != null
                    ? JsonSerializer.Deserialize<HashSet<int>>(HttpContext.Session.GetString("FlaggedQuestions"))
                    : new HashSet<int>();

                var flaggedQuestionsInput = form["flaggedQuestions"].ToString(); // Convert StringValues to string
                if (!string.IsNullOrEmpty(flaggedQuestionsInput))
                {
                    flaggedQuestions = flaggedQuestionsInput.Split(',')
                        .Select(q => int.TryParse(q, out int qId) ? qId : (int?)null)
                        .Where(qId => qId.HasValue)
                        .Select(qId => qId.Value)
                        .ToHashSet();

                    HttpContext.Session.SetString("FlaggedQuestions", JsonSerializer.Serialize(flaggedQuestions));
                }


                // Save current answers
                if (answers.Any())
                {
                    foreach (var answer in answers)
                    {
                        answersDict[answer.Key] = answer.Value;
                    }
                    HttpContext.Session.SetString("ExamAnswers", JsonSerializer.Serialize(answersDict));
                }

                if (matchingAnswers.Any())
                {
                    foreach (var matchingAnswer in matchingAnswers)
                    {
                        MatchingAnswersDict[matchingAnswer.Key] = matchingAnswer.Value;
                    }
                    HttpContext.Session.SetString("ExamMatchingAnswers", JsonSerializer.Serialize(MatchingAnswersDict));
                }

                // Update current question index based on direction
                currentIndex = (direction != 0) ? currentIndex + direction : question_no - 1;
                HttpContext.Session.SetInt32("CurrentQuestion", currentIndex);

                // Handle exam submission if the last question is reached
                if (currentIndex >= questionIds.Count)
                {
                    return SubmitExam(studentIdString).GetAwaiter().GetResult(); // Force execution synchronously
                }

                // Fetch the next question and its options
                var question = _context.Questions
                    .Include(q => q.Options)
                    .Include(q => q.MatchingPairs)
                    .FirstOrDefault(q => q.Id == questionIds[currentIndex]);

                if (question == null)
                {
                    return NotFound("Question not found.");
                }

                // Set ViewBag properties for rendering
                ViewBag.StudentId = studentIdString;
                ViewBag.TotalQuestions = questionIds.Count;
                ViewBag.CurrentQuestion = currentIndex + 1;
                ViewBag.FlaggedQuestions = flaggedQuestions;

                // Pass stored answers to the view for pre-selection
                ViewBag.SelectedAnswers = answersDict.ContainsKey(question.Id)
                    ? new Dictionary<int, List<int>> { { question.Id, answersDict[question.Id] } }
                    : new Dictionary<int, List<int>> { { question.Id, new List<int>() } };

                ViewBag.SelectedMatchingAnswers = MatchingAnswersDict.ContainsKey(question.Id)
                    ? new Dictionary<int, Dictionary<int, int>> { { question.Id, MatchingAnswersDict[question.Id] } }
                    : new Dictionary<int, Dictionary<int, int>> { { question.Id, new Dictionary<int, int>() } };
                #region timer



                ViewBag.ExamEndTime = DateTime.Parse(HttpContext.Session.GetString("ExamStartTime")).AddMinutes(HttpContext.Session.GetInt32("ExamDuration").Value);
                #endregion

                ViewBag.ExamName = HttpContext.Session.GetString("ExamName");

                return View("TakeExam", question);
            }
        }


        public async Task<IActionResult> SubmitExam(string studentId)
        {
            var _context = _dbContextFactory.CreateDbContext();
            int? examId = HttpContext.Session.GetInt32("ExamId");

                var answers = JsonSerializer.Deserialize<Dictionary<int, List<int>>>(
                    HttpContext.Session.GetString("ExamAnswers") ?? "{}"
                );
                var matchingAnswers = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, int>>>(
                   HttpContext.Session.GetString("ExamMatchingAnswers") ?? "{}"
               );

                int score = 0;
                // need modifications
                foreach (var answer in answers)
                {
                    var question = _context.Questions.Include(q => q.Options).FirstOrDefault(q => q.Id == answer.Key);
                    if (question != null)
                    {
                        var correctOptions = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
                        if (correctOptions.SequenceEqual(answer.Value.OrderBy(v => v)))
                        {
                            score++;
                        }
                    }
                }
                foreach (var answer in matchingAnswers)
                {
                    bool rightAnswer = true;
                    foreach (var matchPair in answer.Value)
                    {
                        if (matchPair.Key != matchPair.Value)
                        {
                            rightAnswer = false;
                            break;
                        }
                    }
                    if (rightAnswer)
                    {
                        score++;
                    }

                }


                var student = await _userManager.Users.Include(u => u.ResultRecords).FirstOrDefaultAsync(u => u.Id == studentId);
                if (student != null)
                {
                    var resultRecord = new ResultRecord
                    {
                        StudentId = studentId,
                        ExamId = examId ?? 0,
                        Score = score
                    };

                    var progress = await _context.ExamProgress.FirstOrDefaultAsync(p => p.StudentId == studentId && !p.IsCompleted);
                    if (progress == null) return NotFound("Exam progress not found.");

                    progress.IsCompleted = true;
                    _context.ResultRecords.Add(resultRecord);
                    await  _context.SaveChangesAsync();

                }


                // Clear session
                HttpContext.Session.Clear();

                return RedirectToAction("Result", new { studentId = studentId, ExamId = examId });
            
        }

        [HttpGet]
        public async Task<IActionResult> Result(string studentId, int examId)
        {
            var student = await _userManager.Users
                .Include(u => u.ResultRecords)
                .ThenInclude(r => r.Exam) // ✅ Ensure Exam is loaded
                .FirstOrDefaultAsync(u => u.Id == studentId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var resultRecord = student.ResultRecords?
                .FirstOrDefault(r => r.ExamId == examId);

            if (resultRecord == null)
            {
                return NotFound("Result not found for this exam.");
            }
            ViewData["UseCustomLayout"] = true;
            return View(resultRecord);
        }

    }

}
