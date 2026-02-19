

using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface IReportService
    {
        Task<decimal> GetNetBalanceAsync(int userId);
        Task<List<Income>> GetDailyIncomeReportAsync(int userId, DateTime date);
        Task<decimal> GetProfitLossAsync(int userId);
          // ADD THIS
        Task<decimal> GetTotalReceivablesAsync(int userId);
        Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int userId, int count);
        Task<List<ChartDataPoint>> GetDailyTotalsAsync(int userId, int days);
    
        
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IIncomeService _incomeService;
        private readonly IExpenseService _expenseService;

        public ReportService(ApplicationDbContext context, IIncomeService incomeService, IExpenseService expenseService)
        {
            _context = context;
            _incomeService = incomeService;
            _expenseService = expenseService;
        }

        public async Task<decimal> GetNetBalanceAsync(int userId)
        {
            var totalIncome = await _incomeService.GetTotalIncomeAsync(userId);
            var totalExpense = await _expenseService.GetTotalExpenseAsync(userId);
            return totalIncome - totalExpense;
        }

        public async Task<List<Income>> GetDailyIncomeReportAsync(int userId, DateTime date)
        {
            return await _context.Incomes
                .Include(i => i.Customer)
                .Where(i => i.UserId == userId && i.Date.HasValue && i.Date.Value.Date == date.Date)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetProfitLossAsync(int userId)
        {
            var totalIncome = await _incomeService.GetTotalIncomeAsync(userId);
            var totalExpense = await _expenseService.GetTotalExpenseAsync(userId);
            return totalIncome - totalExpense;
        }
        public async Task<decimal> GetTotalReceivablesAsync(int userId)
        {
            // Sum of remaining amounts from all unpaid sales
            return await _context.Sales
                .Where(s => s.UserId == userId && !s.IsPaid)
                .SumAsync(s => s.RemainingAmount);
        }

        public async Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int userId, int count)
        {
            var incomes = await _context.Incomes
                .Include(i => i.Customer)
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Date)
                .Take(count)
                .Select(i => new RecentTransactionDto
                {
                    Id = i.Id,
                    Date = i.Date ?? DateTime.MinValue,
                    Description = i.Description ?? "No description",
                    Amount = i.Amount, // Assuming Amount is not nullable or handled
                    Type = "Income",
                    Category = i.PaymentType ?? "General",
                    EntityName = i.Customer != null ? (i.Customer.Name ?? "Unknown") : "General"
                })
                .ToListAsync();

            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(count)
                .Select(e => new RecentTransactionDto
                {
                    Id = e.Id,
                    Date = e.Date, // Assuming e.Date is DateTime (non-nullable) or nullable
                    Description = e.Description ?? "No description",
                    Amount = e.Amount,
                    Type = "Expense",
                    Category = e.Category ?? "General",
                    EntityName = "Vendor"
                })
                .ToListAsync();

            return incomes.Concat(expenses)
                .OrderByDescending(t => t.Date)
                .Take(count)
                .ToList();
        }

        public async Task<List<ChartDataPoint>> GetDailyTotalsAsync(int userId, int days)
        {
             var startDate = DateTime.Today.AddDays(-days);

            var incomeData = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date >= startDate)
                .GroupBy(i => i.Date.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            var result = new List<ChartDataPoint>();
            for (var i = 0; i <= days; i++)
            {
                var date = startDate.AddDays(i);
                var dayData = incomeData.FirstOrDefault(d => d.Date == date);
                result.Add(new ChartDataPoint
                {
                    Label = date.ToString("MMM dd"),
                    Value = dayData?.Total ?? 0
                });
            }

            return result;
        }
    }
}