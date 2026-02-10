using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface IExpenseService
    {
        Task<List<Expense>> GetAllAsync(int userId);
        Task<decimal> GetTodayExpenseAsync(int userId);
        Task<decimal> GetTotalExpenseAsync(int userId);
        Task AddAsync(Expense expense);
        Task UpdateAsync(Expense expense);
        Task DeleteAsync(int id);
        Task<Dictionary<string, decimal>> GetExpenseByCategory(int userId);
    }

    public class ExpenseService : IExpenseService
    {
        private readonly ApplicationDbContext _context;

        public ExpenseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Expense>> GetAllAsync(int userId)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTodayExpenseAsync(int userId)
        {
            var today = DateTime.Now.Date;
            return await _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Date == today)
                .SumAsync(e => e.Amount);
        }

        public async Task<decimal> GetTotalExpenseAsync(int userId)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId)
                .SumAsync(e => e.Amount);
        }

        public async Task AddAsync(Expense expense)
        {
            expense.CreatedDate = DateTime.Now;
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Expense expense)
        {
            var existing = await _context.Expenses.FindAsync(expense.Id);
            if (existing == null)
            {
                return;
            }

            existing.Date = expense.Date;
            existing.Category = expense.Category;
            existing.Amount = expense.Amount;
            existing.Description = expense.Description;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<string, decimal>> GetExpenseByCategory(int userId)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId)
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                .ToDictionaryAsync(x => x.Category, x => x.Total);
        }
    }
}
