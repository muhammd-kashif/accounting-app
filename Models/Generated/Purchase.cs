using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("SupplierId", Name = "IX_Purchases_SupplierId")]
public partial class Purchase
{
    [Key]
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public DateTime Date { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string ReferenceNo { get; set; } = null!;

    public string Notes { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    [InverseProperty("Purchase")]
    public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

    [ForeignKey("SupplierId")]
    [InverseProperty("Purchases")]
    public virtual Supplier Supplier { get; set; } = null!;
}
