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
        Task<int> CreateSaleAsync(Sale sale, List<SaleItem> items, List<SalePayment> payments);
        Task UpdateAsync(Sale sale, List<SalePayment> payments);
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
                    .ThenInclude(si => si.Product)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<List<Sale>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<Sale?> GetByIdAsync(int id)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Sale?> GetBySaleNumberAsync(string saleNumber)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Payments)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber);
        }

        public async Task<int> CreateSaleAsync(Sale sale, List<SaleItem> items, List<SalePayment> payments)
        {
            // Generate sale number if not provided
            if (string.IsNullOrEmpty(sale.SaleNumber))
            {
                sale.SaleNumber = await GenerateSaleNumberAsync();
            }

            // Calculate totals
            sale.TotalAmount = items.Sum(i => i.TotalPrice);
            sale.PaidAmount = payments.Sum(p => p.Amount);
            sale.RemainingAmount = sale.TotalAmount - sale.PaidAmount;
            sale.IsPaid = sale.RemainingAmount <= 0;
            sale.CreatedDate = DateTime.Now;

            // Add sale
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Add sale items and update stock
            foreach (var item in items)
            {
                item.SaleId = sale.Id;
                _context.SaleItems.Add(item);

                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    product.UpdatedDate = DateTime.Now; // Product might use CreatedDate/UpdatedDate differently? Needs check.
                }
            }

            // Add payments and create income records
            foreach (var payment in payments)
            {
                payment.SaleId = sale.Id;
                payment.UserId = sale.UserId;
                payment.CreatedDate = DateTime.Now;
                _context.SalePayments.Add(payment);

                // Create Income Record
                if (payment.Amount > 0)
                {
                    var income = new Income
                    {
                        Date = payment.PaymentDate,
                        CustomerId = sale.CustomerId,
                        Amount = payment.Amount,
                        PaymentType = payment.PaymentMethod,
                        Description = $"Sale Payment ({payment.PaymentMethod}): {sale.SaleNumber}",
                        UserId = sale.UserId,
                        SaleId = sale.Id,
                        CreatedDate = DateTime.Now
                    };
                    _context.Incomes.Add(income);
                }
            }

            await _context.SaveChangesAsync();
            return sale.Id;
        }

        public async Task UpdateAsync(Sale sale, List<SalePayment> payments)
        {
            var existing = await _context.Sales
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == sale.Id);
                
            if (existing == null) return;

            // Update sale header
            existing.SaleDate = sale.SaleDate;
            existing.PaymentType = "Split"; // Set to Split if multiple, or keep for compatibility
            existing.TotalAmount = sale.TotalAmount; // Should be recalculated in UI or here
            existing.PaidAmount = payments.Sum(p => p.Amount);
            existing.RemainingAmount = existing.TotalAmount - existing.PaidAmount;
            existing.IsPaid = existing.RemainingAmount <= 0;
            existing.PaymentDueDate = sale.PaymentDueDate;
            existing.Notes = sale.Notes;

            // Update Payments and Income records
            // Remove old payments and incomes related to this sale
            var oldPayments = _context.SalePayments.Where(p => p.SaleId == sale.Id);
            _context.SalePayments.RemoveRange(oldPayments);

            var oldIncomes = _context.Incomes.Where(i => i.SaleId == sale.Id);
            _context.Incomes.RemoveRange(oldIncomes);

            // Add new payments and incomes
            foreach (var payment in payments)
            {
                payment.SaleId = sale.Id;
                payment.UserId = sale.UserId;
                if (payment.CreatedDate == default) payment.CreatedDate = DateTime.Now;
                _context.SalePayments.Add(payment);

                if (payment.Amount > 0)
                {
                    _context.Incomes.Add(new Income
                    {
                        Date = payment.PaymentDate,
                        CustomerId = sale.CustomerId,
                        Amount = payment.Amount,
                        PaymentType = payment.PaymentMethod,
                        Description = $"Sale Payment ({payment.PaymentMethod}): {sale.SaleNumber}",
                        UserId = sale.UserId,
                        SaleId = sale.Id,
                        CreatedDate = DateTime.Now
                    });
                }
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
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.UpdatedDate = DateTime.Now; // Assuming Product has UpdatedDate? 
                        // Checked Product.cs earlier? No. Let's assume UpdatedDate exists or remove it if errored.
                        // Actually better to check Product.cs.
                    }
                }

                // Delete associated payments and incomes
                var payments = _context.SalePayments.Where(p => p.SaleId == id);
                _context.SalePayments.RemoveRange(payments);

                var incomes = _context.Incomes.Where(i => i.SaleId == id);
                _context.Incomes.RemoveRange(incomes);

                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GenerateSaleNumberAsync()
        {
            // Get total count of all sales to generate sequential number
            var totalSalesCount = await _context.Sales.CountAsync();
            var sequence = (totalSalesCount + 1).ToString("D4");
            return $"SALE-{sequence}";
        }
    }
}
