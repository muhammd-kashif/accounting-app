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
                new Supplier { SupplierName = "Ali", Contact = "03001234567", Address = "Lahore", UserId = 1, OpeningBalance = 5000 },
                new Supplier { SupplierName = "Umair", Contact = "03001234567", Address = "Karachi", UserId = 1, OpeningBalance = 0 },
                new Supplier { SupplierName = "Amir", Contact = "03001234567", Address = "Islamabad", UserId = 1, OpeningBalance = 2500 },
                new Supplier { SupplierName = "Hassan", Contact = "03001234567", Address = "Peshawar", UserId = 1, OpeningBalance = 0 },
                new Supplier { SupplierName = "Ahmed", Contact = "03330000000", Address = "Quetta", UserId = 1, OpeningBalance = 10000 }
            };
            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();

            // 4. Seed Products (Stationary + Services)
            var products = new List<Product>
            {
                // Inventory
                new Product { ItemName = "Eraser", PurchasePrice = 15, SalePrice = 20, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Ruler 30cm", PurchasePrice = 30, SalePrice = 40, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Notebook A4", PurchasePrice = 150, SalePrice = 180, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                new Product { ItemName = "Stapler", PurchasePrice = 250, SalePrice = 300, StockQuantity = 100, ItemType = "Inventory", UserId = 1 },
                
                // Services
                new Product { ItemName = "Printing Service", PurchasePrice = 2, SalePrice = 5, StockQuantity = 0, ItemType = "Service", UserId = 1, Description = "Color/BW Printing" },
                new Product { ItemName = "Binding Service", PurchasePrice = 20, SalePrice = 50, StockQuantity = 0, ItemType = "Service", UserId = 1, Description = "Hard/Soft Binding" },
                
                // Non-Inventory
                new Product { ItemName = "Office Chair", PurchasePrice = 4500, SalePrice = 0, StockQuantity = 0, ItemType = "Non-Inventory", UserId = 1, Description = "One-time office asset" },
                new Product { ItemName = "Cleaning Supplies", PurchasePrice = 1200, SalePrice = 0, StockQuantity = 0, ItemType = "Non-Inventory", UserId = 1, Description = "Monthly cleaning stock" }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // 5. Seed Transactions for Supplier Ledger
            var ali = suppliers.First(s => s.SupplierName == "Ali");
            var notebook = products.First(p => p.ItemName == "Notebook A4");

            var aliBill = new Bill
            {
                SupplierId = ali.Id,
                BillNumber = "BILL-0001",
                BillDate = DateTime.Now.AddDays(-5),
                DueDate = DateTime.Now.AddDays(25),
                Status = "Partial",
                TotalAmount = 7500,
                PaidAmount = 3000,
                UserId = 1,
                Items = new List<BillItem>
                {
                    new BillItem { ProductId = notebook.Id, Quantity = 50, Rate = 150, Amount = 7500 }
                }
            };
            context.Bills.Add(aliBill);
            await context.SaveChangesAsync();

            var aliPayment = new BillPayment
            {
                BillId = aliBill.Id,
                PaymentDate = DateTime.Now.AddDays(-2),
                Amount = 3000,
                PaymentMethod = "Cash",
                ReferenceNo = "PAY-001",
                UserId = 1
            };
            context.BillPayments.Add(aliPayment);

            var umair = suppliers.First(s => s.SupplierName == "Umair");
            var chair = products.First(p => p.ItemName == "Office Chair");

            var umairPurchase = new Purchase
            {
                SupplierId = umair.Id,
                Date = DateTime.Now.AddDays(-1),
                PaymentMethod = "Credit",
                ReferenceNo = "PUR-001",
                TotalAmount = 9000,
                UserId = 1,
                Items = new List<PurchaseItem>
                {
                    new PurchaseItem { ProductId = chair.Id, Quantity = 2, Rate = 4500, Amount = 9000 }
                }
            };
            context.Purchases.Add(umairPurchase);

            await context.SaveChangesAsync();

            // 6. Seed Customers (if needed) & Sales
            if (!context.Customers.Any())
            {
                context.Customers.Add(new Customer { Name = "Walk-in Customer", Phone = "03000000000", Address = "Local", UserId = 1, OpeningBalance = 0 });
                await context.SaveChangesAsync();
            }

            var walkInCustomer = context.Customers.First();
            var notebookA4 = products.First(p => p.ItemName == "Notebook A4");
            var stapler = products.First(p => p.ItemName == "Stapler");

            // Seed Sales
            if (!context.Sales.Any())
            {
                var sale1 = new Sale
                {
                    SaleNumber = "SALE-1001",
                    CustomerId = walkInCustomer.Id,
                    SaleDate = DateTime.Now.AddDays(-3),
                    PaymentType = "Cash",
                    TotalAmount = 1800, // 10 notebooks
                    PaidAmount = 1800,
                    RemainingAmount = 0,
                    UserId = 1,
                    SaleItems = new List<SaleItem>
                    {
                        new SaleItem { ProductId = notebookA4.Id, Quantity = 10, UnitPrice = 180, TotalPrice = 1800 }
                    },
                    Payments = new List<SalePayment>
                    {
                        new SalePayment { PaymentDate = DateTime.Now.AddDays(-3), Amount = 1800, PaymentMethod = "Cash", UserId = 1 }
                    }
                };
                context.Sales.Add(sale1);

                var sale2 = new Sale
                {
                    SaleNumber = "SALE-1002",
                    CustomerId = walkInCustomer.Id,
                    SaleDate = DateTime.Now.AddDays(-1),
                    PaymentType = "Credit",
                    TotalAmount = 600, // 2 staplers
                    PaidAmount = 0,
                    RemainingAmount = 600,
                    PaymentDueDate = DateTime.Now.AddDays(7),
                    UserId = 1,
                    SaleItems = new List<SaleItem>
                    {
                        new SaleItem { ProductId = stapler.Id, Quantity = 2, UnitPrice = 300, TotalPrice = 600 }
                    }
                };
                context.Sales.Add(sale2);
                
                await context.SaveChangesAsync();
            }

            // 7. Seed Expenses
            if (!context.Expenses.Any())
            {
                var expenses = new List<Expense>
                {
                    new Expense { Date = DateTime.Now.AddDays(-10), Category = "Rent", Amount = 50000, Description = "Shop Rent - Feb", UserId = 1 },
                    new Expense { Date = DateTime.Now.AddDays(-5), Category = "Utilities", Amount = 15000, Description = "Electricity Bill", UserId = 1 },
                    new Expense { Date = DateTime.Now.AddDays(-2), Category = "Salary", Amount = 30000, Description = "Staff Salary", UserId = 1 },
                    new Expense { Date = DateTime.Now.AddDays(-1), Category = "Maintenance", Amount = 2500, Description = "AC Repair", UserId = 1 }
                };
                context.Expenses.AddRange(expenses);
                await context.SaveChangesAsync();
            }

            // 8. Seed Incomes
            if (!context.Incomes.Any())
            {
                var incomes = new List<Income>
                {
                    new Income { Date = DateTime.Now.AddDays(-4), Amount = 5000, PaymentType = "Cash", Description = "Consulting Fee", UserId = 1, IsPaid = true },
                    new Income { Date = DateTime.Now.AddDays(-2), Amount = 12000, PaymentType = "Bank", Description = "Services Revenue", UserId = 1, IsPaid = true }
                };
                context.Incomes.AddRange(incomes);
                await context.SaveChangesAsync();
            }
        }
    }
}
