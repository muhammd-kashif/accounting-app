using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    // DTOs for Financial Reports
    public class ProfitLossStatement
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCostOfGoodsSold { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal TotalOperatingExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public List<ExpenseCategory> ExpenseBreakdown { get; set; } = new();
        public decimal TotalSalesRevenue { get; set; }
        public decimal TotalOtherIncome { get; set; }
    }

    public class ExpenseCategory
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class CashFlowStatement
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Operating Activities
        public decimal CashFromOperations { get; set; }
        public decimal CashPaidToSuppliers { get; set; }
        public decimal CashReceivedFromCustomers { get; set; }
        public decimal OperatingExpensesPaid { get; set; }
        public decimal NetCashFromOperating { get; set; }
        
        // Investing Activities (Future: assets, investments)
        public decimal NetCashFromInvesting { get; set; }
        
        // Financing Activities (Future: loans, equity)
        public decimal NetCashFromFinancing { get; set; }
        
        // Net Change
        public decimal NetCashFlow { get; set; }
        public decimal OpeningCash { get; set; }
        public decimal ClosingCash { get; set; }
    }

    public class BalanceSheet
    {
        public DateTime AsOfDate { get; set; }
        
        // Assets
        public decimal Cash { get; set; }
        public decimal AccountsReceivable { get; set; }
        public decimal Inventory { get; set; }
        public decimal TotalCurrentAssets { get; set; }
        public decimal TotalAssets { get; set; }
        
        // Liabilities
        public decimal AccountsPayable { get; set; }
        public decimal TotalCurrentLiabilities { get; set; }
        public decimal TotalLiabilities { get; set; }
        
        // Equity
        public decimal RetainedEarnings { get; set; }
        public decimal TotalEquity { get; set; }
        
        // Balance Check
        public decimal TotalLiabilitiesAndEquity { get; set; }
        public bool IsBalanced => Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 0.01m;
    }

    public interface IFinancialReportService
    {
        Task<ProfitLossStatement> GetProfitLossStatementAsync(int userId, DateTime startDate, DateTime endDate);
        Task<CashFlowStatement> GetCashFlowStatementAsync(int userId, DateTime startDate, DateTime endDate);
        Task<BalanceSheet> GetBalanceSheetAsync(int userId, DateTime asOfDate);
    }

    public class FinancialReportService : IFinancialReportService
    {
        private readonly ApplicationDbContext _context;

        public FinancialReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProfitLossStatement> GetProfitLossStatementAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var statement = new ProfitLossStatement
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // Calculate Sales Revenue (from Sales table)
            var salesInPeriod = await _context.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate && s.SaleDate <= endDate)
                .ToListAsync();

            statement.TotalSalesRevenue = salesInPeriod.Sum(s => s.TotalAmount);

            // Calculate Other Income (from Incomes table)
            statement.TotalOtherIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.HasValue && i.Date.Value >= startDate && i.Date.Value <= endDate)
                .SumAsync(i => i.Amount);

            // Total Revenue
            statement.TotalRevenue = statement.TotalSalesRevenue + statement.TotalOtherIncome;

            // Calculate Cost of Goods Sold (COGS) - cost from purchase items related to sales
            var purchasesInPeriod = await _context.Purchases
                .Include(p => p.Items)
                .Where(p => p.UserId == userId && p.Date >= startDate && p.Date <= endDate)
                .ToListAsync();

            statement.TotalCostOfGoodsSold = purchasesInPeriod.SelectMany(p => p.Items).Sum(pi => pi.Amount);

            // Gross Profit = Revenue - COGS
            statement.GrossProfit = statement.TotalRevenue - statement.TotalCostOfGoodsSold;

            // Operating Expenses
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            statement.TotalOperatingExpenses = expenses.Sum(e => e.Amount);

            // Expense Breakdown by Category
            statement.ExpenseBreakdown = expenses
                .GroupBy(e => e.Category ?? "Uncategorized")
                .Select(g => new ExpenseCategory
                {
                    Category = g.Key,
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderByDescending(ec => ec.Amount)
                .ToList();

            // Net Income = Gross Profit - Operating Expenses
            statement.NetIncome = statement.GrossProfit - statement.TotalOperatingExpenses;

            return statement;
        }

        public async Task<CashFlowStatement> GetCashFlowStatementAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var statement = new CashFlowStatement
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // Cash Received from Customers (Paid sales)
            var paidSales = await _context.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= startDate && s.SaleDate <= endDate)
                .ToListAsync();

            statement.CashReceivedFromCustomers = paidSales.Sum(s => s.PaidAmount);

            // Cash from other income
            var otherIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.HasValue && i.Date.Value >= startDate && i.Date.Value <= endDate)
                .SumAsync(i => i.Amount);

            // Cash Paid to Suppliers (Bill payments)
            var billPayments = await _context.BillPayments
                .Include(bp => bp.Bill)
                .Where(bp => bp.Bill.UserId == userId && bp.PaymentDate >= startDate && bp.PaymentDate <= endDate)
                .SumAsync(bp => bp.Amount);

            statement.CashPaidToSuppliers = billPayments;

            // Operating Expenses Paid
            statement.OperatingExpensesPaid = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .SumAsync(e => e.Amount);

            // Net Cash from Operating Activities
            statement.CashFromOperations = statement.CashReceivedFromCustomers + otherIncome;
            statement.NetCashFromOperating = statement.CashFromOperations - statement.CashPaidToSuppliers - statement.OperatingExpensesPaid;

            // Investing & Financing (placeholder for future)
            statement.NetCashFromInvesting = 0;
            statement.NetCashFromFinancing = 0;

            // Net Cash Flow
            statement.NetCashFlow = statement.NetCashFromOperating + statement.NetCashFromInvesting + statement.NetCashFromFinancing;

            // Calculate Opening & Closing Cash
            // Opening cash = all income - all expenses before start date
            var openingIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.HasValue && i.Date.Value < startDate)
                .SumAsync(i => i.Amount);

            var openingSales = await _context.Sales
                .Where(s => s.UserId == userId && s.SaleDate < startDate)
                .SumAsync(s => s.PaidAmount);

            var openingExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date < startDate)
                .SumAsync(e => e.Amount);

            var openingBillPayments = await _context.BillPayments
                .Include(bp => bp.Bill)
                .Where(bp => bp.Bill.UserId == userId && bp.PaymentDate < startDate)
                .SumAsync(bp => bp.Amount);

            statement.OpeningCash = (openingIncome + openingSales) - (openingExpenses + openingBillPayments);
            statement.ClosingCash = statement.OpeningCash + statement.NetCashFlow;

            return statement;
        }

        public async Task<BalanceSheet> GetBalanceSheetAsync(int userId, DateTime asOfDate)
        {
            var sheet = new BalanceSheet
            {
                AsOfDate = asOfDate
            };

            // ASSETS

            // Cash = Total Income + Paid Sales - Total Expenses - Bill Payments (up to asOfDate)
            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date.HasValue && i.Date.Value <= asOfDate)
                .SumAsync(i => i.Amount);

            var totalPaidSales = await _context.Sales
                .Where(s => s.UserId == userId && s.SaleDate <= asOfDate)
                .SumAsync(s => s.PaidAmount);

            var totalExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date <= asOfDate)
                .SumAsync(e => e.Amount);

            var totalBillPayments = await _context.BillPayments
                .Include(bp => bp.Bill)
                .Where(bp => bp.Bill.UserId == userId && bp.PaymentDate <= asOfDate)
                .SumAsync(bp => bp.Amount);

            sheet.Cash = (totalIncome + totalPaidSales) - (totalExpenses + totalBillPayments);

            // Accounts Receivable = Unpaid/Remaining amounts from customers
            sheet.AccountsReceivable = await _context.Sales
                .Where(s => s.UserId == userId && s.SaleDate <= asOfDate && !s.IsPaid)
                .SumAsync(s => s.RemainingAmount);

            // Inventory = Current stock value (quantity * purchase price)
            var products = await _context.Products
                .Where(p => p.UserId == userId)
                .ToListAsync();

            sheet.Inventory = products.Sum(p => p.StockQuantity * p.PurchasePrice);

            sheet.TotalCurrentAssets = sheet.Cash + sheet.AccountsReceivable + sheet.Inventory;
            sheet.TotalAssets = sheet.TotalCurrentAssets; // For now, no fixed assets

            // LIABILITIES

            // Accounts Payable = Unpaid bills to suppliers
            sheet.AccountsPayable = await _context.Bills
                .Where(b => b.UserId == userId && b.BillDate <= asOfDate && b.Status != "Paid")
                .SumAsync(b => b.TotalAmount - b.PaidAmount);

            sheet.TotalCurrentLiabilities = sheet.AccountsPayable;
            sheet.TotalLiabilities = sheet.TotalCurrentLiabilities;

            // EQUITY

            // Retained Earnings = Total Assets - Total Liabilities
            sheet.RetainedEarnings = sheet.TotalAssets - sheet.TotalLiabilities;
            sheet.TotalEquity = sheet.RetainedEarnings;

            // Total Liabilities + Equity
            sheet.TotalLiabilitiesAndEquity = sheet.TotalLiabilities + sheet.TotalEquity;

            return sheet;
        }
    }
}
