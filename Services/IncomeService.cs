using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface IIncomeService
    {
        Task<List<Income>> GetAllAsync(int userId);
        Task<decimal> GetTodayIncomeAsync(int userId);
        Task<decimal> GetTotalIncomeAsync(int userId);
        Task AddAsync(Income income);
        Task UpdateAsync(Income income);
        Task DeleteAsync(int id);
        // New method for Receivables
        Task<decimal> GetTotalReceivablesAsync(int userId);
    }

    public class IncomeService : IIncomeService
    {
        private readonly ApplicationDbContext _context;

        public IncomeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Income>> GetAllAsync(int userId)
        {
            return await _context.Incomes
                .Include(i => i.Customer)
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Date ?? DateTime.MinValue)
                .ToListAsync();
        }

        public async Task<decimal> GetTodayIncomeAsync(int userId)
        {
            var today = DateTime.Now.Date;
            return await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.HasValue && i.Date.Value.Date == today)
                .SumAsync(i => i.Amount);
        }

        public async Task<decimal> GetTotalIncomeAsync(int userId)
        {
            return await _context.Incomes
                .Where(i => i.UserId == userId)
                .SumAsync(i => i.Amount);
        }

        public async Task AddAsync(Income income)
        {
            income.CreatedDate = DateTime.Now;
            _context.Incomes.Add(income);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Income income)
        {
            var existing = await _context.Incomes.FindAsync(income.Id);
            if (existing == null)
            {
                return;
            }

            existing.Date = income.Date;
            existing.CustomerId = income.CustomerId;
            existing.Amount = income.Amount;
            existing.PaymentType = income.PaymentType;
            existing.Description = income.Description;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var income = await _context.Incomes.FindAsync(id);
            if (income != null)
            {
                _context.Incomes.Remove(income);
                await _context.SaveChangesAsync();
            }
        }

         public async Task<decimal> GetTotalReceivablesAsync(int userId)
        {
            return await _context.Incomes
                .Where(i => i.UserId == userId && !i.IsPaid) // IsPaid = false means udhaar
                .SumAsync(i => i.Amount);
        }

    }
}
