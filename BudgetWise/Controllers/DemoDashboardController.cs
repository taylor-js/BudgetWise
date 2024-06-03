using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BudgetWise.Areas.Identity.Data;
using BudgetWise.Models;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Syncfusion.EJ2.Navigations;

namespace BudgetWise.Controllers
{
    public class DemoDashboardController : Controller
    {
        private readonly ILogger<DemoDashboardController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public DemoDashboardController(ILogger<DemoDashboardController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Demo()
        {
            ViewData["isDashboard"] = null;
            if (User is not null && User.Identity is not null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                return RedirectToAction("Index", "Dashboard", new { userId = user?.Id });
            }

            var demoCategories = GetDemoCategories();
            var demoTransactions = GenerateDemoTransactions(500, demoCategories);

            ViewBag.TotalIncome = CalculateTotalDemoIncome(demoTransactions);
            ViewBag.TotalExpense = CalculateTotalDemoExpense(demoTransactions);
            ViewBag.Balance = CalculateDemoBalance(demoTransactions);
            ViewBag.TreemapData = GetDemoTreemapData(demoTransactions);
            ViewBag.BarChartData = GetDemoBarChartData(demoTransactions);
            ViewBag.MonthlyTrendChartData = GetDemoMonthlyTrendChartData(demoTransactions);
            ViewBag.BubbleChartData = GetDemoBubbleChartData(demoTransactions);
            ViewBag.StackedColumnChartData = GetDemoStackedColumnChartData(demoTransactions);
            ViewBag.StackedAreaChartData = GetDemoStackedAreaChartData(demoTransactions);

            return View("Demo");
        }

        private List<Category> GetDemoCategories()
        {
            return new List<Category>
            {
                new Category { CategoryId = 15, Title = "Dividends from stocks/bonds", Icon = "📈", Type = "Income", UserId = "demo-user" ,  MinAmount = 10, MaxAmount = 500 },
                new Category { CategoryId = 16, Title = "Electricity", Icon = "💡", Type = "Expense", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 150 },
                new Category { CategoryId = 17, Title = "Emergency fund", Icon = "🚑", Type = "Expense", UserId = "demo-user" ,  MinAmount = 100, MaxAmount = 1000 },
                new Category { CategoryId = 18, Title = "Family gifts", Icon = "🎁", Type = "Income", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 500 },
                new Category { CategoryId = 19, Title = "Freelancing/Consulting fees", Icon = "💼", Type = "Income", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 500 },
                new Category { CategoryId = 20, Title = "Gas utilities", Icon = "💧", Type = "Expense", UserId = "demo-user" ,  MinAmount = 30, MaxAmount = 150 },
                new Category { CategoryId = 21, Title = "Gifts for family and friends", Icon = "🎁", Type = "Expense", UserId = "demo-user" ,  MinAmount = 200, MaxAmount = 500 },
                new Category { CategoryId = 22, Title = "Groceries", Icon = "🛒", Type = "Expense", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 200 },
                new Category { CategoryId = 23, Title = "Gym memberships", Icon = "🏋️", Type = "Expense", UserId = "demo-user" ,  MinAmount = 20, MaxAmount = 100 },
                new Category { CategoryId = 24, Title = "Haircuts/haircare", Icon = "💇", Type = "Expense", UserId = "demo-user" ,  MinAmount = 20, MaxAmount = 100 },
                new Category { CategoryId = 25, Title = "Health insurance premiums", Icon = "🏥", Type = "Expense", UserId = "demo-user" ,  MinAmount = 100, MaxAmount = 500 },
                new Category { CategoryId = 26, Title = "Hobbies", Icon = "🎨", Type = "Expense", UserId = "demo-user" ,  MinAmount = 20, MaxAmount = 200 },
                new Category { CategoryId = 27, Title = "Home insurance", Icon = "🏡", Type = "Expense", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 300 },
                new Category { CategoryId = 28, Title = "Home repairs and maintenance", Icon = "🔧", Type = "Expense", UserId = "demo-user" ,  MinAmount = 100, MaxAmount = 1000 },
                new Category { CategoryId = 29, Title = "Insurance settlements", Icon = "📃", Type = "Income", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 300 },
                new Category { CategoryId = 30, Title = "Interest from savings accounts", Icon = "💰", Type = "Income", UserId = "demo-user" ,  MinAmount = 1, MaxAmount = 100 },
                new Category { CategoryId = 31, Title = "Internet/cable", Icon = "📡", Type = "Expense", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 150 },
                new Category { CategoryId = 32, Title = "Life insurance premiums", Icon = "🏥", Type = "Expense", UserId = "demo-user" ,  MinAmount = 20, MaxAmount = 100 },
                new Category { CategoryId = 33, Title = "Meal delivery services", Icon = "🍔", Type = "Expense", UserId = "demo-user" ,  MinAmount = 10, MaxAmount = 50 },
                new Category { CategoryId = 34, Title = "Medical copays", Icon = "🏥", Type = "Expense", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 300 },
                new Category { CategoryId = 35, Title = "Morning coffee", Icon = "☕", Type = "Expense", UserId = "demo-user" ,  MinAmount = 2, MaxAmount = 10 },
                new Category { CategoryId = 36, Title = "Mortgage/rent", Icon = "🏡", Type = "Expense", UserId = "demo-user" ,  MinAmount = 500, MaxAmount = 2000 },
                new Category { CategoryId = 37, Title = "Movie/concert tickets", Icon = "🎫", Type = "Expense", UserId = "demo-user" ,  MinAmount = 10, MaxAmount = 50 },
                new Category { CategoryId = 38, Title = "Overtime pay", Icon = "⏰", Type = "Income", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 500 },
                new Category { CategoryId = 39, Title = "Parking fees", Icon = "🅿️", Type = "Expense", UserId = "demo-user" ,  MinAmount = 5, MaxAmount = 20 },
                new Category { CategoryId = 40, Title = "Personal loans", Icon = "🏦", Type = "Expense", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 500 },
                new Category { CategoryId = 41, Title = "Phone bill", Icon = "📞", Type = "Expense", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 150 },
                new Category { CategoryId = 42, Title = "Primary job salary", Icon = "💼", Type = "Income", UserId = "demo-user" ,  MinAmount = 500, MaxAmount = 2000 },
                new Category { CategoryId = 43, Title = "Profit from owned businesses", Icon = "🏢", Type = "Income", UserId = "demo-user" ,  MinAmount = 100, MaxAmount = 5000 },
                new Category { CategoryId = 44, Title = "Property taxes", Icon = "🏡", Type = "Expense", UserId = "demo-user" ,  MinAmount = 500, MaxAmount = 3000 },
                new Category { CategoryId = 45, Title = "Public transportation", Icon = "🚌", Type = "Expense", UserId = "demo-user" ,  MinAmount = 2, MaxAmount = 20 },
                new Category { CategoryId = 46, Title = "Real estate rental income", Icon = "🏢", Type = "Income", UserId = "demo-user" ,  MinAmount = 100, MaxAmount = 1000 },
                new Category { CategoryId = 47, Title = "Retirement contributions (401(k), IRA)", Icon = "💹", Type = "Expense", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 500 },
                new Category { CategoryId = 48, Title = "Savings account deposits", Icon = "🏦", Type = "Expense", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 500 },
                new Category { CategoryId = 49, Title = "School supplies", Icon = "✏️", Type = "Expense", UserId = "demo-user" ,  MinAmount = 10, MaxAmount = 100 },
                new Category { CategoryId = 50, Title = "School tuition", Icon = "🏫", Type = "Expense", UserId = "demo-user" ,  MinAmount = 500, MaxAmount = 2000 },
                new Category { CategoryId = 51, Title = "Side gigs", Icon = "💼", Type = "Income", UserId = "demo-user" ,  MinAmount = 50, MaxAmount = 500 },
                new Category { CategoryId = 52, Title = "Streaming subscriptions", Icon = "📺", Type = "Expense", UserId = "demo-user" ,  MinAmount = 10, MaxAmount = 50 },
                new Category { CategoryId = 53, Title = "Student loan payments", Icon = "🎓", Type = "Expense", UserId = "demo-user" ,  MinAmount = 100, MaxAmount = 500 },
                new Category { CategoryId = 54, Title = "Tax refunds", Icon = "💵", Type = "Income", UserId = "demo-user" ,  MinAmount = 100, MaxAmount = 2000 },
                new Category { CategoryId = 55, Title = "Travel/vacations", Icon = "✈️", Type = "Expense", UserId = "demo-user" ,  MinAmount = 100, MaxAmount = 2000 },
                new Category { CategoryId = 56, Title = "Water bill", Icon = "💧", Type = "Expense", UserId = "demo-user" , MinAmount = 20, MaxAmount = 100 },

            };
        }

        private List<Transaction> GenerateDemoTransactions(int count, List<Category> categories, int maxTransactionsPerDay = 10)
        {
            var random = new Random();
            var transactions = new List<Transaction>();
            var transactionCounts = new Dictionary<DateTime, int>();

            var incomeCategories = categories.Where(c => c.Type == "Income").ToList();
            var expenseCategories = categories.Where(c => c.Type == "Expense").ToList();

            decimal totalIncome = 0;
            decimal totalExpense = 0;

            var usedCategories = new HashSet<int>();
            var last7DaysCategories = new Dictionary<DateTime, HashSet<int>>();

            void AddTransaction(Category category, decimal amount, DateTime date)
            {
                transactions.Add(new Transaction
                {
                    TransactionId = transactions.Count + 1,
                    CategoryId = category.CategoryId,
                    Category = category,
                    Amount = (int)amount,
                    Note = $"Demo note {transactions.Count + 1}",
                    Date = date.Date, // Remove time component
                    UserId = "demo-user"
                });

                usedCategories.Add(category.CategoryId);

                if (!last7DaysCategories.ContainsKey(date))
                {
                    last7DaysCategories[date] = new HashSet<int>();
                }

                last7DaysCategories[date].Add(category.CategoryId);
            }

            bool generateIncomeNext = true;

            for (int i = 0; i < count; i++)
            {
                Category category;
                DateTime date;
                int attempts = 0;

                do
                {
                    category = generateIncomeNext ? incomeCategories[random.Next(incomeCategories.Count)] : expenseCategories[random.Next(expenseCategories.Count)];
                    date = DateTime.Today.AddDays(-random.Next(0, 366)); // Use DateTime.Today and include today
                    attempts++;

                    if (attempts > 2000) // Increase the attempts limit
                    {
                        category = generateIncomeNext ? incomeCategories[random.Next(incomeCategories.Count)] : expenseCategories[random.Next(expenseCategories.Count)];
                        break;
                    }
                } while ((transactionCounts.ContainsKey(date) && transactionCounts[date] >= maxTransactionsPerDay) || usedCategories.Contains(category.CategoryId));

                if (!transactionCounts.ContainsKey(date))
                {
                    transactionCounts[date] = 0;
                }

                transactionCounts[date]++;

                var amount = random.Next(category.MinAmount, category.MaxAmount + 1);

                if (category.Type == "Expense" && totalExpense + amount >= totalIncome)
                {
                    amount = (int)(totalIncome - totalExpense - 1);
                }

                AddTransaction(category, amount, date);

                if (category.Type == "Income")
                {
                    totalIncome += amount;
                    generateIncomeNext = false;
                }
                else
                {
                    totalExpense += amount;
                    generateIncomeNext = true;
                }
            }

            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).ToList(); // Use DateTime.Today
            var allUsedCategories = new HashSet<int>(last7Days.SelectMany(date => last7DaysCategories.ContainsKey(date) ? last7DaysCategories[date] : new HashSet<int>()));

            while (allUsedCategories.Count < 10)
            {
                var category = categories[random.Next(categories.Count)];
                if (!allUsedCategories.Contains(category.CategoryId))
                {
                    var amount = random.Next(category.MinAmount, category.MaxAmount + 1);
                    var date = last7Days[random.Next(last7Days.Count)];

                    if (transactionCounts.ContainsKey(date) && transactionCounts[date] >= maxTransactionsPerDay)
                    {
                        continue;
                    }

                    if (!transactionCounts.ContainsKey(date))
                    {
                        transactionCounts[date] = 0;
                    }

                    transactionCounts[date]++;
                    AddTransaction(category, amount, date);

                    if (category.Type == "Income")
                    {
                        totalIncome += amount;
                    }
                    else
                    {
                        totalExpense += amount;
                    }

                    allUsedCategories.Add(category.CategoryId);
                }
            }

            if (totalExpense == 0 || totalIncome <= totalExpense)
            {
                var category = incomeCategories[random.Next(incomeCategories.Count)];
                var amount = random.Next(category.MinAmount, category.MaxAmount + 1);
                var date = DateTime.Today.AddDays(-random.Next(0, 366)); // Use DateTime.Today and include today

                AddTransaction(category, amount, date);
                totalIncome += amount;
            }

            return transactions;
        }

