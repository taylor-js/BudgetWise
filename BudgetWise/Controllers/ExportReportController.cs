using BudgetWise.Areas.Identity.Data;
using BudgetWise.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System.Data;

namespace BudgetWise.Controllers
{
	[Authorize]
	public class ExportReportController : Controller
	{
		private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExportReportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Excel()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var firstName = user.FirstName;
            var lastName = user.LastName;

            var userId = _userManager.GetUserId(User);
            var today = DateTime.Now.Date; // Use local time kind to avoid time zone issues
            var transactions = await _context.Transactions
                .Include(t => t.Category) // Ensure Category is loaded to avoid null references
                .Where(d => d.Date <= today && d.UserId == userId)
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                transaction.Date = DateTime.SpecifyKind(transaction.Date, DateTimeKind.Unspecified);
            }

            var fileName = $"BudgetWise-Report-{firstName}{lastName}{DateTime.Today:yyyyMMdd}.xlsx";
            return GenerateExcel(fileName, transactions);
        }

        private FileResult GenerateExcel(string fileName, IEnumerable<Transaction> transactions)
        {
            DataTable tDataTable = new DataTable("Transaction");
            tDataTable.Columns.AddRange(new DataColumn[]
            {
                // Removed TransactionId and CategoryId from output
                new DataColumn("Title", typeof(string)),
                new DataColumn("Amount", typeof(string)),
                new DataColumn("Type", typeof(string)),
                new DataColumn("Note", typeof(string)),
                new DataColumn("Date", typeof(DateTime))
            });

            foreach (var transaction in transactions)
            {
                tDataTable.Rows.Add(
                    transaction.TitleWithoutEmoji,
                    transaction.FormattedAmount,
                    transaction.Category?.Type,
                    transaction.Note,
                    transaction.Date
                );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(tDataTable);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
        }

    }
}
