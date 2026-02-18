using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("BillId", Name = "IX_BillItems_BillId")]
[Index("ProductId", Name = "IX_BillItems_ProductId")]
public partial class BillItem
{
    [Key]
    public int Id { get; set; }

    public int BillId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Rate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [ForeignKey("BillId")]
    [InverseProperty("BillItems")]
    public virtual Bill Bill { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("BillItems")]
    public virtual Product Product { get; set; } = null!;
}
