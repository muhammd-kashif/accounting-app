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
    }
}
