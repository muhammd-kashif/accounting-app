using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface ISaleService
    {
        Task<List<Sale>> GetAllAsync(int userId);
        Task<List<Sale>> GetByCustomerIdAsync(int customerId);
        Task<Sale?> GetByIdAsync(int id);
        Task<Sale?> GetBySaleNumberAsync(string saleNumber);
        Task<int> CreateSaleAsync(Sale sale, List<SaleItem> items);
        Task UpdateAsync(Sale sale);
        Task DeleteAsync(int id);
        Task<string> GenerateSaleNumberAsync();
    }

    public class SaleService : ISaleService
    {
        private readonly ApplicationDbContext _context;

        public SaleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Sale>> GetAllAsync(int userId)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Item)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<List<Sale>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Item)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<Sale?> GetByIdAsync(int id)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Sale?> GetBySaleNumberAsync(string saleNumber)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber);
        }

        public async Task<int> CreateSaleAsync(Sale sale, List<SaleItem> items)
        {
            // Generate sale number if not provided
            if (string.IsNullOrEmpty(sale.SaleNumber))
            {
                sale.SaleNumber = await GenerateSaleNumberAsync();
            }

            // Calculate total amount
            sale.TotalAmount = items.Sum(i => i.TotalPrice);
            sale.RemainingAmount = sale.TotalAmount - sale.PaidAmount;
            sale.IsPaid = sale.RemainingAmount <= 0;
            sale.CreatedDate = DateTime.Now;

            // Add sale
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Add sale items
            foreach (var item in items)
            {
                item.SaleId = sale.Id;
                _context.SaleItems.Add(item);

                // Update stock
                var product = await _context.Items.FindAsync(item.ItemId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    product.UpdatedDate = DateTime.Now;
                }
            }

            // --- STEP 4 Sync: Create Income Record ---
            if (sale.PaidAmount > 0)
            {
                var income = new Income
                {
                    Date = sale.SaleDate,
                    CustomerId = sale.CustomerId,
                    Amount = sale.PaidAmount,
                    PaymentType = sale.PaymentType,
                    Description = $"Sale Income: {sale.SaleNumber}",
                    UserId = sale.UserId,
                    SaleId = sale.Id,
                    CreatedDate = DateTime.Now
                };
                _context.Incomes.Add(income);
            }

            await _context.SaveChangesAsync();
            return sale.Id;
        }

        public async Task UpdateAsync(Sale sale)
        {
            var existing = await _context.Sales.FindAsync(sale.Id);
            if (existing == null) return;

            decimal oldPaidAmount = existing.PaidAmount;
            
            existing.SaleDate = sale.SaleDate;
            existing.PaymentType = sale.PaymentType;
            existing.PaidAmount = sale.PaidAmount;
            existing.RemainingAmount = existing.TotalAmount - sale.PaidAmount;
            existing.IsPaid = existing.RemainingAmount <= 0;
            existing.PaymentDueDate = sale.PaymentDueDate;
            existing.Notes = sale.Notes;

            // Sync with Income record
            var income = await _context.Incomes.FirstOrDefaultAsync(i => i.SaleId == sale.Id);
            if (income != null)
            {
                if (sale.PaidAmount > 0)
                {
                    income.Amount = sale.PaidAmount;
                    income.PaymentType = sale.PaymentType;
                    income.Date = sale.SaleDate;
                }
                else
                {
                     // If paid amount becomes 0, remove the income record
                    _context.Incomes.Remove(income);
                }
            }
            else if (sale.PaidAmount > 0)
            {
                 // Create new income record if it didn't exist
                _context.Incomes.Add(new Income
                {
                    Date = sale.SaleDate,
                    CustomerId = sale.CustomerId,
                    Amount = sale.PaidAmount,
                    PaymentType = sale.PaymentType,
                    Description = $"Sale Income: {sale.SaleNumber}",
                    UserId = sale.UserId,
                    SaleId = sale.Id,
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.SaleItems)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale != null)
            {
                // Restore stock
                foreach (var item in sale.SaleItems)
                {
                    var product = await _context.Items.FindAsync(item.ItemId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.UpdatedDate = DateTime.Now;
                    }
                }

                // Delete associated income
                var income = await _context.Incomes.FirstOrDefaultAsync(i => i.SaleId == id);
                if (income != null)
                {
                    _context.Incomes.Remove(income);
                }

                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GenerateSaleNumberAsync()
        {
            var today = DateTime.Now;
            var datePrefix = today.ToString("yyyyMMdd");
            
            // Get the count of sales created today
            var todaySalesCount = await _context.Sales
                .Where(s => s.SaleNumber.StartsWith($"SALE-{datePrefix}"))
                .CountAsync();

            var sequence = (todaySalesCount + 1).ToString("D4");
            return $"SALE-{datePrefix}-{sequence}";
        }
    }
}
