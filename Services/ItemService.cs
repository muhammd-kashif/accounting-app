using AccountingApp.Data;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Services
{
    public interface IItemService
    {
        Task<List<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(int id);
        Task<List<Item>> GetByCategoryAsync(string category);
        Task<List<string>> GetCategoriesAsync();
        Task AddAsync(Item item);
        Task UpdateAsync(Item item);
        Task DeleteAsync(int id);
        Task UpdateStockAsync(int itemId, int quantityChange);
    }

    public class ItemService : IItemService
    {
        private readonly ApplicationDbContext _context;

        public ItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Item>> GetAllAsync()
        {
            return await _context.Items
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _context.Items.FindAsync(id);
        }

        public async Task<List<Item>> GetByCategoryAsync(string category)
        {
            return await _context.Items
                .Where(i => i.Category == category)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Items
                .Where(i => !string.IsNullOrEmpty(i.Category))
                .Select(i => i.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task AddAsync(Item item)
        {
            item.CreatedDate = DateTime.Now;
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Item item)
        {
            var existing = await _context.Items.FindAsync(item.Id);
            if (existing == null) return;

            existing.Name = item.Name;
            existing.Category = item.Category;
            existing.Price = item.Price;
            existing.StockQuantity = item.StockQuantity;
            existing.Description = item.Description;
            existing.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStockAsync(int itemId, int quantityChange)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item != null)
            {
                item.StockQuantity += quantityChange;
                item.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
