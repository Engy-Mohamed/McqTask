using Microsoft.AspNetCore.Mvc;
using McqTask.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

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
                .Select(e => new { e.Name, e.ExamTime })
                .ToListAsync();

            ViewBag.TotalExams = totalExams;
            ViewBag.ActiveExams = activeExams;
            ViewBag.StudentCount = studentCount;
            ViewBag.ExamData = examData;

            return View();
        }
    }
}
