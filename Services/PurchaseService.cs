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


            // Update stock for Inventory items and create Expense for Non-Inventory items

            foreach (var item in purchase.Items)
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
                            Date = purchase.Date,
                            Category = product.ItemType == "Service" ? "Service" : "Non-Inventory Purchase",
                            Amount = item.Amount,
                            Description = $"Purchase: {product.ItemName} (Qty: {item.Quantity}) - Ref: {purchase.ReferenceNo}",
                            UserId = purchase.UserId,
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
            var purchase = await _context.Purchases
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase != null)
            {

                // Reverse stock changes for Inventory items and delete Expense for Non-Inventory

                foreach (var item in purchase.Items)
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
                                    e.Description.Contains($"Purchase: {item.Product.ItemName}") &&
                                    e.Description.Contains($"Ref: {purchase.ReferenceNo}") &&
                                    e.Date.Date == purchase.Date.Date);
                            
                            if (relatedExpense != null)
                            {
                                _context.Expenses.Remove(relatedExpense);
                            }
                        }
                    }
                }

                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();
            }
        }
    }
}
