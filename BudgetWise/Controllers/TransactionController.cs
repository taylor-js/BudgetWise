using BudgetWise.Areas.Identity.Data;
using BudgetWise.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetWise.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionController(ILogger<TransactionController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId)
                .OrderByDescending(d => d.Date)
                .Where(d => d.Date <= DateTime.UtcNow)
                .ToListAsync();

            return View(transactions);
        }

        // GET: Transaction/Create
        [HttpGet]
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            PopulateCategories();

            if (id == 0)
            {
                var transaction = new Transaction
                {
                    UserId = _userManager.GetUserId(User) ?? string.Empty,
                };
                return View(transaction);
            }
            else
            {
                var transaction = await _context.Transactions.FindAsync(id);
                if (transaction == null || transaction.UserId != _userManager.GetUserId(User))
                {
                    return NotFound();
                }
                return View(transaction);
            }
        }

        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date,UserId")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    transaction.UserId = userId ?? string.Empty;
                    if (transaction.Date.Kind == DateTimeKind.Unspecified)
                    {
                        transaction.Date = DateTime.SpecifyKind(transaction.Date, DateTimeKind.Utc);
                    }
                    if (transaction.TransactionId == 0)
                    {
                        _context.Add(transaction);
                    }
                    else
                    {
                        var existingTransaction = await _context.Transactions.FindAsync(transaction.TransactionId);
                        if (existingTransaction == null || existingTransaction.UserId != userId)
                        {
                            return NotFound();
                        }
                        existingTransaction.Amount = transaction.Amount;
                        existingTransaction.Note = transaction.Note;
                        existingTransaction.Date = transaction.Date;
                        existingTransaction.UserId = userId ?? string.Empty;
                        _context.Update(existingTransaction);
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Handle the case when userId is null
                    // You can add appropriate error handling or redirection logic here
                    ModelState.AddModelError("", "User ID is missing.");
                }
            }
            PopulateCategories();
            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null || transaction.UserId != userId)
            {
                return NotFound();
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        public void PopulateCategories()
        {
            var userId = _userManager.GetUserId(User);
            var categoryCollection = _context.Categories
                .Where(c => c.UserId == userId)
                .ToList();

            Category defaultCategory = new Category() { CategoryId = 0, Title = "Choose a Category" };
            categoryCollection.Insert(0, defaultCategory);
            ViewBag.Categories = categoryCollection;
        }
    }
}