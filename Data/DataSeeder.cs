using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Clear existing generic inventory/purchase data
            // We need to clear child tables first to avoid FK errors
            if (context.PurchaseItems.Any())
            {
                context.PurchaseItems.RemoveRange(context.PurchaseItems);
            }
            if (context.BillItems.Any())
            {
                context.BillItems.RemoveRange(context.BillItems);
            }
            if (context.Purchases.Any())
            {
                context.Purchases.RemoveRange(context.Purchases);
            }
             if (context.Bills.Any())
            {
                context.Bills.RemoveRange(context.Bills);
            }

            await context.SaveChangesAsync();

            // 2. Clear Suppliers & Products
            if (context.Suppliers.Any())
            {
                context.Suppliers.RemoveRange(context.Suppliers);
            }
            if (context.Products.Any())
            {
                context.Products.RemoveRange(context.Products);
            }
            await context.SaveChangesAsync();

            // 3. Seed Suppliers
            var suppliers = new List<Supplier>
            {
                new Supplier { SupplierName = "Ali", Contact = "03001234567", Address = "Lahore", UserId = 1 },
                new Supplier { SupplierName = "Umair", Contact = "03001234567", Address = "Karachi", UserId = 1 },
                new Supplier { SupplierName = "Amir", Contact = "03001234567", Address = "Islamabad", UserId = 1 },
                new Supplier { SupplierName = "Hassan", Contact = "03001234567", Address = "Peshawar", UserId = 1 },
                new Supplier { SupplierName = "Hussain", Contact = "03001234567", Address = "Multan", UserId = 1 },
                new Supplier { SupplierName = "Kashif", Contact = "03001234567", Address = "Faisalabad", UserId = 1 },
                new Supplier { SupplierName = "Sujjal", Contact = "03001234567", Address = "Rawalpindi", UserId = 1 },
                new Supplier { SupplierName = "Hamza", Contact = "03210000000", Address = "Sialkot", UserId = 1 },
                new Supplier { SupplierName = "Bilal", Contact = "03450000000", Address = "Gujranwala", UserId = 1 },
                new Supplier { SupplierName = "Ahmed", Contact = "03330000000", Address = "Quetta", UserId = 1 }
            };
            await context.Suppliers.AddRangeAsync(suppliers);

            // 4. Seed Products (Stationary Items)
            // Prices from image are mapped to PurchasePrice. SalePrice set to same or + margin.
            var products = new List<Product>
            {
                new Product { ItemName = "Eraser", PurchasePrice = 15, SalePrice = 20, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Ruler 30cm", PurchasePrice = 30, SalePrice = 40, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Sharpener", PurchasePrice = 25, SalePrice = 35, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Glue Stick", PurchasePrice = 40, SalePrice = 50, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Highlighter Pink", PurchasePrice = 35, SalePrice = 45, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Highlighter Yellow", PurchasePrice = 35, SalePrice = 45, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Marker Set", PurchasePrice = 200, SalePrice = 250, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Notebook A4", PurchasePrice = 150, SalePrice = 180, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Notebook B5", PurchasePrice = 100, SalePrice = 130, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Scissors", PurchasePrice = 120, SalePrice = 150, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Stapler", PurchasePrice = 250, SalePrice = 300, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Pencil HB", PurchasePrice = 10, SalePrice = 15, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Pen Black", PurchasePrice = 20, SalePrice = 25, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Pen Blue", PurchasePrice = 20, SalePrice = 25, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Pen Red", PurchasePrice = 20, SalePrice = 25, StockQuantity = 100, ItemType = "Inventory", UserId = 1 }
            };
            await context.Products.AddRangeAsync(products);

            await context.SaveChangesAsync();
        }
    }
}
