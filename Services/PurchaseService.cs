using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface IPurchaseService
    {
        Task<List<Purchase>> GetAllAsync(int userId);
        Task<Purchase?> GetByIdAsync(int id);
        Task AddAsync(Purchase purchase);
        Task DeleteAsync(int id);
    }

    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public PurchaseService(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<List<Purchase>> GetAllAsync(int userId)
        {
            return await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<Purchase?> GetByIdAsync(int id)
        {
            return await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Purchase purchase)
        {
            purchase.CreatedDate = DateTime.Now;
            purchase.TotalAmount = purchase.Items.Sum(i => i.Amount);

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            // Update stock for each item (increase stock on purchase)
            foreach (var item in purchase.Items)
            {
                await _productService.UpdateStockAsync(item.ProductId, item.Quantity);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase != null)
            {
                // Reverse stock changes
                foreach (var item in purchase.Items)
                {
                    await _productService.UpdateStockAsync(item.ProductId, -item.Quantity);
                }

                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();
            }
        }
    }
}
