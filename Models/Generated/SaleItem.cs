using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("ItemId", Name = "IX_SaleItems_ItemId")]
[Index("SaleId", Name = "IX_SaleItems_SaleId")]
public partial class SaleItem
{
    [Key]
    public int Id { get; set; }

    public int SaleId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPrice { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("SaleItems")]
    public virtual Item Item { get; set; } = null!;

    [ForeignKey("SaleId")]
    [InverseProperty("SaleItems")]
    public virtual Sale Sale { get; set; } = null!;
}
