using Microsoft.AspNetCore.Mvc;
using McqTask.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DocumentFormat.OpenXml.Spreadsheet;

namespace McqTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly ExamContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ExamContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var totalExams = await _context.Exams.CountAsync();
            var activeExams = await _context.Exams.CountAsync(e => e.IsActive);
            var studentCount = (await _userManager.GetUsersInRoleAsync("examtaker")).Count;

            var examData = await _context.Exams
                .OrderByDescending(e => e.ExamTime)
                .Take(5)
                .Select(e => new { e.Name, e.Date })
                .ToListAsync();

            // Get latest 5 exams the student has attempted
            var recentExams = await _context.ExamProgress
                .Where(ep => ep.IsCompleted) // Only completed exams
                .OrderByDescending(ep => ep.ExamStartTime) // Sort by most recent attempt
                .Take(5)
                .Select(ep => new
                {
                    ep.ExamId,
                    ep.StudentId,
                    ep.ExamStartTime
                })
                .ToListAsync();

            ViewBag.RecentExams = recentExams;

            // Get the last completed exam attempt
            var lastExamAttempt = recentExams.FirstOrDefault();

            // Fetch the highest score of the last taken exam
            if (lastExamAttempt != null)
            {
                var highestScore = await _context.ResultRecords
                    .Where(r => r.ExamId == lastExamAttempt.ExamId)
                    .OrderByDescending(r => r.Score)
                    .FirstOrDefaultAsync();

                ViewBag.LastExam = lastExamAttempt.ExamId;
                ViewBag.HighestScore = highestScore?.Score ?? 0;
            }
            else
            {
                ViewBag.LastExam = "No exams taken";
                ViewBag.HighestScore = null;
            }

            ViewBag.TotalExams = totalExams;
            ViewBag.ActiveExams = activeExams;
            ViewBag.StudentCount = studentCount;
            ViewBag.ExamData = examData;

            return View();
        }
    }
}
