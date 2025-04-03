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

[Route("api/exam")]
[ApiController]
public class ExamApiController : ControllerBase
{

        //private readonly ExamContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDbContextFactory<ExamContext> _dbContextFactory;

        public ExamApiController(UserManager<ApplicationUser> userManager, IDbContextFactory<ExamContext> dbContextFactory)
        {
            _userManager = userManager;
            _dbContextFactory = dbContextFactory;
        }

     

        [HttpPost("save-progress")]
        public IActionResult SaveProgress([FromBody] ExamProgressDto progressDto)
        {
            using (var _context = _dbContextFactory.CreateDbContext())
            {
                var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var examProgress = _context.ExamProgress.FirstOrDefault(p => p.StudentId == studentId && !p.IsCompleted);

                if (examProgress == null) return BadRequest("No active exam found.");

                examProgress.CurrentQuestionIndex = progressDto.CurrentQuestionIndex;
                examProgress.AnswersJson = JsonSerializer.Serialize(progressDto.Answers);
                examProgress.MatchingAnswersJson = JsonSerializer.Serialize(progressDto.MatchingAnswers);
                examProgress.FlaggedQuestionsJson = JsonSerializer.Serialize(progressDto.FlaggedQuestions);
                examProgress.TimeLeftInSeconds = progressDto.TimeLeftInSeconds;

               

                _context.SaveChanges();
                return Ok(new { message = "Progress saved successfully." });
            }
           
        }


        [Authorize(Roles = "ExamTaker")]
        [HttpGet("take-exam/{code}")]
        public IActionResult TakeExam( Guid code)
        {
            using (var _context = _dbContextFactory.CreateDbContext())
            {

                var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (studentId == null) return Unauthorized("User not authenticated.");

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
                    return Ok(new
                    {
                        progress.ExamStartTime,
                        progress.CurrentQuestionIndex,
                        ExamName = exam.Name,
                        TimeLeftInSeconds = progress.TimeLeftInSeconds,
                        Questions = JsonSerializer.Deserialize<List<int>>(progress.ExamQuestionsJson),
                        Answers = JsonSerializer.Deserialize<Dictionary<int, List<int>>>(progress.AnswersJson),
                        MatchingAnswers = JsonSerializer.Deserialize<Dictionary<int, Dictionary<int, int>>>(progress.MatchingAnswersJson),
                        FlaggedQuestions = JsonSerializer.Deserialize<HashSet<int>>(progress.FlaggedQuestionsJson)
                    });

                }
                var randomQuestions = exam.Questions.OrderBy(q => Guid.NewGuid()).Take(150).ToList();
                var questionIdsList = randomQuestions.Select(q => q.Id).ToList();

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
                    ExamQuestionsJson = JsonSerializer.Serialize(questionIdsList)
                };

                _context.ExamProgress.Add(newProgress);
                _context.SaveChanges();
                return Ok(new
                {
                    ExamStartTime = newProgress.ExamStartTime,
                    CurrentQuestionIndex = newProgress.CurrentQuestionIndex,
                    ExamName = exam.Name,
                    TimeLeftInSeconds = newProgress.TimeLeftInSeconds,
                    Questions = questionIdsList,
                    Answers = new Dictionary<int, List<int>>(),
                    MatchingAnswers = new Dictionary<int, Dictionary<int, int>>(),
                    FlaggedQuestions = new HashSet<int>()
                });
            }

        }

        [HttpPost("navigate-question")]
        public IActionResult NavigateQuestion([FromBody] NavigationRequestDto requestDto)
        {
            using (var _context = _dbContextFactory.CreateDbContext())
            {
                if (requestDto.TimeUp)
                    return SubmitExam(requestDto.StudentId);

                var progress = _context.ExamProgress.FirstOrDefault(p => p.StudentId == requestDto.StudentId);
                if (progress == null)
                    return NotFound("Progress not found.");

                progress.CurrentQuestionIndex += requestDto.Direction;
                progress.AnswersJson = JsonSerializer.Serialize(requestDto.Answers);
                progress.MatchingAnswersJson = JsonSerializer.Serialize(requestDto.MatchingAnswers);

                _context.SaveChanges();

                return Ok(new { message = "Question navigation successful." });
            }
        }

        private IActionResult SubmitExam(string studentId)
        {
            using (var _context = _dbContextFactory.CreateDbContext())
            {
                var progress = _context.ExamProgress.FirstOrDefault(p => p.StudentId == studentId);
                if (progress == null) return NotFound("Exam progress not found.");

                progress.IsCompleted = true;
                _context.SaveChanges();

                return Ok(new { message = "Exam submitted successfully." });
            }
        }
    }

    public class ExamProgressDto
    {
        public int CurrentQuestionIndex { get; set; }
        public Dictionary<int, List<int>> Answers { get; set; }
        public Dictionary<int, Dictionary<int, int>> MatchingAnswers { get; set; }
        public HashSet<int> FlaggedQuestions { get; set; }
        public int TimeLeftInSeconds { get; set; }
    }

    public class NavigationRequestDto
    {
        public string StudentId { get; set; }
        public int Direction { get; set; }
        public Dictionary<int, List<int>> Answers { get; set; }
        public Dictionary<int, Dictionary<int, int>> MatchingAnswers { get; set; }
        public bool TimeUp { get; set; }
    }
}
