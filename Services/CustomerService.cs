using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllAsync(int userId);
        Task<Customer?> GetByIdAsync(int id);
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
        Task<decimal> GetTotalOpeningBalance(int userId);
        Task<List<LedgerEntry>> GetCustomerLedgerAsync(int customerId, DateTime? fromDate, DateTime? toDate);
        // Task<List<CustomerBalanceDto>> GetCustomerBalancesAsync();
        Task<List<CustomerBalanceDto>> GetCustomerBalancesAsync();
        Task<CustomerBalanceDto?> GetCustomerBalanceByIdAsync(int customerId);

    }

    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllAsync(int userId)
        {
            return await _context.Customers
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task AddAsync(Customer customer)
        {
            customer.CreatedDate = DateTime.Now;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            var existing = await _context.Customers.FindAsync(customer.Id);
            if (existing == null)
            {
                return;
            }

            existing.Name = customer.Name;
            existing.Phone = customer.Phone;
            existing.Email = customer.Email;
            existing.OpeningBalance = customer.OpeningBalance;
            existing.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalOpeningBalance(int userId)
        {
            return await _context.Customers
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.OpeningBalance ?? 0m);
        }

        public async Task<List<LedgerEntry>> GetCustomerLedgerAsync(int customerId, DateTime? fromDate, DateTime? toDate)
        {
             var customer = await _context.Customers.FindAsync(customerId);
             if (customer == null) return new List<LedgerEntry>();

             // 1. Get Sales (Debits)
             var salesQuery = _context.Sales.Where(s => s.CustomerId == customerId);
             if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
             if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);
             
             var sales = await salesQuery.ToListAsync();

             // 2. Get Incomes (Credits)
             var incomesQuery = _context.Incomes.Where(i => i.CustomerId == customerId);
             if (fromDate.HasValue) incomesQuery = incomesQuery.Where(i => i.Date >= fromDate.Value);
             if (toDate.HasValue) incomesQuery = incomesQuery.Where(i => i.Date <= toDate.Value);

             var incomes = await incomesQuery.ToListAsync();

             // 3. Merge into Ledger Entries
             var ledger = new List<LedgerEntry>();

             // Add Opening Balance as first entry if no date filter or start date is old enough
             // For simplicity, we just show it as the starting point.
             // Ideally we calculate opening balance based on transactions BEFORE fromDate.
             
             decimal runningBalance = customer.OpeningBalance ?? 0;
             
             // If filters are applied, we need to calculate the balance before the 'fromDate'
             if (fromDate.HasValue)
             {
                 var preSales = await _context.Sales
                     .Where(s => s.CustomerId == customerId && s.SaleDate < fromDate.Value)
                     .SumAsync(s => s.TotalAmount);
                     
                 var preIncomes = await _context.Incomes
                     .Where(i => i.CustomerId == customerId && i.Date < fromDate.Value)
                     .SumAsync(i => i.Amount);

                 runningBalance += (preSales - preIncomes);
                 
                 ledger.Add(new LedgerEntry 
                 { 
                     Date = fromDate.Value.AddDays(-1), 
                     Description = "Opening Balance (Brought Forward)", 
                     Balance = runningBalance 
                 });
             }
             else
             {
                  ledger.Add(new LedgerEntry 
                 { 
                     Date = customer.CreatedDate ?? DateTime.MinValue, 
                     Description = "Opening Balance", 
                     Balance = runningBalance 
                 });
             }

             // Sales to Entries
             var saleEntries = sales.Select(s => new LedgerEntry
             {
                 Date = s.SaleDate,
                 Description = $"Sale #{s.SaleNumber}",
                 Debit = s.TotalAmount,
                 Credit = 0,
                 Reference = s.Id.ToString() // Link to sale
             });

             // Incomes to Entries
             var incomeEntries = incomes.Select(i => new LedgerEntry
             {
                 Date = i.Date ?? DateTime.Now,
                 Description = i.Description ?? "Payment Received",
                 Debit = 0,
                 Credit = i.Amount,
                 Reference = i.SaleId.HasValue ? $"Sale Ref: {i.SaleId}" : "Manual"
             });

             // Merge and Sort
             var allTransactions = saleEntries.Concat(incomeEntries).OrderBy(e => e.Date).ToList();

             foreach (var entry in allTransactions)
             {
                 runningBalance += (entry.Debit - entry.Credit);
                 entry.Balance = runningBalance;
                 ledger.Add(entry);
             }

             return ledger;
        }
        public async Task<List<CustomerBalanceDto>> GetCustomerBalancesAsync()
        {
            var customers = await _context.Customers.ToListAsync();
            var summaries = new List<CustomerBalanceDto>();

            foreach (var c in customers)
            {
                var sales = await _context.Sales
                    .Where(s => s.CustomerId == c.Id)
                    .Select(s => new { s.TotalAmount, s.SaleDate })
                    .ToListAsync();

                var incomes = await _context.Incomes
                    .Where(i => i.CustomerId == c.Id)
                    .Select(i => new { i.Amount, i.Date })
                    .ToListAsync();

                var totalSales = sales.Sum(s => s.TotalAmount);
                var totalReceived = incomes.Sum(i => i.Amount);
                var opening = c.OpeningBalance ?? 0;

                // Last activity logic
                DateTime? lastSale = sales.OrderByDescending(s => s.SaleDate).FirstOrDefault()?.SaleDate;
                DateTime? lastPayment = incomes.OrderByDescending(i => i.Date).FirstOrDefault()?.Date;
                DateTime? lastActivity = (lastSale > lastPayment ? lastSale : lastPayment) ?? lastSale ?? lastPayment;

                summaries.Add(new CustomerBalanceDto
                {
                    CustomerId = c.Id,
                    Name = c.Name ?? "Unknown",
                    Phone = c.Phone ?? "",
                    TotalSales = totalSales,
                    TotalReceived = totalReceived,
                    OpeningBalance = opening,
                    NetBalance = (opening + totalSales) - totalReceived,
                    CreditLimit = c.CreditLimit ?? 5000,
                    TotalOrders = sales.Count,
                    LastActivityDate = lastActivity
                });
            }

            return summaries.OrderByDescending(s => s.NetBalance).ToList();
        }
         public async Task<CustomerBalanceDto?> GetCustomerBalanceByIdAsync(int customerId)
{
    var c = await _context.Customers.FindAsync(customerId);
    if (c == null) return null;

    var sales = await _context.Sales
        .Where(s => s.CustomerId == customerId)
        .Select(s => new { s.TotalAmount, s.SaleDate })
        .ToListAsync();

    var incomes = await _context.Incomes
        .Where(i => i.CustomerId == customerId)
        .Select(i => new { i.Amount, i.Date })
        .ToListAsync();

    var totalSales = sales.Sum(s => s.TotalAmount);
    var totalReceived = incomes.Sum(i => i.Amount);
    var opening = c.OpeningBalance ?? 0;

    DateTime? lastSale = sales.OrderByDescending(s => s.SaleDate).FirstOrDefault()?.SaleDate;
    DateTime? lastPayment = incomes.OrderByDescending(i => i.Date).FirstOrDefault()?.Date;
    DateTime? lastActivity = (lastSale > lastPayment ? lastSale : lastPayment) ?? lastSale ?? lastPayment;

    return new CustomerBalanceDto
    {
        CustomerId = c.Id,
        Name = c.Name ?? "Unknown",
        Phone = c.Phone ?? "",
        OpeningBalance = opening,
        TotalSales = totalSales,
        TotalReceived = totalReceived,
        NetBalance = (opening + totalSales) - totalReceived,
        CreditLimit = c.CreditLimit ?? 5000,
        TotalOrders = sales.Count,
        LastActivityDate = lastActivity
    };
}

    }

}
