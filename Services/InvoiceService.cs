using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface IInvoiceService
    {
        Task<Invoice?> GenerateInvoiceAsync(int saleId);
        Task<Invoice?> GetInvoiceBySaleIdAsync(int saleId);
        Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber);
        Task<string> GenerateInvoiceNumberAsync();
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> GenerateInvoiceAsync(int saleId)
        {
            // Check if invoice already exists for this sale
            var existingInvoice = await GetInvoiceBySaleIdAsync(saleId);
            if (existingInvoice != null)
            {
                return existingInvoice;
            }

            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.Id == saleId);

            if (sale == null) return null;

            var invoice = new Invoice
            {
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                SaleId = saleId,
                InvoiceDate = sale.SaleDate,
                PaymentDueDate = sale.PaymentDueDate,
                GeneratedDate = DateTime.Now
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await _context.Invoices
                .Include(i => i.Sale)
                    .ThenInclude(s => s.Customer)
                .Include(i => i.Sale)
                    .ThenInclude(s => s.SaleItems)
                        .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);
        }

        public async Task<Invoice?> GetInvoiceBySaleIdAsync(int saleId)
        {
            return await _context.Invoices
                .Include(i => i.Sale)
                    .ThenInclude(s => s.Customer)
                .Include(i => i.Sale)
                    .ThenInclude(s => s.SaleItems)
                        .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(i => i.SaleId == saleId);
        }

        public async Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            return await _context.Invoices
                .Include(i => i.Sale)
                    .ThenInclude(s => s.Customer)
                .Include(i => i.Sale)
                    .ThenInclude(s => s.SaleItems)
                        .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            // Get the count of all invoices
            var invoiceCount = await _context.Invoices.CountAsync();
            var sequence = (invoiceCount + 1).ToString("D3");
            return $"INV-{sequence}";
        }
    }
}
