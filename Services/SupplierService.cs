using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface ISupplierService
    {
        Task<List<Supplier>> GetAllAsync(int userId);
        Task<Supplier?> GetByIdAsync(int id);
        Task AddAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task DeleteAsync(int id);
        Task<List<LedgerEntry>> GetSupplierLedgerAsync(int supplierId, DateTime? fromDate, DateTime? toDate);
        Task<SupplierBalanceDto?> GetSupplierBalanceByIdAsync(int supplierId);
        Task<List<SupplierBalanceDto>> GetSupplierBalancesAsync();
    }

    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;

        public SupplierService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Supplier>> GetAllAsync(int userId)
        {
            return await _context.Suppliers
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task AddAsync(Supplier supplier)
        {
            supplier.CreatedDate = DateTime.Now;
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            var existing = await _context.Suppliers.FindAsync(supplier.Id);
            if (existing == null)
            {
                return;
            }

            existing.SupplierName = supplier.SupplierName;
            existing.Contact = supplier.Contact;
            existing.Email = supplier.Email;
            existing.Address = supplier.Address;
            existing.OpeningBalance = supplier.OpeningBalance;
            existing.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<LedgerEntry>> GetSupplierLedgerAsync(int supplierId, DateTime? fromDate, DateTime? toDate)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null) return new List<LedgerEntry>();

            var ledger = new List<LedgerEntry>();
            decimal runningBalance = supplier.OpeningBalance;

            // 1. Get Bills (Credits - We owe this)
            var bills = await _context.Bills
                .Where(b => b.SupplierId == supplierId)
                .ToListAsync();

            // 2. Get Purchases (Credits)
            var purchases = await _context.Purchases
                .Where(p => p.SupplierId == supplierId)
                .ToListAsync();

            // 3. Get Payments (Debits)
            var payments = await _context.BillPayments
                .Include(bp => bp.Bill)
                .Where(bp => bp.Bill != null && bp.Bill.SupplierId == supplierId)
                .ToListAsync();

            // MERGE & MAP
            var entries = new List<LedgerEntry>();

            // Opening Balance Entry (Always at the start)
            entries.Add(new LedgerEntry
            {
                Date = supplier.CreatedDate,
                Description = "Opening Balance",
                Debit = 0,
                Credit = 0, // It's the starting balance, not a transaction
                Balance = supplier.OpeningBalance,
                Reference = "OB"
            });

            foreach (var b in bills)
            {
                entries.Add(new LedgerEntry
                {
                    Date = b.BillDate,
                    Description = $"Bill #{b.BillNumber}",
                    Debit = 0,
                    Credit = b.TotalAmount,
                    Reference = b.Id.ToString()
                });
            }

            foreach (var p in purchases)
            {
                entries.Add(new LedgerEntry
                {
                    Date = p.Date,
                    Description = $"Purchase (Ref: {p.ReferenceNo})",
                    Debit = 0,
                    Credit = p.TotalAmount,
                    Reference = p.Id.ToString()
                });

                // If non-credit, add a matching payment/debit
                if (p.PaymentMethod != "Credit" && p.PaymentMethod != "Udhar")
                {
                    entries.Add(new LedgerEntry
                    {
                        Date = p.Date,
                        Description = $"Paid ({p.PaymentMethod}) for Pur: {p.ReferenceNo}",
                        Debit = p.TotalAmount,
                        Credit = 0,
                        Reference = p.Id.ToString()
                    });
                }
            }

            foreach (var pay in payments)
            {
                entries.Add(new LedgerEntry
                {
                    Date = pay.PaymentDate,
                    Description = $"Bill Payment Ref: {pay.ReferenceNo}",
                    Debit = pay.Amount,
                    Credit = 0,
                    Reference = pay.Id.ToString()
                });
            }

            // Sort by Date then calculate running balance
            var sortedEntries = entries.OrderBy(e => e.Date).ToList();
            decimal balance = 0;
            var finalLedger = new List<LedgerEntry>();

            if (fromDate.HasValue)
            {
                // Calculate balance before the fromDate
                decimal bfBalance = 0;
                foreach (var entry in sortedEntries)
                {
                    if (entry.Date < fromDate.Value)
                    {
                        if (entry.Reference == "OB") bfBalance = entry.Balance;
                        else bfBalance += (entry.Credit - entry.Debit);
                    }
                }

                // Add Brought Forward entry
                finalLedger.Add(new LedgerEntry
                {
                    Date = fromDate.Value.AddSeconds(-1),
                    Description = "Brought Forward Balance",
                    Debit = 0,
                    Credit = 0,
                    Balance = bfBalance,
                    Reference = "BF"
                });
                balance = bfBalance;

                // Add entries from fromDate onwards
                foreach (var entry in sortedEntries.Where(e => e.Date >= fromDate.Value && (!toDate.HasValue || e.Date <= toDate.Value)))
                {
                    balance += (entry.Credit - entry.Debit);
                    entry.Balance = balance;
                    finalLedger.Add(entry);
                }
            }
            else
            {
                // No fromDate, show everything from start
                foreach (var entry in sortedEntries)
                {
                    if (entry.Reference == "OB")
                    {
                        balance = entry.Balance;
                    }
                    else
                    {
                        balance += (entry.Credit - entry.Debit);
                        entry.Balance = balance;
                    }
                    
                    if (!toDate.HasValue || entry.Date <= toDate.Value)
                    {
                        finalLedger.Add(entry);
                    }
                }
            }

            return finalLedger;
        }

        public async Task<SupplierBalanceDto?> GetSupplierBalanceByIdAsync(int supplierId)
        {
            var s = await _context.Suppliers.FindAsync(supplierId);
            if (s == null) return null;

            var bills = await _context.Bills
                .Where(b => b.SupplierId == supplierId)
                .Select(b => new { b.TotalAmount, b.BillDate })
                .ToListAsync();

            var purchases = await _context.Purchases
                .Where(p => p.SupplierId == supplierId)
                .Select(p => new { p.TotalAmount, p.Date, p.PaymentMethod })
                .ToListAsync();

            var payments = await _context.BillPayments
                .Include(bp => bp.Bill)
                .Where(bp => bp.Bill != null && bp.Bill.SupplierId == supplierId)
                .Select(p => new { p.Amount, p.PaymentDate })
                .ToListAsync();

            var totalBills = bills.Sum(b => b.TotalAmount);
            var totalPurchases = purchases.Sum(p => p.TotalAmount);
            var totalDerivedBills = totalBills + totalPurchases;

            var billPayments = payments.Sum(p => p.Amount);
            var purchasePayments = purchases
                .Where(p => p.PaymentMethod != "Credit" && p.PaymentMethod != "Udhar")
                .Sum(p => p.TotalAmount);
            
            var totalPaid = billPayments + purchasePayments;
            
            var opening = s.OpeningBalance;

            var lastBillDate = bills.OrderByDescending(b => b.BillDate).FirstOrDefault()?.BillDate;
            var lastPurchaseDate = purchases.OrderByDescending(p => p.Date).FirstOrDefault()?.Date;
            var lastPaymentDate = payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate;

            DateTime? lastActivity = lastBillDate;
            if (lastPurchaseDate > lastActivity) lastActivity = lastPurchaseDate;
            if (lastPaymentDate > lastActivity) lastActivity = lastPaymentDate;
            if (lastActivity == null) lastActivity = s.CreatedDate;

            return new SupplierBalanceDto
            {
                SupplierId = s.Id,
                SupplierName = s.SupplierName,
                Contact = s.Contact,
                Email = s.Email,
                OpeningBalance = opening,
                TotalBills = totalDerivedBills,
                TotalPaid = totalPaid,
                NetBalance = (opening + totalDerivedBills) - totalPaid,
                LastActivityDate = lastActivity
            };
        }

        public async Task<List<SupplierBalanceDto>> GetSupplierBalancesAsync()
        {
            var suppliers = await _context.Suppliers.ToListAsync();
            var dtos = new List<SupplierBalanceDto>();

            // For performance, we should fetch all related data in batches or optimize
            // But for now, let's just loop (N+1 query issue but simplest for immediate consistency)
            // Or better: fetch all relevant data and join in memory if dataset is small
            
            var allBills = await _context.Bills.Select(b => new { b.SupplierId, b.TotalAmount }).ToListAsync();
            var allPurchases = await _context.Purchases.Select(p => new { p.SupplierId, p.TotalAmount, p.PaymentMethod }).ToListAsync();
            
            // Payment handling is tricky because BillPayments link to Bill, not SupplierId directly in simple model sometimes
            // But we can filter by Bill.SupplierId
            var allPayments = await _context.BillPayments
                .Include(bp => bp.Bill)
                .Where(bp => bp.Bill != null)
                .Select(p => new { p.Bill.SupplierId, p.Amount })
                .ToListAsync();

            foreach (var s in suppliers)
            {
                var sBills = allBills.Where(b => b.SupplierId == s.Id).Sum(b => b.TotalAmount);
                var sPurchases = allPurchases.Where(p => p.SupplierId == s.Id).Sum(p => p.TotalAmount);
                
                var sBillPayments = allPayments.Where(p => p.SupplierId == s.Id).Sum(p => p.Amount);
                var sPurchasePayments = allPurchases
                    .Where(p => p.SupplierId == s.Id && p.PaymentMethod != "Credit" && p.PaymentMethod != "Udhar")
                    .Sum(p => p.TotalAmount);

                var totalPaid = sBillPayments + sPurchasePayments;
                var totalOwed = sBills + sPurchases;
                
                dtos.Add(new SupplierBalanceDto 
                {
                    SupplierId = s.Id,
                    SupplierName = s.SupplierName,
                    Contact = s.Contact,
                    Email = s.Email,
                    OpeningBalance = s.OpeningBalance,
                    TotalBills = totalOwed,
                    TotalPaid = totalPaid,
                    NetBalance = (s.OpeningBalance + totalOwed) - totalPaid,
                    LastActivityDate = s.UpdatedDate ?? s.CreatedDate // Approximate for list view
                });
            }

            return dtos;
        }
    }
}
