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
            ViewBag.TotalIncome = await GetTotalIncome();
            ViewBag.TotalExpense = await GetTotalExpense();
            ViewBag.Balance = await GetBalance();
            ViewBag.TreemapData = await GetTreemapData() ?? new List<object>();
            ViewBag.BarChartData = await GetBarChartData() ?? new List<BarChartData>();
            ViewBag.MonthlyTrendChartData = await GetMonthlyTrendChartData() ?? new List<MonthlyTrendData>();
            ViewBag.RadarChartData = await GetRadarChartData() ?? new RadarChartData { ExpenseData = new List<RadarData>(), IncomeData = new List<RadarData>() };
            ViewBag.StackedColumnChartData = await GetStackedColumnChartData() ?? new List<object>();
            ViewBag.StackedAreaChartData = await GetStackedAreaChartData() ?? new List<object>();

            return View();
        }

        private async Task<string> GetTotalIncome()
        {
            DateTime StartDate7Days = DateTime.UtcNow.Date.AddDays(-6);
            DateTime EndDate7Days = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions7Days = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate7Days && y.Date <= EndDate7Days && y.UserId == userId)
                .ToListAsync();

            int TotalIncome = SelectedTransactions7Days
                .Where(i => i.Category?.Type == "Income")
                .Sum(j => j.Amount);

            return TotalIncome.ToString("C0");
        }

        private async Task<string> GetTotalExpense()
        {
            DateTime StartDate7Days = DateTime.UtcNow.Date.AddDays(-6);
            DateTime EndDate7Days = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions7Days = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate7Days && y.Date <= EndDate7Days && y.UserId == userId)
                .ToListAsync();

            int TotalExpense = SelectedTransactions7Days
                .Where(i => i.Category?.Type == "Expense")
                .Sum(j => j.Amount);

            return TotalExpense.ToString("C0");
        }

        private async Task<string> GetBalance()
        {
            DateTime StartDate7Days = DateTime.UtcNow.Date.AddDays(-6);
            DateTime EndDate7Days = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions7Days = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate7Days && y.Date <= EndDate7Days && y.UserId == userId)
                .ToListAsync();

            int TotalIncome = SelectedTransactions7Days
                .Where(i => i.Category?.Type == "Income")
                .Sum(j => j.Amount);

            int TotalExpense = SelectedTransactions7Days
                .Where(i => i.Category?.Type == "Expense")
                .Sum(j => j.Amount);

            int Balance = TotalIncome - TotalExpense;

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;

            return String.Format(culture, "{0:C0}", Balance);
        }
        //Treemap: Expense by Category - Last 7 Days
        private async Task<List<object>> GetTreemapData()
        {
            // Ensure non-null return value
            var data = await GetTreemapDataImplementation();
            return data ?? new List<object>();
        }
        private async Task<List<object>> GetTreemapDataImplementation()
        {
            DateTime StartDate7Days = DateTime.UtcNow.Date.AddDays(-6);
            DateTime EndDate7Days = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions7Days = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate7Days && y.Date <= EndDate7Days && y.UserId == userId)
                .ToListAsync();

            var TreemapData = SelectedTransactions7Days
                .Where(i => i.Category?.Type == "Expense")
                .GroupBy(j => j.Category?.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category?.Icon + " " + k.First().Category?.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0")
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            return TreemapData.Cast<object>().ToList();
        }
        // Bar Chart: Income vs Expense - Last 7 Days
        private async Task<List<BarChartData>> GetBarChartData()
        {
            DateTime StartDate7Days = DateTime.UtcNow.Date.AddDays(-6);
            DateTime EndDate7Days = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions7Days = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate7Days && y.Date <= EndDate7Days && y.UserId == userId)
                .ToListAsync();

            List<BarChartData> BarChartIncomeSummary = SelectedTransactions7Days
                .Where(i => i.Category?.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new BarChartData()
                {
                    day = k.First().Date.ToString("MMM-dd-yyyy"),
                    income = k.Sum(l => l.Amount),
                    expense = 0
                })
                .ToList();

            List<BarChartData> BarChartExpenseSummary = SelectedTransactions7Days
                .Where(i => i.Category?.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new BarChartData()
                {
                    day = k.First().Date.ToString("MMM-dd-yyyy"),
                    income = 0,
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            var Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate7Days.AddDays(i).ToString("MMM-dd-yyyy"))
                .ToArray();

            var BarChartData = Last7Days
                .GroupJoin(BarChartIncomeSummary, day => day, income => income.day, (day, income) => new { day, income })
                .SelectMany(
                    x => x.income.DefaultIfEmpty(new BarChartData { day = x.day, income = 0, expense = 0 }),
                    (x, income) => new { x.day, income })
                .GroupJoin(BarChartExpenseSummary, x => x.day, expense => expense.day, (x, expense) => new { x.day, x.income, expense })
                .SelectMany(
                    x => x.expense.DefaultIfEmpty(new BarChartData { day = x.day, income = 0, expense = 0 }),
                    (x, expense) => new BarChartData
                    {
                        day = x.day,
                        income = x.income.income,
                        expense = expense.expense
                    })
                .ToList();

            return BarChartData;
        }
        // Stacked Column Chart: Income vs Expense - Last 30 Days
        private async Task<List<object>> GetStackedColumnChartData()
        {
            var data = await GetStackedColumnChartDataImplementation();
            return data ?? new List<object>();
        }
        private async Task<List<object>> GetStackedColumnChartDataImplementation()
        {
            DateTime StartDate30Days = DateTime.UtcNow.Date.AddDays(-29);
            DateTime EndDate30Days = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions30Days = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate30Days && y.Date <= EndDate30Days && y.UserId == userId)
                .ToListAsync();

            var StackedColumnChartData = SelectedTransactions30Days
                .GroupBy(t => new { t.Date, Type = t.Category?.Type ?? "Unknown" })
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

            return StackedColumnChartData.Cast<object>().ToList();
        }
        //Expense Distribution - Across Different Categories
        private async Task<RadarChartData> GetRadarChartData()
        {
            var data = await GetRadarChartDataImplementation();
            return data ?? new RadarChartData { ExpenseData = new List<RadarData>(), IncomeData = new List<RadarData>() };
        }
        private async Task<RadarChartData> GetRadarChartDataImplementation()
        {
            DateTime StartDate12Months = DateTime.UtcNow.Date.AddMonths(-11);
            DateTime EndDate12Months = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions12Months = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate12Months && y.Date <= EndDate12Months && y.UserId == userId)
                .ToListAsync();

            DateTime? earliestTransactionDate = SelectedTransactions12Months
                .OrderBy(t => t.Date)
                .Select(t => t.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue)
            {
                StartDate12Months = earliestTransactionDate.Value;
            }

            var expenseData = SelectedTransactions12Months
                .Where(t => t.Category?.Type == "Expense")
                .GroupBy(t => t.Category?.Title)
                .Select(g => new RadarData
                {
                    Category = g.Key ?? "Unknown",
                    Amount = g.Sum(t => t.Amount)
                })
                .ToList();

            var incomeData = SelectedTransactions12Months
                .Where(t => t.Category?.Type == "Income")
                .GroupBy(t => t.Category?.Title)
                .Select(g => new RadarData
                {
                    Category = g.Key ?? "Unknown",
                    Amount = g.Sum(t => t.Amount)
                })
                .ToList();

            return new RadarChartData
            {
                ExpenseData = expenseData,
                IncomeData = incomeData
            };
        }
        //Line Chart (3 Lines): Monthly Trend - Last 12 Months from First Entry
        private async Task<List<MonthlyTrendData>> GetMonthlyTrendChartData()
        {
            var data = await GetMonthlyTrendChartDataImplementation();
            return data ?? new List<MonthlyTrendData>();
        }
        private async Task<List<MonthlyTrendData>> GetMonthlyTrendChartDataImplementation()
        {
            DateTime StartDate12Months = DateTime.UtcNow.Date.AddMonths(-11);
            DateTime EndDate12Months = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions12Months = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate12Months && y.Date <= EndDate12Months && y.UserId == userId)
                .ToListAsync();

            DateTime? earliestTransactionDate = SelectedTransactions12Months
                .OrderBy(t => t.Date)
                .Select(t => t.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue)
            {
                StartDate12Months = earliestTransactionDate.Value;
            }

            List<MonthlyTrendData> MonthlyTrendChartData = new List<MonthlyTrendData>();

            for (DateTime date = StartDate12Months; date <= EndDate12Months; date = date.AddMonths(1))
            {
                int TotalIncome = SelectedTransactions12Months
                    .Where(i => i.Category?.Type == "Income" && i.Date.Month == date.Month && i.Date.Year == date.Year)
                    .Sum(j => j.Amount);

                int TotalExpense = SelectedTransactions12Months
                    .Where(i => i.Category?.Type == "Expense" && i.Date.Month == date.Month && i.Date.Year == date.Year)
                    .Sum(j => j.Amount);

                int Balance = TotalIncome - TotalExpense;

                MonthlyTrendChartData.Add(new MonthlyTrendData
                {
                    Month = date.ToString("MMM yyyy"),
                    Income = TotalIncome,
                    Expense = TotalExpense,
                    Balance = Balance
                });
            }

            return MonthlyTrendChartData;
        }
        
        
        //Staked Area Chart: Income vs Expense - Last 12 Months from First Entry
        private async Task<List<object>> GetStackedAreaChartData()
        {
            var data = await GetStackedAreaChartDataImplementation();
            return data ?? new List<object>();
        }
        private async Task<List<object>> GetStackedAreaChartDataImplementation()
        {
            DateTime StartDate12Months = DateTime.UtcNow.Date.AddMonths(-11);
            DateTime EndDate12Months = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions12Months = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate12Months && y.Date <= EndDate12Months && y.UserId == userId)
                .ToListAsync();

            DateTime? earliestTransactionDate = SelectedTransactions12Months
                .OrderBy(t => t.Date)
                .Select(t => t.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue && earliestTransactionDate.Value < StartDate12Months)
            {
                StartDate12Months = new DateTime(earliestTransactionDate.Value.Year, earliestTransactionDate.Value.Month, 1);
            }

            var StackedAreaChartData = SelectedTransactions12Months
                .Where(t => t.Date >= StartDate12Months)
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

            return StackedAreaChartData.Cast<object>().ToList();
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
        public string day;
        public int income;
        public int expense;
    }

    public class RadarChartData
    {
        public List<RadarData> ExpenseData { get; set; }
        public List<RadarData> IncomeData { get; set; }
    }

    public class RadarData
    {
        public string Category { get; set; }
        public int Amount { get; set; }
    }
}