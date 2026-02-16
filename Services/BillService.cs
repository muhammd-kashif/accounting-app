using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface IBillService
    {
        Task<List<Bill>> GetAllAsync(int userId);
        Task<List<Bill>> GetUnpaidBillsAsync(int userId);
        Task<Bill?> GetByIdAsync(int id);
        Task AddAsync(Bill bill);
        Task DeleteAsync(int id);
        Task PayBillAsync(BillPayment payment);
        Task<List<BillPayment>> GetPaymentsAsync(int billId);
    }

    public class BillService : IBillService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public BillService(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<List<Bill>> GetAllAsync(int userId)
        {
            return await _context.Bills
                .Include(b => b.Supplier)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Product)
                .Include(b => b.Payments)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BillDate)
                .ToListAsync();
        }

        public async Task<List<Bill>> GetUnpaidBillsAsync(int userId)
        {
            return await _context.Bills
                .Include(b => b.Supplier)
                .Include(b => b.Payments)
                .Where(b => b.UserId == userId && b.Status != "Paid")
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<Bill?> GetByIdAsync(int id)
        {
            return await _context.Bills
                .Include(b => b.Supplier)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Product)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Bill bill)
        {
            // Generate bill number
            var count = await _context.Bills.CountAsync(b => b.UserId == bill.UserId);
            bill.BillNumber = $"BILL-{count + 1:D4}";
            bill.CreatedDate = DateTime.Now;
            bill.TotalAmount = bill.Items.Sum(i => i.Amount);
            bill.Status = "Unpaid";
            bill.PaidAmount = 0;

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            // Update stock for Inventory items and create Expense for Non-Inventory items
            foreach (var item in bill.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product != null)
                {
                    // If Inventory item, update stock
                    if (product.ItemType == "Inventory")
                    {
                        await _productService.UpdateStockAsync(item.ProductId, item.Quantity);
                    }
                    // If Non-Inventory or Service, create Expense record
                    else if (product.ItemType == "Non-Inventory" || product.ItemType == "Service")
                    {
                        var expense = new Expense
                        {
                            Date = bill.BillDate,
                            Category = product.ItemType == "Service" ? "Service" : "Non-Inventory Bill",
                            Amount = item.Amount,
                            Description = $"Bill: {product.ItemName} (Qty: {item.Quantity}) - Bill #: {bill.BillNumber}",
                            UserId = bill.UserId,
                            CreatedDate = DateTime.Now
                        };
                        _context.Expenses.Add(expense);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Items)
                    .ThenInclude(i => i.Product)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill != null)
            {

                // Reverse stock for Inventory items and delete Expense for Non-Inventory

                foreach (var item in bill.Items)
                {
                    if (item.Product != null)
                    {
                        // If Inventory item, reverse stock
                        if (item.Product.ItemType == "Inventory")
                        {
                            await _productService.UpdateStockAsync(item.ProductId, -item.Quantity);
                        }
                        // If Non-Inventory or Service, delete associated Expense
                        else if (item.Product.ItemType == "Non-Inventory" || item.Product.ItemType == "Service")
                        {
                            var relatedExpense = await _context.Expenses
                                .FirstOrDefaultAsync(e => 
                                    e.Description.Contains($"Bill: {item.Product.ItemName}") &&
                                    e.Description.Contains($"Bill #: {bill.BillNumber}") &&
                                    e.Date.Date == bill.BillDate.Date);
                            
                            if (relatedExpense != null)
                            {
                                _context.Expenses.Remove(relatedExpense);
                            }
                        }
                    }
                }

                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();
            }
        }

        public async Task PayBillAsync(BillPayment payment)
        {
            var bill = await _context.Bills
                .Include(b => b.Supplier)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == payment.BillId);

            if (bill == null) return;

            payment.CreatedDate = DateTime.Now;
            _context.BillPayments.Add(payment);
            await _context.SaveChangesAsync(); // Save to get Payment Id

            // Create corresponding Expense record
            var expense = new Expense
            {
                Date = payment.PaymentDate,
                Category = "Bill Payment",
                Amount = payment.Amount,
                Description = $"Payment for Bill {bill.BillNumber} to {bill.Supplier?.SupplierName ?? "Unknown"}",
                UserId = payment.UserId,
                BillPaymentId = payment.Id,
                CreatedDate = DateTime.Now
            };
            _context.Expenses.Add(expense);

            bill.PaidAmount += payment.Amount;
            if (bill.PaidAmount >= bill.TotalAmount)
            {
                bill.Status = "Paid";
                bill.PaidAmount = bill.TotalAmount; // Don't overpay
            }
            else
            {
                bill.Status = "Partial";
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<BillPayment>> GetPaymentsAsync(int billId)
        {
            return await _context.BillPayments
                .Where(p => p.BillId == billId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}
