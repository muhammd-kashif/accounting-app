using Microsoft.EntityFrameworkCore;
using AccountingApp.Models;

namespace AccountingApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Account> Accounts { get; set; }
        
        // Inventory/Purchase System
        public DbSet<Product> Products { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillItem> BillItems { get; set; }
        public DbSet<BillPayment> BillPayments { get; set; }
        
        // Bookshop Stationary System
        public DbSet<Item> Items { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Income>()
                .HasOne(i => i.Customer)
                .WithMany()
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Purchase -> Supplier
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // PurchaseItem -> Purchase
            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // PurchaseItem -> Product
            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Product)
                .WithMany()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bill -> Supplier
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Supplier)
                .WithMany()
                .HasForeignKey(b => b.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // BillItem -> Bill
            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Bill)
                .WithMany(b => b.Items)
                .HasForeignKey(bi => bi.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // BillItem -> Product
            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Product)
                .WithMany()
                .HasForeignKey(bi => bi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // BillPayment -> Bill
            modelBuilder.Entity<BillPayment>()
                .HasOne(bp => bp.Bill)
                .WithMany(b => b.Payments)
                .HasForeignKey(bp => bp.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed property
            modelBuilder.Entity<Bill>()
                .Ignore(b => b.BalanceDue);

            // Decimal precision
            modelBuilder.Entity<Customer>()
                .Property(c => c.OpeningBalance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Supplier>()
                .Property(s => s.OpeningBalance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Income>()
                .Property(i => i.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.PurchasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Purchase>()
                .Property(p => p.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PurchaseItem>()
                .Property(pi => pi.Rate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PurchaseItem>()
                .Property(pi => pi.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Bill>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Bill>()
                .Property(b => b.PaidAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<BillItem>()
                .Property(bi => bi.Rate)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<BillItem>()
                .Property(bi => bi.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<BillPayment>()
                .Property(bp => bp.Amount)
                .HasColumnType("decimal(18,2)");

            // Bookshop Stationary System Configurations
            modelBuilder.Entity<Item>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sale>()
                .Property(s => s.PaidAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sale>()
                .Property(s => s.RemainingAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SaleItem>()
                .Property(si => si.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SaleItem>()
                .Property(si => si.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // Configure relationships
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Item)
                .WithMany()
                .HasForeignKey(si => si.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Sale)
                .WithMany()
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data for stationary items
            var seedDate = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Item>().HasData(
                new Item { Id = 1, Name = "Notebook A4", Category = "Notebooks", Price = 150m, StockQuantity = 100, Description = "A4 size notebook with 200 pages", CreatedDate = seedDate },
                new Item { Id = 2, Name = "Notebook B5", Category = "Notebooks", Price = 100m, StockQuantity = 150, Description = "B5 size notebook with 150 pages", CreatedDate = seedDate },
                new Item { Id = 3, Name = "Pen Blue", Category = "Pens", Price = 20m, StockQuantity = 500, Description = "Blue ballpoint pen", CreatedDate = seedDate },
                new Item { Id = 4, Name = "Pen Black", Category = "Pens", Price = 20m, StockQuantity = 500, Description = "Black ballpoint pen", CreatedDate = seedDate },
                new Item { Id = 5, Name = "Pen Red", Category = "Pens", Price = 20m, StockQuantity = 300, Description = "Red ballpoint pen", CreatedDate = seedDate },
                new Item { Id = 6, Name = "Pencil HB", Category = "Pencils", Price = 10m, StockQuantity = 400, Description = "HB grade pencil", CreatedDate = seedDate },
                new Item { Id = 7, Name = "Eraser", Category = "Accessories", Price = 15m, StockQuantity = 300, Description = "White eraser", CreatedDate = seedDate },
                new Item { Id = 8, Name = "Sharpener", Category = "Accessories", Price = 25m, StockQuantity = 250, Description = "Metal sharpener", CreatedDate = seedDate },
                new Item { Id = 9, Name = "Ruler 30cm", Category = "Accessories", Price = 30m, StockQuantity = 200, Description = "30cm plastic ruler", CreatedDate = seedDate },
                new Item { Id = 10, Name = "Marker Set", Category = "Markers", Price = 200m, StockQuantity = 80, Description = "Set of 12 colored markers", CreatedDate = seedDate },
                new Item { Id = 11, Name = "Stapler", Category = "Office Supplies", Price = 250m, StockQuantity = 50, Description = "Standard stapler with staples", CreatedDate = seedDate },
                new Item { Id = 12, Name = "Highlighter Yellow", Category = "Markers", Price = 35m, StockQuantity = 200, Description = "Yellow highlighter", CreatedDate = seedDate },
                new Item { Id = 13, Name = "Highlighter Pink", Category = "Markers", Price = 35m, StockQuantity = 200, Description = "Pink highlighter", CreatedDate = seedDate },
                new Item { Id = 14, Name = "Scissors", Category = "Office Supplies", Price = 120m, StockQuantity = 100, Description = "Stainless steel scissors", CreatedDate = seedDate },
                new Item { Id = 15, Name = "Glue Stick", Category = "Adhesives", Price = 40m, StockQuantity = 180, Description = "40g glue stick", CreatedDate = seedDate }
            );
        }
    }
}
