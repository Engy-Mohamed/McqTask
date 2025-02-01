using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using McqTask.Models;

namespace McqTask.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly ExamContext _context;

        public QuestionsController(ExamContext context)
        {
            _context = context;
        }

        // GET: Questions
        public async Task<IActionResult> Index()
        {
            var examContext = _context.Questions.Include(q => q.Exam);
            return View(await examContext.ToListAsync());
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Exam)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Create
        public IActionResult Create()
        {
            ViewData["ExamData"] = new SelectList(_context.Exams, "Id", "Name");
            ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType)));
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Text,Type,ExamId")] QuestionViewModel questionView, List<Option> options, List<MatchingPair> matchingPairs)
        //{
        //    Question newQuestion;

        //    if (ModelState.IsValid)
        //    {
        //        newQuestion = new Question(questionView.ExamId, questionView.Text, questionView.Type);
        //        if (questionView.Type == QuestionType.MultipleChoice || questionView.Type == QuestionType.MultipleResponse)
        //        {
        //            newQuestion.Options = options;
        //        }
        //        else if (questionView.Type == QuestionType.Matching)
        //        {
        //            newQuestion.MatchingPairs = matchingPairs;
        //        }

        //        _context.Questions.Add(newQuestion);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["ExamId"] = new SelectList(_context.Exams, "Id", "Name", questionView.ExamId);
        //    return View(questionView);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If the model is invalid, reload the dropdowns and return the view
                ViewBag.ExamData = new SelectList(await _context.Exams.ToListAsync(), "Id", "Name");
                ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType)));

                return View(model);
            }

            // Create new Question entity
            var question = new Question
            {
                ExamId = model.ExamId,
                Text = model.Text,
                Type = model.Type
            };

            // Handle Options (for MultipleChoice or MultipleResponse)
            if (model.Type == QuestionType.MultipleChoice || model.Type == QuestionType.MultipleResponse)
            {
                question.Options = model.Options?
                    .Where(o => !string.IsNullOrWhiteSpace(o.Text)) // Ignore empty options
                    .Select(o => new Option
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    })
                    .ToList();
            }

            // Handle Matching Pairs (for Matching questions)
            if (model.Type == QuestionType.Matching)
            {
                question.MatchingPairs = model.MatchingPairs?
                    .Where(mp => !string.IsNullOrWhiteSpace(mp.LeftSideText) && !string.IsNullOrWhiteSpace(mp.RightSideText))
                    .Select(mp => new MatchingPair
                    {
                        LeftSideText = mp.LeftSideText,
                        RightSideText = mp.RightSideText
                    })
                    .ToList();
            }

            // Save to database
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to list of questions
        }

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.MatchingPairs)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            var model = new QuestionViewModel
            {
                Id = question.Id,
                ExamId = question.ExamId,
                Text = question.Text,
                Type = question.Type,
                Options = question.Options ?? new List<Option>(),
                MatchingPairs = question.MatchingPairs ?? new List<MatchingPair>()
            };

            ViewBag.ExamData = new SelectList(await _context.Exams.ToListAsync(), "Id", "Name", question.ExamId);
            ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType)), question.Type);

            return View(model);
        }


        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ExamData = new SelectList(await _context.Exams.ToListAsync(), "Id", "Name", model.ExamId);
                ViewBag.QuestionTypes = new SelectList(Enum.GetValues(typeof(QuestionType)), model.Type);
                return View(model);
            }

            var question = await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.MatchingPairs)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            // Update Question properties
            question.ExamId = model.ExamId;
            question.Text = model.Text;
            question.Type = model.Type;

            // Handle Options
            if (model.Type == QuestionType.MultipleChoice || model.Type == QuestionType.MultipleResponse)
            {
                question.Options = model.Options?
                    .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                    .Select(o => new Option
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    })
                    .ToList();
            }
            else
            {
                question.Options?.Clear(); // Clear existing options if question type changes
            }

            // Handle Matching Pairs
            if (model.Type == QuestionType.Matching)
            {
                question.MatchingPairs = model.MatchingPairs?
                    .Where(mp => !string.IsNullOrWhiteSpace(mp.LeftSideText) && !string.IsNullOrWhiteSpace(mp.RightSideText))
                    .Select(mp => new MatchingPair
                    {
                        LeftSideText = mp.LeftSideText,
                        RightSideText = mp.RightSideText
                    })
                    .ToList();
            }
            else
            {
                question.MatchingPairs?.Clear(); // Clear existing pairs if question type changes
            }

            _context.Update(question);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Questions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Exam)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}
