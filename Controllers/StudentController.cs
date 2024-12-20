using McqTask.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
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
            _context.Students.Add(student);
            _context.SaveChanges();

            return RedirectToAction("TakeExam", new { studentId = student.Id });
        }

        public IActionResult TakeExam(int studentId)
        {
            var numberOfQuestions = 5;
            //get matching pairs +options
            var allQuestions = _context.Questions.Include(q => q.Options).Include(q=>q.MatchingPairs).ToList();
            // Get 10 random questions
            var randomQuestions = allQuestions.OrderBy(q => Guid.NewGuid()).Take(numberOfQuestions).ToList();

            // Store questions in session
            HttpContext.Session.SetString("ExamQuestions", JsonSerializer.Serialize(randomQuestions.Select(q => q.Id)));
            HttpContext.Session.SetInt32("CurrentQuestion", 0);

            ViewBag.StudentId = studentId;
            ViewBag.TotalQuestions = numberOfQuestions;
            ViewBag.CurrentQuestion = 1;

            return View(randomQuestions.First());
        }
        [HttpPost]
        //public IActionResult NavigateQuestion(int? studentId, int direction, Dictionary<int, List<int>>? answers, Dictionary<int, Dictionary<int, int>>? matchingAnswers)
        public IActionResult NavigateQuestion(int studentId, int direction, [FromForm] IFormCollection form)
        {
            string studentIdString = Request.Form["studentId"];
            Dictionary<int, Dictionary<int, int>>? matchingAnswers = new Dictionary<int, Dictionary<int, int>>();

            var matchingansers = form["matchingAnswers"];
            Dictionary<int, List<int>>? answers = new Dictionary<int, List<int>>();
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

            foreach (var key in form.Keys)
            {
                if (key.StartsWith("answers["))
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
            }
            // Getting value from hidden input field


            if (int.TryParse(studentIdString, out int parsedStudentId))
            {
                ;
            }
            else
            {
                // Handle other types or error if needed
                return BadRequest("Invalid Student ID format.");
            }
          
            // Retrieve question IDs and current question index from the session
            var questionIds = JsonSerializer.Deserialize<List<int>>(HttpContext.Session.GetString("ExamQuestions"));
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestion") ?? 0;

            // Load existing answers from the session or initialize a new dictionary
            var answersDict = HttpContext.Session.GetString("ExamAnswers") != null
                ? JsonSerializer.Deserialize<Dictionary<int, List<int>>>(HttpContext.Session.GetString("ExamAnswers"))
                : new Dictionary<int, List<int>>();
            var MatchingAnswersDict = HttpContext.Session.GetString("ExamMatchingAnswers") != null
                ? JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, int>>>(HttpContext.Session.GetString("ExamMatchingAnswers"))
                : new Dictionary<int, Dictionary<int, int>>();

            // Save current answers if provided
            if (answers != null && answers.Any())
            {
                foreach (var answer in answers)
                {
                    answersDict[answer.Key] = answer.Value; // Add or update answers
                }
                HttpContext.Session.SetString("ExamAnswers", JsonSerializer.Serialize(answersDict));
            }
            if (matchingAnswers != null && matchingAnswers.Any())
            {
                foreach (var matchingAnswer in matchingAnswers)
                {
                    MatchingAnswersDict[matchingAnswer.Key] = matchingAnswer.Value; // Add or update answers
                }
                HttpContext.Session.SetString("ExamMatchingAnswers", JsonSerializer.Serialize(MatchingAnswersDict));
            }

            // Update current question index based on direction
            currentIndex += direction;
            HttpContext.Session.SetInt32("CurrentQuestion", currentIndex);

            // Handle exam submission if the last question is reached
            if (currentIndex >= questionIds.Count)
            {
                return SubmitExam(parsedStudentId);
            }

            // Fetch the next question and its options
            var question = _context.Questions
                .Include(q => q.Options).Include(q => q.MatchingPairs)
                .FirstOrDefault(q => q.Id == questionIds[currentIndex]);

            if (question == null)
            {
                return NotFound("Question not found.");
            }

            // Set ViewBag properties for rendering
            ViewBag.StudentId = parsedStudentId;
            ViewBag.TotalQuestions = questionIds.Count;
            ViewBag.CurrentQuestion = currentIndex + 1;

            // Pass stored answers to the view for pre-selection
            ViewBag.SelectedAnswers = answersDict.ContainsKey(question.Id)
                ? new Dictionary<int, List<int>> { { question.Id, answersDict[question.Id] } }
                : new Dictionary<int, List<int>> { { question.Id, new List<int>() } };

            ViewBag.SelectedMatchingAnswers = MatchingAnswersDict.ContainsKey(question.Id)
                ? new Dictionary<int, Dictionary<int,int>> { { question.Id, MatchingAnswersDict[question.Id] } }
                : new Dictionary<int, Dictionary<int, int>> { { question.Id, new Dictionary<int, int>() } };

            return View("TakeExam", question);
        }


        public IActionResult SubmitExam(int? studentId)
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
