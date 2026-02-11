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

            // Update stock for each item (increase stock on bill/purchase)
            foreach (var item in bill.Items)
            {
                await _productService.UpdateStockAsync(item.ProductId, item.Quantity);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Items)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill != null)
            {
                // Remove linked Expenses from payments
                var paymentIds = bill.Payments.Select(p => p.Id).ToList();
                var expenses = await _context.Expenses
                    .Where(e => e.BillPaymentId != null && paymentIds.Contains(e.BillPaymentId.Value))
                    .ToListAsync();
                _context.Expenses.RemoveRange(expenses);

                // Reverse stock
                foreach (var item in bill.Items)
                {
                    await _productService.UpdateStockAsync(item.ProductId, -item.Quantity);
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
