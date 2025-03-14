using McqTask.Models;
using Microsoft.AspNetCore.Authorization;
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
            student.GroupId = 1;
            _context.Students.Add(student);
            _context.SaveChanges();

            return RedirectToAction("TakeExam", new { studentId = student.Id });
        }

        [Authorize(Roles = "ExamTaker")]
        public IActionResult TakeExam([FromQuery] Guid code)
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

            // 🟢 Step 1: Check if session already contains exam data (restore previous session)
            if (HttpContext.Session.GetString("ExamQuestions") != null &&
                HttpContext.Session.GetInt32("CurrentQuestion") != null)
            {
                // Restore Exam Progress
                var questionIds = JsonSerializer.Deserialize<List<int>>(HttpContext.Session.GetString("ExamQuestions"));
                int currentIndex = HttpContext.Session.GetInt32("CurrentQuestion").Value;

                ViewBag.ExamEndTime = HttpContext.Session.GetString("ExamEndTime");
                ViewBag.StudentId = studentId;
                ViewBag.TotalQuestions = questionIds.Count;
                ViewBag.CurrentQuestion = currentIndex + 1;

                // Get the next question
                var currentQuestion = _context.Questions
                    .Include(q => q.Options)
                    .Include(q => q.MatchingPairs)
                    .FirstOrDefault(q => q.Id == questionIds[currentIndex]);

                return View(currentQuestion);
            }

            // 🔴 Step 2: If no previous session, start a new exam
            int numberOfQuestions = Math.Min(150, exam.Questions.Count);
            var randomQuestions = exam.Questions.OrderBy(q => Guid.NewGuid()).Take(numberOfQuestions).ToList();

            HttpContext.Session.SetString("ExamQuestions", JsonSerializer.Serialize(randomQuestions.Select(q => q.Id)));
            HttpContext.Session.SetInt32("CurrentQuestion", 0);
            HttpContext.Session.SetString("ExamStartTime", DateTime.UtcNow.ToString("o"));
            HttpContext.Session.SetInt32("ExamDuration", exam.ExamTime);
            HttpContext.Session.SetString("FlaggedQuestions", JsonSerializer.Serialize(new HashSet<int>()));
            HttpContext.Session.SetString("ExamAnswers", JsonSerializer.Serialize(new Dictionary<int, List<int>>()));
            HttpContext.Session.SetString("ExamMatchingAnswers", JsonSerializer.Serialize(new Dictionary<int, Dictionary<int, int>>()));

            DateTime parsedStartTime = DateTime.Parse(HttpContext.Session.GetString("ExamStartTime")).ToUniversalTime();
            int examDuration = HttpContext.Session.GetInt32("ExamDuration").Value;
            DateTime examEndTime = parsedStartTime.AddMinutes(examDuration);

            HttpContext.Session.SetString("ExamEndTime", examEndTime.ToUniversalTime().ToString("o"));

            ViewBag.ExamEndTime = examEndTime.ToString("o");
            ViewBag.StudentId = studentId;
            ViewBag.TotalQuestions = numberOfQuestions;
            ViewBag.CurrentQuestion = 1;

            return View(randomQuestions.First());

        }
        [HttpPost]
        //public IActionResult NavigateQuestion(int? studentId, int direction, Dictionary<int, List<int>>? answers, Dictionary<int, Dictionary<int, int>>? matchingAnswers)
        [HttpPost]
        public IActionResult NavigateQuestion(int studentId, int direction, int question_no, [FromForm] IFormCollection form)
        {
            string studentIdString = Request.Form["studentId"];
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

            if(string.IsNullOrWhiteSpace(studentIdString))
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
                return SubmitExam(studentIdString);
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

            return View("TakeExam", question);
        }

        


        public IActionResult SubmitExam(string? studentId)
        {
            var answers = JsonSerializer.Deserialize<Dictionary<int, List<int>>>(
                HttpContext.Session.GetString("ExamAnswers") ?? "{}"
            );
            var matchingAnswers = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int,int>>>(
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
                foreach(var matchPair in answer.Value)
                {
                    if(matchPair.Key != matchPair.Value)
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

        [HttpGet]
        public IActionResult Result(int studentId)
        {
            var student = _context.Students.Find(studentId);
            return View(student);
        }
    }

}
