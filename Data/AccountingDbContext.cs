using System;
using System.Collections.Generic;
using AccountingApp.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Data;

public partial class AccountingDbContext : DbContext
{
    public AccountingDbContext()
    {
    }

    public AccountingDbContext(DbContextOptions<AccountingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillItem> BillItems { get; set; }

    public virtual DbSet<BillPayment> BillPayments { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Income> Incomes { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Purchase> Purchases { get; set; }

    public virtual DbSet<PurchaseItem> PurchaseItems { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleItem> SaleItems { get; set; }

    public virtual DbSet<SalePayment> SalePayments { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasOne(d => d.Supplier).WithMany(p => p.Bills).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BillItem>(entity =>
        {
            entity.HasOne(d => d.Product).WithMany(p => p.BillItems).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Income>(entity =>
        {
            entity.HasOne(d => d.Customer).WithMany(p => p.Incomes).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasOne(d => d.Supplier).WithMany(p => p.Purchases).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PurchaseItem>(entity =>
        {
            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseItems).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasOne(d => d.Customer).WithMany(p => p.Sales).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasOne(d => d.Item).WithMany(p => p.SaleItems).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.Property(e => e.Address).HasDefaultValue("");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
