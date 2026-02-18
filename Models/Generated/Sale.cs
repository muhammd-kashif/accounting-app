using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("CustomerId", Name = "IX_Sales_CustomerId")]
public partial class Sale
{
    [Key]
    public int Id { get; set; }

    public string SaleNumber { get; set; } = null!;

    public int CustomerId { get; set; }

    public DateTime SaleDate { get; set; }

    public string PaymentType { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RemainingAmount { get; set; }

    public DateTime? PaymentDueDate { get; set; }

    public bool IsPaid { get; set; }

    public string? Notes { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Sales")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("Sale")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("Sale")]
    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    [InverseProperty("Sale")]
    public virtual ICollection<SalePayment> SalePayments { get; set; } = new List<SalePayment>();
}
