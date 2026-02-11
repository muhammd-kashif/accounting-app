using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync(int userId);
        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task UpdateStockAsync(int productId, int quantityChange);
        Task<List<Product>> GetLowStockAsync(int userId);
        Task<Product> FindOrCreateByNameAsync(string name, decimal rate, int userId);
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync(int userId)
        {
            return await _context.Products
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.ItemName)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task AddAsync(Product product)
        {
            product.CreatedDate = DateTime.Now;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing == null) return;

            existing.ItemName = product.ItemName;
            existing.ItemType = product.ItemType;
            existing.SKU = product.SKU;
            existing.Description = product.Description;
            existing.PurchasePrice = product.PurchasePrice;
            existing.SalePrice = product.SalePrice;
            existing.StockQuantity = product.StockQuantity;
            existing.ReorderLevel = product.ReorderLevel;
            existing.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Check if product is used in any purchase or bill items
            var usedInPurchase = await _context.PurchaseItems.AnyAsync(pi => pi.ProductId == id);
            var usedInBill = await _context.BillItems.AnyAsync(bi => bi.ProductId == id);

            if (usedInPurchase || usedInBill)
            {
                throw new InvalidOperationException("This item is used in purchases/bills and cannot be deleted.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStockAsync(int productId, int quantityChange)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity += quantityChange;
                product.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Product>> GetLowStockAsync(int userId)
        {
            return await _context.Products
                .Where(p => p.UserId == userId && p.StockQuantity <= p.ReorderLevel)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        public async Task<Product> FindOrCreateByNameAsync(string name, decimal rate, int userId)
        {
            var trimmed = name.Trim();
            var existing = await _context.Products
                .FirstOrDefaultAsync(p => p.UserId == userId && p.ItemName.ToLower() == trimmed.ToLower());

            if (existing != null)
                return existing;

            var product = new Product
            {
                ItemName = trimmed,
                ItemType = "Inventory",
                PurchasePrice = rate,
                SalePrice = rate,
                StockQuantity = 0,
                UserId = userId,
                CreatedDate = DateTime.Now
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
    }
}