        private string CalculateTotalDemoIncome(List<Transaction> transactions)
        {
            DateTime? earliestDate = transactions.OrderBy(t => t.Date).FirstOrDefault()?.Date;

            if (earliestDate == null)
            {
                return 0.ToString("C0");
            }

            DateTime startDate = earliestDate.Value;
            DateTime endDate = DateTime.Today;

            int totalIncome = transactions
                .Where(i => i.Date.Date >= startDate && i.Date.Date <= endDate && i.Category?.Type == "Income")
                .Sum(j => j.Amount);

            return totalIncome.ToString("C0");
        }

        private string CalculateTotalDemoExpense(List<Transaction> transactions)
        {
            DateTime? earliestDate = transactions.OrderBy(t => t.Date).FirstOrDefault()?.Date;

            if (earliestDate == null)
            {
                return 0.ToString("C0");
            }

            DateTime startDate = earliestDate.Value;
            DateTime endDate = DateTime.Today;

            int totalExpense = transactions
                .Where(i => i.Date.Date >= startDate && i.Date.Date <= endDate && i.Category?.Type == "Expense")
                .Sum(j => j.Amount);

            return totalExpense.ToString("C0");
        }

        private string CalculateDemoBalance(List<Transaction> transactions)
        {
            DateTime? earliestDate = transactions.OrderBy(t => t.Date).FirstOrDefault()?.Date;

            if (earliestDate == null)
            {
                return 0.ToString("C0");
            }

            DateTime startDate = earliestDate.Value;
            DateTime endDate = DateTime.Today;

            int totalIncome = transactions
                .Where(i => i.Date.Date >= startDate && i.Date.Date <= endDate && i.Category?.Type == "Income")
                .Sum(j => j.Amount);

            int totalExpense = transactions
                .Where(i => i.Date.Date >= startDate && i.Date.Date <= endDate && i.Category?.Type == "Expense")
                .Sum(j => j.Amount);

            int balance = totalIncome - totalExpense;

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;

            return String.Format(culture, "{0:C0}", balance);
        }

