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
            ViewBag.TreemapData = await GetTreemapData();
            ViewBag.BarChartData = await GetBarChartData();
            ViewBag.HeatmapChartData = await GetHeatmapChartData();
            ViewBag.MonthlyTrendChartData = await GetMonthlyTrendChartData();
            ViewBag.RadarChartData = await GetRadarChartData();
            ViewBag.StackedColumnChartData = await GetStackedColumnChartData();
            ViewBag.StackedAreaChartData = await GetStackedAreaChartData();

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
        private async Task<object> GetTreemapData()
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

            return TreemapData;
        }
        //Bar Chart: Income vs Expense - Last 7 Days
        private async Task<object> GetBarChartData()
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
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            List<BarChartData> BarChartExpenseSummary = SelectedTransactions7Days
                .Where(i => i.Category?.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new BarChartData()
                {
                    day = k.First().Date.ToString("MMM-dd-yyyy"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate7Days.AddDays(i).ToString("MMM-dd-yyyy"))
                .ToArray();

            var BarChartData = from day in Last7Days
                               join income in BarChartIncomeSummary on day equals income.day into dayIncomeJoined
                               from income in dayIncomeJoined.DefaultIfEmpty()
                               join expense in BarChartExpenseSummary on day equals expense.day into expenseJoined
                               from expense in expenseJoined.DefaultIfEmpty()
                               select new
                               {
                                   day = day,
                                   income = income == null ? 0 : income.income,
                                   expense = expense == null ? 0 : expense.expense,
                               };

            return BarChartData;
        }
        // Heatmap: Transaction Amounts - By Category and Month from First Entry
        private async Task<HeatmapData> GetHeatmapChartData()
        {
            DateTime StartDate12Months = DateTime.UtcNow.Date.AddMonths(-11);
            DateTime EndDate12Months = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions12Months = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate12Months && y.Date <= EndDate12Months && y.UserId == userId)
                .ToListAsync();

            var heatmapData = SelectedTransactions12Months
                .GroupBy(t => new { Month = t.Date.ToString("MMM yyyy"), Category = t.Category?.Title ?? "Unknown" })
                .Select(g => new { Month = g.Key.Month, Category = g.Key.Category, Amount = g.Sum(t => t.Amount) })
                .ToList();

            string[] xLabels = heatmapData.Select(d => d.Month).Distinct().ToArray();
            string[] yLabels = heatmapData.Select(d => d.Category).Distinct().ToArray();

            int[,] data = new int[xLabels.Length, yLabels.Length];
            foreach (var item in heatmapData)
            {
                int xIndex = Array.IndexOf(xLabels, item.Month);
                int yIndex = Array.IndexOf(yLabels, item.Category);
                data[xIndex, yIndex] = (int)item.Amount; // Convert to integer
            }

            var formattedHeatmapData = new HeatmapData
            {
                Data = data,
                XLabels = xLabels,
                YLabels = yLabels,
            };

            return formattedHeatmapData;
        }
        //Line Chart (3 Lines): Monthly Trend - Last 12 Months from First Entry
        private async Task<List<MonthlyTrendData>> GetMonthlyTrendChartData()
        {
            DateTime StartDate12Months = DateTime.UtcNow.Date.AddMonths(-11);
            DateTime EndDate12Months = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions12Months = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate12Months && y.Date <= EndDate12Months && y.UserId == userId)
                .ToListAsync();

            // Find the earliest month with transactions
            DateTime? earliestTransactionDate = SelectedTransactions12Months
                .OrderBy(t => t.Date)
                .Select(t => t.Date)
                .FirstOrDefault();

            // If there is no transaction in the last 12 months, keep the start date as is
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

        //Expense Distribution - Across Different Categories
        private async Task<RadarChartData> GetRadarChartData()
        {
            DateTime StartDate12Months = DateTime.UtcNow.Date.AddMonths(-11);
            DateTime EndDate12Months = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions12Months = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate12Months && y.Date <= EndDate12Months && y.UserId == userId)
                .ToListAsync();

            // Ensure the date range covers transactions
            DateTime? earliestTransactionDate = SelectedTransactions12Months
                .OrderBy(t => t.Date)
                .Select(t => t.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue)
            {
                StartDate12Months = earliestTransactionDate.Value;
            }

            // Separate and aggregate data
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

            var radarChartData = new RadarChartData
            {
                ExpenseData = expenseData,
                IncomeData = incomeData
            };

            return radarChartData;
        }

        // Stacked Column Chart: Income vs Expense - Last 30 Days
        private async Task<object> GetStackedColumnChartData()
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

            return StackedColumnChartData;
        }
        //Staked Area Chart: Income vs Expense - Last 12 Months from First Entry
        private async Task<object> GetStackedAreaChartData()
        {
            DateTime StartDate12Months = DateTime.UtcNow.Date.AddMonths(-11);
            DateTime EndDate12Months = DateTime.UtcNow.Date;
            string userId = _userManager.GetUserId(User) ?? string.Empty;

            List<Transaction> SelectedTransactions12Months = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate12Months && y.Date <= EndDate12Months && y.UserId == userId)
                .ToListAsync();

            // Find the earliest month with transactions
            DateTime? earliestTransactionDate = SelectedTransactions12Months
                .OrderBy(t => t.Date)
                .Select(t => t.Date)
                .FirstOrDefault();

            // If there is an earlier transaction date, adjust the start date
            if (earliestTransactionDate.HasValue && earliestTransactionDate.Value < StartDate12Months)
            {
                StartDate12Months = new DateTime(earliestTransactionDate.Value.Year, earliestTransactionDate.Value.Month, 1);
            }

            var StackedAreaChartData = SelectedTransactions12Months
                .Where(t => t.Date >= StartDate12Months) // Filter transactions from the adjusted start date
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

            return StackedAreaChartData;
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

    public class HeatmapData
    {
        public int[,] Data { get; set; }
        public string[] XLabels { get; set; }
        public string[] YLabels { get; set; }
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