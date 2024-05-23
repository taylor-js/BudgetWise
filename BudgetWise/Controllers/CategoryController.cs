using BudgetWise.Areas.Identity.Data;
using BudgetWise.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BudgetWise.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly EmojiApiController _emojiApiController;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(ILogger<CategoryController> logger, ApplicationDbContext context, IHttpClientFactory httpClientFactory, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _httpClient = httpClientFactory.CreateClient();
            _emojiApiController = new EmojiApiController(_httpClient);
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Title)
                .ToListAsync();
            return View(categories);
        }

        // GET: Category/AddOrEdit
        [HttpGet]
        public async Task<IActionResult> AddOrEdit(int? id)
        {
            if (id == null)
            {
                var category = new Category
                {
                    UserId = _userManager.GetUserId(User) ?? string.Empty,
                };
                return View(category);
            }
            else
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null || category.UserId != _userManager.GetUserId(User))
                {
                    return NotFound();
                }
                return View(category);
            }
        }

        // POST: Category/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("CategoryId,Title,Icon,Type,UserId")] Category category)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                category.UserId = userId ?? string.Empty;
                if (category.CategoryId == 0)
                {
                    _context.Add(category);
                }
                else
                {
                    var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
                    if (existingCategory == null || existingCategory.UserId != userId)
                    {
                        return NotFound();
                    }
                    existingCategory.Title = category.Title;
                    existingCategory.Icon = category.Icon;
                    existingCategory.Type = category.Type;
                    _context.Update(existingCategory);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.UserId != userId)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
