using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using McqTask.Data;
using McqTask.Models;
using System.Linq;
using System.Threading.Tasks;

namespace McqTask.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ExamContext _context;

        public ResultsController(ExamContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var results = await _context.ResultRecords
                .Include(r => r.Exam)
                .Include(r => r.Student)
                .ToListAsync();

            return View(results);
        }
        public async Task<IActionResult> ExamResults(int id)
        {
            var results = await _context.ResultRecords
                .Where(r => r.ExamId == id)
                .Include(r => r.Student)
                .Include(r => r.Exam)
                // .OrderByDescending(r => r.AttemptDate)
                .ToListAsync();

            return View("index",results);
        }
    }
}
        