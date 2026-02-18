using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

public partial class Product
{
    [Key]
    public int Id { get; set; }

    public string ItemName { get; set; } = null!;

    public string ItemType { get; set; } = null!;

    [Column("SKU")]
    public string Sku { get; set; } = null!;

    public string Description { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PurchasePrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SalePrice { get; set; }

    public int StockQuantity { get; set; }

    public int ReorderLevel { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int UserId { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();

    [InverseProperty("Product")]
    public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}
