using BudgetWise.Areas.Identity.Data;
using BudgetWise.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BudgetWise.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ILogger<DashboardController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            ViewData["isDashboard"] = true;

            ViewBag.TotalIncome = await GetTotalIncome();
            ViewBag.TotalExpense = await GetTotalExpense();
            ViewBag.Balance = await GetBalance();
            ViewBag.TreemapData = await GetTreemapData() ?? new List<object>();
            ViewBag.BarChartData = await GetBarChartData() ?? new List<BarChartData>();
            ViewBag.MonthlyTrendChartData = await GetMonthlyTrendChartData() ?? new List<MonthlyTrendData>();
            ViewBag.bubbleChartData = await GetBubbleChartData() ?? new List<BubbleChartData>();
            ViewBag.StackedColumnChartData = await GetStackedColumnChartData() ?? new List<object>();
            ViewBag.StackedAreaChartData = await GetStackedAreaChartData() ?? new List<object>();

            return View();
        }

        private async Task<string> GetTotalIncome()
        {
            string userId = _userManager.GetUserId(User) ?? string.Empty;
            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.UserId == userId)
                .OrderBy(y => y.Date)
                .ToListAsync();
            DateTime? earliestDate = selectedTransactions.FirstOrDefault()?.Date;
            if (earliestDate == null)
            {
                return 0.ToString("C0");
            }
            DateTime startDate = earliestDate.Value;
            DateTime endDate = DateTime.Today;
            int totalIncome = selectedTransactions
                .Where(i => i.Date.Date >= startDate && i.Date.Date <= endDate && i.Category?.Type == "Income")
                .Sum(j => j.Amount);

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;

            return String.Format(culture, "{0:C0}", totalIncome);
        }

        private async Task<string> GetTotalExpense()
        {
            string userId = _userManager.GetUserId(User) ?? string.Empty;
            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.UserId == userId)
                .OrderBy(y => y.Date)
                .ToListAsync();
            DateTime? earliestDate = selectedTransactions.FirstOrDefault()?.Date;
            if (earliestDate == null)
            {
                return 0.ToString("C0");
            }
            DateTime startDate = earliestDate.Value;
            DateTime endDate = DateTime.Today;
            int totalExpense = selectedTransactions
                .Where(i => i.Date.Date >= startDate && i.Date.Date <= endDate && i.Category?.Type == "Expense")
                .Sum(j => j.Amount);

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;

            return String.Format(culture, "{0:C0}", totalExpense);
        }

        private async Task<string> GetBalance()
        {
            string userId = _userManager.GetUserId(User) ?? string.Empty;
            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.UserId == userId)
                .OrderBy(y => y.Date)
                .ToListAsync();
            DateTime? earliestDate = selectedTransactions.FirstOrDefault()?.Date;
            if (earliestDate == null)
            {
                return 0.ToString("C0");
            }
            DateTime startDate = earliestDate.Value;
            DateTime endDate = DateTime.Today;
            int totalIncome = selectedTransactions
                .Where(i => i.Date.Date >= startDate && i.Date.Date <= endDate && i.Category?.Type == "Income")
                .Sum(j => j.Amount);
            int totalExpense = selectedTransactions
                .Where(i => i.Date.Date >= startDate && i.Date.Date <= endDate && i.Category?.Type == "Expense")
                .Sum(j => j.Amount);
            int balance = totalIncome - totalExpense;

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            return String.Format(culture, "{0:C0}", balance);
        }

        //Treemap: Expense by Category - Last 7 Days
        private async Task<List<object>> GetTreemapData()
        {
            // Define the start and end date for the past week
            DateTime startDate = DateTime.Today.AddDays(-6);
            DateTime endDate = DateTime.Today;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            // Retrieve the transactions for the user within the specified date range
            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date.Date >= startDate && y.Date.Date <= endDate && y.UserId == userId)
                .ToListAsync();

            // Create a CultureInfo object for formatting currency values
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;

            // Group the transactions by category, calculate the sum of amounts,
            // and format the amount as currency using the specified culture
            var treemapData = selectedTransactions
                .Where(i => i.Category?.Type == "Expense")
                .GroupBy(j => j.Category?.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category?.Icon + " " + k.First().Category?.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = String.Format(culture, "{0:C0}", k.Sum(j => j.Amount))
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            // Cast the list to a list of objects and return, or an empty list if null
            return treemapData.Cast<object>().ToList() ?? new List<object>();
        }


        // Bar Chart: Income vs Expense - Last 7 Days
        private async Task<List<BarChartData>> GetBarChartData()
        {
            DateTime startDate = DateTime.Today.AddDays(-6); // Last 7 days including today
            DateTime endDate = DateTime.Today; // Include today
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            // Logging for debugging
            Console.WriteLine($"Start Date: {startDate}, End Date: {endDate}");

            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date.Date >= startDate && y.Date.Date <= endDate && y.UserId == userId)
                .ToListAsync();

            var barChartData = selectedTransactions
                .GroupBy(t => t.Date.Date) // Group by date only
                .Select(g => new BarChartData
                {
                    date = g.Key,
                    day = g.Key.ToString("MMM dd"),
                    income = g.Where(t => t.Category?.Type == "Income").Sum(t => t.Amount),
                    expense = g.Where(t => t.Category?.Type == "Expense").Sum(t => t.Amount)
                })
                .OrderBy(d => d.date)
                .ToList();

            // Logging for debugging
            barChartData.ForEach(data =>
            {
                Console.WriteLine($"Date: {data.date}, Day: {data.day}, Income: {data.income}, Expense: {data.expense}");
            });

            return barChartData ?? new List<BarChartData>();
        }

        // Stacked Column Chart: Income vs Expense - Last 30 Days
        private async Task<List<object>> GetStackedColumnChartData()
        {
            DateTime startDate = DateTime.Today.AddDays(-29);
            DateTime endDate = DateTime.Today; // Include today
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            Console.WriteLine($"Start Date: {startDate}, End Date: {endDate}");

            var selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(t => t.Date.Date >= startDate && t.Date.Date <= endDate && t.UserId == userId)
                .ToListAsync();

            var stackedColumnChartData = selectedTransactions
                .GroupBy(t => new { Date = t.Date.Date, Type = t.Category?.Type ?? "Unknown" })
                .Select(g => new
                {
                    Date = g.Key.Date,
                    Type = g.Key.Type,
                    Amount = g.Sum(t => t.Amount)
                })
                .GroupBy(x => x.Date)
                .Select(y => new
                {
                    Date = y.Key,
                    Income = y.Where(z => z.Type == "Income").Sum(z => z.Amount),
                    Expense = y.Where(z => z.Type == "Expense").Sum(z => z.Amount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Logging for debugging
            stackedColumnChartData.ForEach(data =>
            {
                Console.WriteLine($"Date: {data.Date}, Income: {data.Income}, Expense: {data.Expense}");
            });

            return stackedColumnChartData.Cast<object>().ToList();
        }

        // Bubble chart
        private async Task<List<BubbleChartData>> GetBubbleChartData()
        {
            DateTime startDate = DateTime.Today.AddMonths(-11);
            DateTime endDate = DateTime.Today;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date.Date >= startDate && y.Date.Date <= endDate && y.UserId == userId)
                .ToListAsync();

            DateTime? earliestTransactionDate = selectedTransactions
                .OrderBy(t => t.Date)
                .Select(t => t.Date.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue)
            {
                startDate = earliestTransactionDate.Value;
            }

            var bubbleChartData = selectedTransactions
                .Where(t => t.Date.Date >= startDate)
                .GroupBy(t => new { t.Category?.Title, t.Category?.Type })
                .Select(g => new BubbleChartData
                {
                    category = g.Key.Title ?? "Unknown",
                    type = g.Key.Type ?? "Unknown",
                    amount = g.Sum(t => t.Amount),
                    size = g.Count()
                })
                .OrderBy(d => d.size)
                .ToList();

            return bubbleChartData ?? new List<BubbleChartData>();
        }
        //Line Chart (3 Lines): Monthly Trend - Last 12 Months from First Entry
        private async Task<List<MonthlyTrendData>> GetMonthlyTrendChartData()
        {
            DateTime startDate = DateTime.Today.AddMonths(-11);
            DateTime endDate = DateTime.Today;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date.Date >= startDate && y.Date.Date <= endDate && y.UserId == userId)
                .ToListAsync();

            DateTime? earliestTransactionDate = selectedTransactions
                .OrderBy(t => t.Date)
                .Select(t => t.Date.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue)
            {
                startDate = earliestTransactionDate.Value;
            }

            List<MonthlyTrendData> monthlyTrendChartData = new List<MonthlyTrendData>();

            for (DateTime date = startDate; date <= endDate; date = date.AddMonths(1))
            {
                int totalIncome = selectedTransactions
                    .Where(i => i.Category?.Type == "Income" && i.Date.Year == date.Year && i.Date.Month == date.Month)
                    .Sum(j => j.Amount);

                int totalExpense = selectedTransactions
                    .Where(i => i.Category?.Type == "Expense" && i.Date.Year == date.Year && i.Date.Month == date.Month)
                    .Sum(j => j.Amount);

                int balance = totalIncome - totalExpense;

                monthlyTrendChartData.Add(new MonthlyTrendData
                {
                    Month = date.ToString("MMM yyyy"),
                    Income = totalIncome,
                    Expense = totalExpense,
                    Balance = balance
                });
            }

            return monthlyTrendChartData ?? new List<MonthlyTrendData>();
        }

        //Staked Area Chart: Income vs Expense - Last 12 Months from First Entry
        private async Task<List<object>> GetStackedAreaChartData()
        {
            DateTime startDate = DateTime.Today.AddMonths(-11);
            DateTime endDate = DateTime.Today;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date.Date >= startDate && y.Date.Date <= endDate && y.UserId == userId)
                .ToListAsync();

            DateTime? earliestTransactionDate = selectedTransactions
                .OrderBy(t => t.Date)
                .Select(t => t.Date.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue && earliestTransactionDate.Value < startDate)
            {
                startDate = new DateTime(earliestTransactionDate.Value.Year, earliestTransactionDate.Value.Month, 1);
            }

            var stackedAreaChartData = selectedTransactions
                .Where(t => t.Date.Date >= startDate)
                .GroupBy(t => new { Month = t.Date.ToString("MMM yyyy"), Type = t.Category?.Type ?? "Unknown" })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Type = g.Key.Type,
                    Amount = g.Sum(t => t.Amount)
                })
                .GroupBy(x => x.Month)
                .Select(y => new
                {
                    Month = y.Key,
                    Income = y.Where(z => z.Type == "Income").Sum(z => z.Amount),
                    Expense = y.Where(z => z.Type == "Expense").Sum(z => z.Amount),
                    FormattedIncome = y.Where(z => z.Type == "Income").Sum(z => z.Amount).ToString("C0"),
                    FormattedExpense = y.Where(z => z.Type == "Expense").Sum(z => z.Amount).ToString("C0")
                })
                .OrderBy(x => DateTime.ParseExact(x.Month, "MMM yyyy", CultureInfo.InvariantCulture))
                .ToList();

            return stackedAreaChartData.Cast<object>().ToList() ?? new List<object>();
        }
    }

    public class MonthlyTrendData
    {
        public string Month { get; set; }
        public int Income { get; set; }
        public int Expense { get; set; }
        public int Balance { get; set; }
    }

    public class BarChartData
    {
        public DateTime date { get; set; }
        public string day;
        public int income;
        public int expense;
    }

    public class BubbleChartData
    {
        public string category;
        public string type;
        public int amount;
        public int size;
    }
}