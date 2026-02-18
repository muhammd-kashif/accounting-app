using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("SupplierId", Name = "IX_Bills_SupplierId")]
public partial class Bill
{
    [Key]
    public int Id { get; set; }

    public string BillNumber { get; set; } = null!;

    public int SupplierId { get; set; }

    public DateTime BillDate { get; set; }

    public DateTime DueDate { get; set; }

    public string Status { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PaidAmount { get; set; }

    public string Notes { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    [InverseProperty("Bill")]
    public virtual ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();

    [InverseProperty("Bill")]
    public virtual ICollection<BillPayment> BillPayments { get; set; } = new List<BillPayment>();

    [ForeignKey("SupplierId")]
    [InverseProperty("Bills")]
    public virtual Supplier Supplier { get; set; } = null!;
}
