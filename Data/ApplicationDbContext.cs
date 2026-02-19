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
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<SalePayment> SalePayments { get; set; }
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
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Sale)
                .WithMany()
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalePayment>()
                .HasOne(sp => sp.Sale)
                .WithMany(s => s.Payments)
                .HasForeignKey(sp => sp.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalePayment>()
                .Property(sp => sp.Amount)
                .HasColumnType("decimal(18,2)");


        }
    }
}
