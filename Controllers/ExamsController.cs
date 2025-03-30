using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using McqTask.Models;
using McqTask.ViewModels;

namespace McqTask.Controllers
{
    public class ExamsController : Controller
    {
        private readonly ExamContext _context;

        public ExamsController(ExamContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Edit(int id)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamGroups) // Load ExamGroups
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
                return NotFound();

            var viewModel = new ExamViewModel
            {
                Id = exam.Id,
                Name = exam.Name,
                Date = exam.Date,
                CategoryId = exam.CategoryId,
                EndDate = exam.EndDate,
                IsActive = exam.IsActive,
                IsPracticeMode = exam.IsPracticeMode,
                IsPublic = exam.IsPublic,
                ExamTime = exam.ExamTime,
                ExamGroups = exam.ExamGroups.Select(g => g.GroupId).ToList() // Get selected groups
            };

            ViewBag.Groups = new SelectList(_context.Groups, "Id", "Name", viewModel.ExamGroups);
            ViewData["Category"] = new SelectList(_context.Category, "Id", "Name", exam.CategoryId);

            return View(viewModel);
        }

        // GET: Exams
        public async Task<IActionResult> Index()
        {
            var examContext = _context.Exams.Include(e => e.Category);
            return View(await examContext.ToListAsync());
        }

        // GET: Exams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exam = await _context.Exams
      .Include(e => e.Category) // Load Category
      .Include(e => e.ExamGroups)
          .ThenInclude(eg => eg.Group) // Load Group inside ExamGroups
      .FirstOrDefaultAsync(m => m.Id == id);

            if (exam == null)
            {
                return NotFound();
            }

            return View(exam);
        }

        // GET: Exams/Create
        public IActionResult Create()
        {
            ViewBag.Groups = new SelectList(_context.Groups, "Id", "Name");
            ViewData["Category"] = new SelectList(_context.Category, "Id", "Name");

            return View(new ExamViewModel()); // Pass an empty model
        }

        // POST: Exams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exam = new Exam
                {
                    Name = model.Name,
                    Date = model.Date,
                    CategoryId = model.CategoryId,
                    EndDate = model.EndDate,
                    IsActive = model.IsActive,
                    IsPracticeMode = model.IsPracticeMode,
                    IsPublic = model.IsPublic,
                    ExamTime = model.ExamTime,
                    ExamGroups = model.ExamGroups.Select(groupId => new ExamGroup { GroupId = groupId }).ToList()
                };

                _context.Add(exam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Groups = new SelectList(_context.Groups, "Id", "Name", model.ExamGroups);
            ViewData["Category"] = new SelectList(_context.Category, "Id", "Name", model.CategoryId);

            return View(model);
        }



        // GET: Exams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exam = await _context.Exams
                    .Include(e => e.ExamGroups)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (exam == null)
                    return NotFound();

                // Update exam details
                exam.Name = model.Name;
                exam.Date = model.Date;
                exam.CategoryId = model.CategoryId;
                exam.EndDate = model.EndDate;
                exam.IsActive = model.IsActive;
                exam.IsPracticeMode = model.IsPracticeMode;
                exam.IsPublic = model.IsPublic;
                exam.ExamTime = model.ExamTime;

                // Clear old ExamGroups and add selected ones
                exam.ExamGroups.Clear();
                exam.ExamGroups = model.ExamGroups.Select(groupId => new ExamGroup { GroupId = groupId }).ToList();

                _context.Update(exam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Groups = new SelectList(_context.Groups, "Id", "Name", model.ExamGroups);
            ViewData["Category"] = new SelectList(_context.Category, "Id", "Name", model.CategoryId);

            return View(model);
        }



        // POST: Exams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    
      

        // GET: Exams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exam = await _context.Exams
     .Include(e => e.Category) // Load Category
     .Include(e => e.ExamGroups)
         .ThenInclude(eg => eg.Group) // Load Group inside ExamGroups
     .FirstOrDefaultAsync(m => m.Id == id);

            if (exam == null)
            {
                return NotFound();
            }

            return View(exam);
        }

        // POST: Exams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExamExists(int id)
        {
            return _context.Exams.Any(e => e.Id == id);
        }

        public IActionResult GenerateExamLink(int examId)
        {
            var exam = _context.Exams.Find(examId);
            if (exam == null || !exam.IsActive)
            {
                return NotFound("Exam not available.");
            }

            var examLink = $"{Request.Scheme}://{Request.Host}/Student/TakeExam?code={exam.ExamCode}";
            return Ok(new { link = examLink });
        }

        

        [HttpPost]
        public IActionResult CopyExam(int id, string newName)
        {
            var existingExam = _context.Exams
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Options) // Ensure Options are loaded
                .Include(e => e.Questions)
                    .ThenInclude(q => q.MatchingPairs) // Ensure MatchingPairs are loaded
                .FirstOrDefault(e => e.Id == id);

            if (existingExam == null)
            {
                return NotFound();
            }

            // Create a new Exam object
            var copiedExam = new Exam
            {
                Name = newName,
                Date = DateTime.Now,
                EndDate = existingExam.EndDate,
                ExamTime = existingExam.ExamTime,
                IsPublic = existingExam.IsPublic,
                IsPracticeMode = existingExam.IsPracticeMode,
                IsActive = existingExam.IsActive,
                CategoryId = existingExam.CategoryId,
                ExamGroups = existingExam.ExamGroups.ToList(), // Copy Exam Groups
                Questions = existingExam.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    Type = q.Type,
                    Options = q.Options?.Select(o => new Option // Copy options properly
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList(),
                    MatchingPairs = q.MatchingPairs?.Select(mp => new MatchingPair // Copy matching pairs properly
                    {
                        LeftSideText = mp.LeftSideText,
                        RightSideText = mp.RightSideText
                    }).ToList()
                }).ToList()
            };

            _context.Exams.Add(copiedExam);
            _context.SaveChanges(); // Save changes to assign a new Id

            return RedirectToAction("Details", new { id = copiedExam.Id });
        }

    }
}
