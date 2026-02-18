using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Models.Generated;

[Index("SaleId", Name = "IX_SalePayments_SaleId")]
public partial class SalePayment
{
    [Key]
    public int Id { get; set; }

    public int SaleId { get; set; }

    public DateTime PaymentDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? ReferenceNo { get; set; }

    public string? Notes { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    [ForeignKey("SaleId")]
    [InverseProperty("SalePayments")]
    public virtual Sale Sale { get; set; } = null!;
}