        //Expense by Category - Last 7 Days
        private List<object> GetDemoTreemapData(List<Transaction> transactions)
        {
            DateTime startDate = DateTime.Today.AddDays(-6);
            DateTime endDate = DateTime.Today;

            var treemapData = transactions
                .Where(t => t.Date.Date >= startDate && t.Date.Date <= endDate && t.Category?.Type == "Expense")
                .GroupBy(j => j.Category?.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category?.Icon + " " + k.First().Category?.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0")
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            return treemapData.Cast<object>().ToList();
        }

        // Income vs Expense - Last 7 Days - Bar chart
        private List<BarChartData> GetDemoBarChartData(List<Transaction> transactions)
        {
            DateTime startDate = DateTime.Today.AddDays(-6); // Last 7 days including today
            DateTime endDate = DateTime.Today; // Include today

            // Logging for debugging
            Console.WriteLine($"Demo Bar Chart - Start Date: {startDate}, End Date: {endDate}");

            var barChartData = transactions
                .Where(y => y.Date.Date >= startDate && y.Date.Date <= endDate)
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

            return barChartData;
        }

        // Income vs Expense - Last 30 Days
        private List<object> GetDemoStackedColumnChartData(List<Transaction> transactions)
        {
            DateTime startDate = DateTime.Today.AddDays(-29); // Last 30 days including today
            DateTime endDate = DateTime.Today; // Include today

            // Logging for debugging
            Console.WriteLine($"Demo Stacked Column Chart - Start Date: {startDate}, End Date: {endDate}");

            var stackedColumnChartData = transactions
                .Where(t => t.Date.Date >= startDate && t.Date.Date <= endDate)
                .GroupBy(t => new { t.Date.Date, Type = t.Category?.Type ?? "Unknown" })
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


        // Income & Expense - Last 12 Months From First Entry - Bubble chart
        private List<BubbleChartData> GetDemoBubbleChartData(List<Transaction> transactions)
        {
            DateTime startDate = DateTime.Today.AddMonths(-11);
            DateTime endDate = DateTime.Today;

            DateTime? earliestTransactionDate = transactions
                .OrderBy(t => t.Date)
                .Select(t => t.Date.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue)
            {
                startDate = earliestTransactionDate.Value;
            }

            var bubbleChartData = transactions
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

            return bubbleChartData;
        }

        // Monthly Trend - Last 12 Months from First Entry
        private List<MonthlyTrendData> GetDemoMonthlyTrendChartData(List<Transaction> transactions)
        {
            DateTime startDate = DateTime.Today.AddMonths(-11);
            DateTime endDate = DateTime.Today;

            DateTime? earliestTransactionDate = transactions
                .OrderBy(t => t.Date)
                .Select(t => t.Date.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue && earliestTransactionDate.Value < startDate)
            {
                startDate = new DateTime(earliestTransactionDate.Value.Year, earliestTransactionDate.Value.Month, 1);
            }

            List<MonthlyTrendData> monthlyTrendChartData = new List<MonthlyTrendData>();

            for (DateTime date = startDate; date <= endDate; date = date.AddMonths(1))
            {
                int totalIncome = transactions
                    .Where(i => i.Category?.Type == "Income" && i.Date.Year == date.Year && i.Date.Month == date.Month)
                    .Sum(j => j.Amount);

                int totalExpense = transactions
                    .Where(i => i.Category?.Type == "Expense" && i.Date.Year == date.Year && i.Date.Month == date.Month)
                    .Sum(j => j.Amount);

                int balance = totalIncome - totalExpense;

                monthlyTrendChartData.Add(new MonthlyTrendData
                {
                    Month = date.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Income = totalIncome,
                    Expense = totalExpense,
                    Balance = balance
                });
            }

            return monthlyTrendChartData;
        }

        //Income vs Expense - Last 12 Months from First Entry
        private List<object> GetDemoStackedAreaChartData(List<Transaction> transactions)
        {
            DateTime startDate = DateTime.Today.AddMonths(-11);
            DateTime endDate = DateTime.Today;

            DateTime? earliestTransactionDate = transactions
                .OrderBy(t => t.Date)
                .Select(t => t.Date.Date)
                .FirstOrDefault();

            if (earliestTransactionDate.HasValue && earliestTransactionDate.Value < startDate)
            {
                startDate = new DateTime(earliestTransactionDate.Value.Year, earliestTransactionDate.Value.Month, 1);
            }

            var stackedAreaChartData = transactions
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

            return stackedAreaChartData.Cast<object>().ToList();
        }

    }

}
