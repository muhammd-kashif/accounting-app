using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("ProductId", Name = "IX_PurchaseItems_ProductId")]
[Index("PurchaseId", Name = "IX_PurchaseItems_PurchaseId")]
public partial class PurchaseItem
{
    [Key]
    public int Id { get; set; }

    public int PurchaseId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Rate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("PurchaseItems")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("PurchaseId")]
    [InverseProperty("PurchaseItems")]
    public virtual Purchase Purchase { get; set; } = null!;
}
